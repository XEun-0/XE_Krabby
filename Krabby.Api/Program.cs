using Krabby.Core.Models;
using Krabby.Core.Services.AniDB;
using Krabby.Persistence;
using Microsoft.AspNetCore.Http.Timeouts;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Services setup
builder.Services.AddControllers();

builder.Services.AddSingleton<AniDbSession>();
builder.Services.AddSingleton<AniDbService>();
builder.Services.AddSingleton<AniDbRateLimiter>();
builder.Services.AddSingleton<AniDbJobStore>();

builder.Services.AddDbContext<KrabbyDbContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("Default")
    );
});

builder.Services.AddRequestTimeouts(options =>
{
    options.DefaultPolicy = new RequestTimeoutPolicy
    {
        Timeout = TimeSpan.FromMinutes(10)
    };
});

builder.Services.Configure<AniDbAuth>(builder.Configuration.GetSection("AniDb"));

var app = builder.Build();

using (IServiceScope scope = app.Services.CreateScope())
{
    var service = scope.ServiceProvider.GetRequiredService<AniDbService>();
    var jobStore  = scope.ServiceProvider.GetRequiredService<AniDbJobStore>();

    Console.WriteLine("[AniDbService] Created");

    await service.InitializeAsync();
}

// Middleware setup
app.MapControllers();
app.UseRequestTimeouts();

// Blocks until shutdown
app.Run();


// Should not get here during normal operation
// until shut down.
Console.WriteLine("Logging out");
