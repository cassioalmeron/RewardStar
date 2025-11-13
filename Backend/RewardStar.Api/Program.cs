using RewardStart.Core;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

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

// Run database migrations automatically
using (var scope = app.Services.CreateScope())
{
    try
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<RewardStartDbContext>();
        Console.WriteLine("Running database migrations...");
        dbContext.Database.Migrate();
        Console.WriteLine("Database migrations completed successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error running migrations: {ex.Message}");
        throw;
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Use the CORS policy
app.UseCors("AllowAll");

app.UseAuthorization();
app.MapControllers();
app.Run();