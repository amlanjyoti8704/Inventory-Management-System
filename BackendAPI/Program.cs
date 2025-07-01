using System.Text.Json;
using MySqlConnector;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
// using Twilio;
using DotNetEnv; // ✅ Import DotNetEnv

var builder = WebApplication.CreateBuilder(args);

// ✅ Load .env file (this should be in your BackendAPI folder)
DotNetEnv.Env.Load();

// ✅ Build secure connection string from environment variables
string dbConnection = $"Server={Environment.GetEnvironmentVariable("DB_SERVER")};" +
                      $"Port={Environment.GetEnvironmentVariable("DB_PORT")};" +
                      $"Database={Environment.GetEnvironmentVariable("DB_NAME")};" +
                      $"Uid={Environment.GetEnvironmentVariable("DB_USER")};" +
                      $"Pwd={Environment.GetEnvironmentVariable("DB_PASSWORD")};";

// ✅ Set config values from env manually
builder.Configuration["ConnectionStrings:DefaultConnection"] = dbConnection;
builder.Configuration["Twilio:AccountSid"] = Environment.GetEnvironmentVariable("TWILIO_ACCOUNT_SID");
builder.Configuration["Twilio:AuthToken"] = Environment.GetEnvironmentVariable("TWILIO_AUTH_TOKEN");

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
});

// Add controllers and config
builder.Services.AddControllers();
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

var app = builder.Build();

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

// ✅ Initialize Twilio with values from .env
var config = app.Services.GetRequiredService<IConfiguration>();
// TwilioClient.Init(config["Twilio:AccountSid"], config["Twilio:AuthToken"]);

app.Run();