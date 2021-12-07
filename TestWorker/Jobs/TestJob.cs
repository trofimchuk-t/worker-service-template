using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Quartz;

namespace TestWorker.Jobs
{
    public class TestJob : IJob
    {
        private readonly ILogger logger;

        public TestJob(ILoggerFactory loggerFactory)
        {
            logger = loggerFactory.CreateLogger<TestJob>();
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var currentDate = DateTime.UtcNow;
            logger.LogInformation($"{nameof(TestJob)} started at {currentDate}");

            await Task.Delay(5000);

            logger.LogInformation($"{nameof(TestJob)} finished at {currentDate}");
        }
    }
}
