using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;
using TestWorker.Infrastructure;
using TestWorker.Jobs;

namespace TestWorker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseWindowsService()
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<QuartzScheduler>();

                    // Add Quartz services
                    services.AddSingleton<QuartzJobRunner>();
                    services.AddSingleton<IJobFactory, JobFactory>();
                    services.AddSingleton<ISchedulerFactory, StdSchedulerFactory>();

                    AddJobs(services);
                });

        public static IServiceCollection AddJobs(IServiceCollection services)
        {
            var configuration = GetConfiguration(services);
            var jobTypes = new List<Type>
            {
                typeof(TestJob),
            };

            jobTypes.ForEach(type =>
            {
                services.AddScoped(type);

                services.AddSingleton(new JobSchedule(
                jobType: type,
                cronExpression: configuration[$"{type.Name}CronExpression"]));
            });

            return services;
        }

        private static IConfiguration GetConfiguration(IServiceCollection services)
        {
            IConfiguration configuration;

            using (var serviceProvider = services.BuildServiceProvider())
            {
                configuration = serviceProvider.GetService<IConfiguration>();
            }

            return configuration;
        }
    }
}
