using CloudShift.Application.Common.Interfaces;
using CloudShift.Infrastructure.Data;
using CloudShift.Infrastructure.Logging;
using CloudShift.Worker;
using CloudShift.Worker.Consumers;
using CloudShift.Worker.Storage;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;

var builder = Host.CreateApplicationBuilder(args);
var logFilePath = RunLogFile.Prepare(builder.Environment.ContentRootPath, truncateExisting: false);

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "CloudShift.Worker")
    .WriteTo.File(
        logFilePath,
        shared: true,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] [{Application}] [{SourceContext}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

builder.Services.AddSerilog();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ICloudStorageProvider, NoopCloudStorageProvider>();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<MigrationJobConsumer>();

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

builder.Services.AddHostedService<Worker>();

try
{
    var host = builder.Build();

    Log.Information(
        "CloudShift Worker starting. Environment: {EnvironmentName}, LogFilePath: {LogFilePath}",
        builder.Environment.EnvironmentName,
        logFilePath);

    host.Run();
}
catch (Exception ex) when (ex is not OperationCanceledException)
{
    Log.Fatal(ex, "CloudShift Worker terminated unexpectedly.");
    throw;
}
finally
{
    Log.Information("CloudShift Worker stopped.");
    await Log.CloseAndFlushAsync();
}
