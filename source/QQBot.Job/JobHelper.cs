using QQBot.Entities.Model;
using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Text;
using System.Threading.Tasks;

namespace QQBot.Job
{
    public static class JobHelper
    {
        public static async Task CreateJob(this QQBotTask taskInfo)
        {
            if (!taskInfo.Enable || !CronExpression.IsValidExpression(taskInfo.Cron))
            {
                return;
            }
            NameValueCollection props = new NameValueCollection
            {
                {"quartz.serializer.type", "binary"}
            };
            StdSchedulerFactory factory = new StdSchedulerFactory(props);
            IScheduler scheduler = await factory.GetScheduler();
            await scheduler.Start();
            string jobId = taskInfo.Id;
            var jobGroupName = taskInfo.FileName;
            var jobTriggerName = $"{jobId}_Trigger";
            var jobDataMap = new JobDataMap();
            jobDataMap.Put("TaskInfo", taskInfo);
            IJobDetail job = JobBuilder.Create<QQBotTaskJob>()
                .SetJobData(jobDataMap)
                .WithIdentity(jobId, jobGroupName)
                .Build();
            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity(jobTriggerName, jobGroupName)
                .StartNow()
                .WithCronSchedule(taskInfo.Cron)
                .Build();
            await scheduler.ScheduleJob(job, trigger);
        }

        public static async Task DeleteJob(this QQBotTask taskInfo)
        {
            NameValueCollection props = new NameValueCollection { { "quartz.serializer.type", "binary" } };
            StdSchedulerFactory factory = new StdSchedulerFactory(props);
            IScheduler scheduler = await factory.GetScheduler();
            var jobKey = new JobKey(taskInfo.Id, taskInfo.FileName);
            await scheduler.PauseJob(jobKey);
            await scheduler.UnscheduleJob(new TriggerKey($"{taskInfo.Id}_Trigger"));
            var t = await scheduler.DeleteJob(jobKey);
        }
    }
}
