using AppEngine.DataAccess;
using AppEngine.DependencyInjection;

using ClubEngine.MigrationService;

using Microsoft.EntityFrameworkCore;

var builder = Host.CreateApplicationBuilder(args);
builder.AddServiceDefaults();

builder.Services.AddHostedService<Worker>();

builder.Services.AddOpenTelemetry()
                .WithTracing(tracing => tracing.AddSource(Worker.ActivitySourceName));

builder.Services.AddSingleton(new AppAssemblies([typeof(AppDbContext).Assembly, typeof(ClubEngine.ApiService.HomeController).Assembly]));
builder.Services.AddDbContext<DbContext, AppDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("database"),
                                                                                       b => b.MigrationsAssembly("ClubEngine.ApiService")));

var host = builder.Build();
host.Run();
