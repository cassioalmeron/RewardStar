using RewardStart.Core;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using RewardStar.Api.Models;
using RewardStar.Api.Services;
using Serilog;

var folder = Environment.SpecialFolder.LocalApplicationData;
var path = Environment.GetFolderPath(folder);
path = Path.Combine(path, "RewardStar", "Logs");

if (!Directory.Exists(path))
    Directory.CreateDirectory(path);

path = Path.Combine(path, "log-.log");

Log.Logger = new LoggerConfiguration()
    .Filter.ByExcluding("StartsWith(SourceContext, 'Microsoft.')")
    .WriteTo.File(
        path: path,                             // file Path
        rollingInterval: RollingInterval.Day,   // Creates a new file per day
        retainedFileCountLimit: 7,              // Keeps just the 7 more recent files
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
    )
    .CreateLogger();

Log.Information("Starting the application...");

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog(Log.Logger);

// Configure JWT Settings
var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>();
builder.Services.AddSingleton(jwtSettings);

// Configure JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key))
    };
});

// Register JWT Service
builder.Services.AddScoped<JwtService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<RewardStartDbContext>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Use the CORS policy
app.UseCors("AllowAll");

// Add Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.Run();