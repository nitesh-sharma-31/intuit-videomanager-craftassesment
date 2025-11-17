using Microsoft.EntityFrameworkCore;
using VideoManager.Data;
using VideoManager.Data.Repositories;
using VideoManager.Data.Services;
using VideoManager.Shared.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowDesktopApp", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Configure Database
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Data Source=VideoManager.db";

builder.Services.AddDbContext<VideoManagerDbContext>(options =>
    options.UseSqlite(connectionString));

// Register repositories
builder.Services.AddScoped<IVideoRepository, VideoRepository>();

// Register services
builder.Services.AddScoped<IVideoService, VideoService>();
builder.Services.AddSingleton<IStorageService>(sp => 
{
    var storagePath = builder.Configuration["StoragePath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "Storage");
    return new LocalStorageService(storagePath);
});

// Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add logging
builder.Services.AddLogging(config =>
{
    config.AddConsole();
    config.AddDebug();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowDesktopApp");

app.UseAuthorization();

app.MapControllers();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<VideoManagerDbContext>();
    dbContext.Database.EnsureCreated();
}

app.Run();
