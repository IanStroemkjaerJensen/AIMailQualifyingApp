using Care.Web.Application;
using Care.Web.Infrastructure;
using RestSharp;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();



builder.Configuration.SetBasePath(Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)!)
                     .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                     .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true);

// TODO: Skal Api'en have en Bootstrapper.cs
builder.Services.AddApplication(builder.Configuration)
                .AddInfrastructure()
                .AddSingleton(new RestClient(new HttpClient { BaseAddress = new Uri(builder.Configuration["openAi:apiSettings:baseUrl"]!) }).AddDefaultHeader("api-key", builder.Configuration["openAi:apiSettings:apiKey"]!));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllers();

app.Run();
