using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;

namespace TestWorker.Infrastructure
{
    public class QuartzJobRunner : IJob
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger _errorlogger;

        public QuartzJobRunner(IServiceProvider serviceProvider, ILoggerFactory loggerFactory)
        {
            _serviceProvider = serviceProvider;
            _errorlogger = loggerFactory.CreateLogger("errors");
        }

        public async Task Execute(IJobExecutionContext context)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var jobType = context.JobDetail.JobType;

                try
                {
                    var job = scope.ServiceProvider.GetRequiredService(jobType) as IJob;

                    await job.Execute(context);
                }
                catch (Exception ex)
                {
                    _errorlogger.LogError(ex, $"An error occurred during activating of the {jobType.Name}\n");
                    throw;
                }
            }
        }
    }
}
