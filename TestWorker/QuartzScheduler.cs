using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Quartz;
using Quartz.Spi;
using TestWorker.Infrastructure;

namespace TestWorker
{
    public class QuartzScheduler : IHostedService
    {
        private readonly ISchedulerFactory _schedulerFactory;
        private readonly IJobFactory _jobFactory;
        private readonly IEnumerable<JobSchedule> _jobSchedules;
        private readonly ILogger _informationlogger;

        public QuartzScheduler(
            ISchedulerFactory schedulerFactory,
            IJobFactory jobFactory,
            IEnumerable<JobSchedule> jobSchedules,
            ILoggerFactory loggerFactory)
        {
            _schedulerFactory = schedulerFactory;
            _jobFactory = jobFactory;
            _jobSchedules = jobSchedules;
            _informationlogger = loggerFactory.CreateLogger("application");
        }

        public IScheduler Scheduler { get; set; }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            Scheduler = await _schedulerFactory.GetScheduler(cancellationToken);
            Scheduler.JobFactory = _jobFactory;

            foreach (var jobSchedule in _jobSchedules)
            {
                var job = CreateJob(jobSchedule);
                var trigger = CreateTrigger(jobSchedule);

                ComputeFireTimes(job, trigger);

                await Scheduler.ScheduleJob(job, trigger, cancellationToken);
            }

            await Scheduler.Start(cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Scheduler?.Shutdown(cancellationToken);
        }

        private static IJobDetail CreateJob(JobSchedule schedule)
        {
            var jobType = schedule.JobType;
            return JobBuilder
                .Create(jobType)
                .WithIdentity(jobType.FullName)
                .WithDescription(jobType.Name)
                .Build();
        }

        private static ITrigger CreateTrigger(JobSchedule schedule)
        {
            return TriggerBuilder
                .Create()
                .WithIdentity($"{schedule.JobType.FullName}.trigger")
                .WithCronSchedule(schedule.CronExpression, cron => cron.InTimeZone(TimeZoneInfo.Utc))
                //.StartNow() // To be able to run job immediately (e.g. for debugging), uncomment this line and comment line above
                .WithDescription(schedule.CronExpression)
                .Build();
        }

        private void ComputeFireTimes(IJobDetail job, ITrigger trigger)
        {
            _informationlogger.LogInformation($"Next 5 occurrences of the {job.Description}:");

            var times = TriggerUtils.ComputeFireTimes(trigger as IOperableTrigger, null, 5);
            foreach (var time in times)
            {
                _informationlogger.LogInformation($"- {time.UtcDateTime}");
            }
        }
    }
}
