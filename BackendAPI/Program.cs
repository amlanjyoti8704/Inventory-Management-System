
using System.Text.Json;
using MySqlConnector;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Twilio; // ✅ Add this

var builder = WebApplication.CreateBuilder(args);

// ✅ Load configuration from appsettings.json
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

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

// ✅ Add configuration and controllers
builder.Services.AddControllers();
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

var app = builder.Build();

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

// ✅ Initialize Twilio after building the app
var config = app.Services.GetRequiredService<IConfiguration>();
string accountSid = config["Twilio:AccountSid"];
string authToken = config["Twilio:AuthToken"];
TwilioClient.Init(accountSid, authToken); // ✅ Twilio initialized

app.Run();