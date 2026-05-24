using CloudShift.Application;
using CloudShift.Domain.Entities;
using CloudShift.Infrastructure;
using CloudShift.Infrastructure.Data;
using MassTransit;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ─── Application Layer ────────────────────────────────────────────────────────
// Registers MediatR and all handlers discovered in CloudShift.Application
builder.Services.AddApplicationServices();

// ─── Infrastructure Layer ─────────────────────────────────────────────────────
// Registers EF Core DbContext + all repository implementations
builder.Services.AddInfrastructureServices(builder.Configuration);

// ─── MassTransit / RabbitMQ ──────────────────────────────────────────────────
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

// ─── ASP.NET Core ────────────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title   = "CloudShift API",
        Version = "v1",
        Description = "Cloud-to-cloud migration platform API"
    });
});

// Allow Angular dev server (port 4200) during development
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularDev", policy =>
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

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

// ─── HTTP Pipeline ────────────────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "CloudShift API v1"));
}

app.UseHttpsRedirection();
app.UseCors("AllowAngularDev");
app.UseAuthorization();
app.MapControllers();

app.Run();
