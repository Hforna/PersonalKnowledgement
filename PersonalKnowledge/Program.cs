using Hangfire;
using PersonalKnowledge.Application;
using PersonalKnowledge.Middleware;
using PersonalKnowledge.Infrastructure;
using PersonalKnowledge.Workers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddRouting(d => d.LowercaseUrls = true);
builder.Services.AddRouting(d => d.LowercaseQueryStrings = true);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddWorker(builder.Configuration);

var app = builder.Build();

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

app.Run();