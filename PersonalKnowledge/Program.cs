using Hangfire;
using PersonalKnowledge.Application;
using PersonalKnowledge.Middleware;
using PersonalKnowledge.Infrastructure;
using PersonalKnowledge.Workers;

using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Personal Knowledge API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});
builder.Services.AddRouting(d => d.LowercaseUrls = true);
builder.Services.AddRouting(d => d.LowercaseQueryStrings = true);

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
    // Clear known proxies and networks to allow ngrok (or any proxy) to be trusted in dev/limited environments.
    // For production, you'd typically specify known proxy IP addresses.
    options.KnownProxies.Clear();
    options.KnownNetworks.Clear();
});

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddWorker(builder.Configuration);

builder.Services.AddMemoryCache();

var app = builder.Build();

app.UseForwardedHeaders();

// Configure the HTTP request pipeline.
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

// Cleanup Hangfire jobs before the application fully starts to ensure no pending jobs from previous sessions remain.
// This handles the "pending jobs when i start" requirement.
try 
{
    var monitor = JobStorage.Current.GetMonitoringApi();
    var queues = monitor.Queues();

    foreach (var queue in queues)
    {
        var jobs = monitor.EnqueuedJobs(queue.Name, 0, int.MaxValue);
        foreach (var job in jobs)
        {
            BackgroundJob.Delete(job.Key);
        }
    }

    // Also clear scheduled and processing jobs to ensure a truly clean slate.
    var scheduledJobs = monitor.ScheduledJobs(0, int.MaxValue);
    foreach (var job in scheduledJobs)
    {
        BackgroundJob.Delete(job.Key);
    }

    var processingJobs = monitor.ProcessingJobs(0, int.MaxValue);
    foreach (var job in processingJobs)
    {
        BackgroundJob.Delete(job.Key);
    }
}
catch (Exception ex)
{
    Console.WriteLine($"[WARNING] Could not clear Hangfire jobs on startup: {ex.Message}");
}

app.Use(async (context, next) =>
{
    context.Request.EnableBuffering();
    await next();
});

app.Run();