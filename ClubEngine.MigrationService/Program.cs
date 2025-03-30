using ClubEngine.ApiService.Infrastructure;
using ClubEngine.MigrationService;

var builder = Host.CreateApplicationBuilder(args);
builder.AddServiceDefaults();

builder.Services.AddHostedService<Worker>();

builder.Services.AddOpenTelemetry()
                .WithTracing(tracing => tracing.AddSource(Worker.ActivitySourceName));

builder.AddSqlServerDbContext<ClubEngineDbContext>("database");

var host = builder.Build();
host.Run();
