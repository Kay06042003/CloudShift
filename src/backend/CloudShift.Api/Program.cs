using CloudShift.Application;
using CloudShift.Domain.Entities;
using CloudShift.Infrastructure;
using CloudShift.Infrastructure.Data;
using CloudShift.Infrastructure.Logging;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);
var logFilePath = RunLogFile.Prepare(builder.Environment.ContentRootPath, truncateExisting: true);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "CloudShift.Api")
    .WriteTo.File(
        logFilePath,
        shared: true,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{Application}] [{SourceContext}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });

        cfg.ConfigureEndpoints(context);
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "CloudShift API",
        Version = "v1",
        Description = "Cloud-to-cloud migration platform API"
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularDev", policy =>
        policy.WithOrigins("http://localhost:4200", "http://127.0.0.1:4200")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

try
{
    var app = builder.Build();

    Log.Information(
        "CloudShift API starting. Environment: {EnvironmentName}, LogFilePath: {LogFilePath}",
        app.Environment.EnvironmentName,
        logFilePath);

    if (app.Environment.IsDevelopment())
    {
        await using var scope = app.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.MigrateAsync();

        var demoUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        if (!await dbContext.Users.AnyAsync(u => u.Id == demoUserId))
        {
            dbContext.Users.Add(new User
            {
                Id = demoUserId,
                Email = "demo@cloudshift.local",
                FirstName = "CloudShift",
                LastName = "Demo",
                CreatedAt = DateTime.UtcNow
            });

            await dbContext.SaveChangesAsync();
        }
    }

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "CloudShift API v1"));
    }

    app.UseSerilogRequestLogging();
    if (!app.Environment.IsDevelopment())
    {
        app.UseHttpsRedirection();
    }

    app.UseCors("AllowAngularDev");
    app.UseAuthorization();
    app.MapControllers();

    app.Run();
}
catch (Exception ex) when (ex is not OperationCanceledException)
{
    Log.Fatal(ex, "CloudShift API terminated unexpectedly.");
    throw;
}
finally
{
    Log.Information("CloudShift API stopped.");
    await Log.CloseAndFlushAsync();
}
