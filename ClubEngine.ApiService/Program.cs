using AppEngine;
using AppEngine.ErrorHandling;
using AppEngine.ServiceBus;

using ClubEngine.ApiService;
using ClubEngine.ApiService.Properties;

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

builder.Services.AddAntiforgery();
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
builder.AddAppEngine([typeof(Program).Assembly], [Resources.ResourceManager]);

foreach (var cs in builder.Configuration.GetSection("ConnectionString").AsEnumerable())
{
    Console.WriteLine($"{cs.Key}: {cs.Value?[..5]}");
}

var app = builder.Build();
//app.Services.GetService()
// Configure the HTTP request pipeline.
//app.UseExceptionHandler();
app.UseDeveloperExceptionPage();
app.UseMiddleware<ExceptionMiddleware>();
app.UseHttpsRedirection();
app.UseRouting();

app.UseCors(corsBuilder => corsBuilder.AllowAnyOrigin()
                                      .AllowAnyHeader()
                                      .AllowAnyMethod());

app.UseAuthentication();
app.UseAuthorization();

//app.UseAntiforgery();

app.MapAppEngineEndpoints();


//app.UseEndpoints(endpoints =>
//{
//    endpoints.MapAppEngineEndpoints(app.Services);
//});

app.MapGet("/",
           () =>
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

app.Services.GetService<MessageQueueReceiver>()!.StartReceiveLoop();

app.Run();