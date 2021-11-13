using Quartz;
using System;

namespace QQBot.Job
{
    public static class ServiceCollectionQuartzConfiguratorExtensions
    {
        public static void AddJobAndTrigger<T>(
            this IServiceCollectionQuartzConfigurator quartz,
            string cron)
            where T : IJob
        {
            if (string.IsNullOrEmpty(cron) || !CronExpression.IsValidExpression(cron))
            {
                Console.WriteLine("Cookie定时检查Cron表达式错误，将无法自动检查Cookie");
                return;
            }

            string jobName = typeof(T).Name;
            var configKey = $"Quartzs:{jobName}";

            var jobKey = new JobKey(jobName);
            quartz.AddJob<T>(opts => opts.WithIdentity(jobKey));

            quartz.AddTrigger(opts => opts
                .ForJob(jobKey)
                .WithIdentity(jobName + "-trigger")
                .WithCronSchedule(cron));
        }
    }
}
