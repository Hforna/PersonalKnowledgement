using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Hangfire;

namespace PersonalKnowledge.Workers;

public static class WorkerConfiguration
{
    public static void AddWorker(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHangfire(d => 
        {
            d.UseSqlServerStorage(configuration.GetConnectionString("sqlserver"), new Hangfire.SqlServer.SqlServerStorageOptions
            {
                InvisibilityTimeout = TimeSpan.FromHours(1)
            });
            d.UseFilter(new AutomaticRetryAttribute { Attempts = 0 });
        });
        
        services.AddHangfireServer();
        
        AddJobs();       
    }

    static void AddJobs()
    {
        
    }
}