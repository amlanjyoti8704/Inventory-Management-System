using System.Text.Json;
using MongoDB.Driver;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using DotNetEnv;

var builder = WebApplication.CreateBuilder(args);

// Load .env file
DotNetEnv.Env.Load();

// Build MongoDB settings from environment variables
string mongoConnectionString = Environment.GetEnvironmentVariable("MONGO_CONNECTION_STRING") ?? "mongodb://localhost:27017";
string mongoDatabaseName = Environment.GetEnvironmentVariable("MONGO_DATABASE_NAME") ?? "Inventory_management";

// Set config values from env
builder.Configuration["MongoDB:ConnectionString"] = mongoConnectionString;
builder.Configuration["MongoDB:DatabaseName"] = mongoDatabaseName;
builder.Configuration["Twilio:AccountSid"] = Environment.GetEnvironmentVariable("TWILIO_ACCOUNT_SID");
builder.Configuration["Twilio:AuthToken"] = Environment.GetEnvironmentVariable("TWILIO_AUTH_TOKEN");

// Register MongoDB client and database as singletons
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    return new MongoClient(mongoConnectionString);
});

builder.Services.AddSingleton<IMongoDatabase>(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    return client.GetDatabase(mongoDatabaseName);
});

builder.Services.AddSingleton<MongoDbContext>();

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

// Initialize Twilio with values from .env
var config = app.Services.GetRequiredService<IConfiguration>();
// TwilioClient.Init(config["Twilio:AccountSid"], config["Twilio:AuthToken"]);

app.Run();