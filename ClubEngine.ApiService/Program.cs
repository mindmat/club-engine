using AppEngine;
using AppEngine.ErrorHandling;

using ClubEngine.ApiService;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();

builder.Services.AddControllers();

builder.Services.AddSignalR();
builder.Services.AddCors();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddOpenApiDocument();
//settings =>
//{
    //settings.DocumentName = "swagger";
    //settings.PostProcess = document =>
    //{
    //    document.Info.Version = "v1";
    //    document.Info.Title = "Example API";
    //    document.Info.Description = "REST API for example.";
    //};
//});

//builder.Services.AddControllers();
builder.Services.AddScoped<HomeController>();
builder.AddAppEngine([typeof(Program).Assembly]);

var app = builder.Build();
//app.Services.GetService()
// Configure the HTTP request pipeline.
//app.UseExceptionHandler();
app.UseMiddleware<ExceptionMiddleware>();


string[] summaries = ["Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"];

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.MapGet("/hello", () => "Hello, World!")
   .WithSummary("Get a greeting")
   .WithDescription("This endpoint returns a friendly greeting.");

app.MapAppEngineEndpoints();



//app.UseRouting();

app.UseCors(corsBuilder => corsBuilder.AllowAnyOrigin()
                                      .AllowAnyHeader()
                                      .AllowAnyMethod());

//app.UseEndpoints(endpoints =>
//{
//    endpoints.MapAppEngineEndpoints(app.Services);
//});

app.MapGet("/", () =>
{
    var scope = app.Services.CreateScope();
    return scope.ServiceProvider.GetService<HomeController>()!.Index();
}).AllowAnonymous();

app.MapDefaultEndpoints();

if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    app.MapOpenApi();
    app.UseSwaggerUI();
}

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
