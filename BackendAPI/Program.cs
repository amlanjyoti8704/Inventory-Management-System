using DotNetEnv;
using BackendAPI.Data;
using BackendAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// Load .env file
Env.Load();

// Read PostgreSQL connection string from environment
string connectionString = Environment.GetEnvironmentVariable("POSTGRES_CONNECTION_STRING")
    ?? "Host=localhost;Port=5432;Database=inventory_management;Username=postgres;Password=postgres";

// Set Twilio config from env
builder.Configuration["Twilio:AccountSid"] = Environment.GetEnvironmentVariable("TWILIO_ACCOUNT_SID");
builder.Configuration["Twilio:AuthToken"] = Environment.GetEnvironmentVariable("TWILIO_AUTH_TOKEN");

// Register the database helper as a singleton (holds connection string, creates connections on demand)
builder.Services.AddSingleton(new DbHelper(connectionString));

// Register services (business logic layer)
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<CategoryService>();
builder.Services.AddScoped<ItemService>();
builder.Services.AddScoped<PurchaseService>();
builder.Services.AddScoped<IssueService>();
builder.Services.AddScoped<AlertService>();
builder.Services.AddScoped<DashboardService>();

// CORS — allow all origins for development
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddControllers();
builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

var app = builder.Build();

app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

app.Run();