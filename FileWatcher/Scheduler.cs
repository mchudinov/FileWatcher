using System;
using Quartz;
using log4net;
using Quartz.Impl;
using System.Configuration;

namespace FileWatcher
{
    public class Scheduler
    {
        static IScheduler _scheduler;
        static readonly ILog log = LogManager.GetLogger(typeof(FileWatcher.Scheduler));

        public void Start()
        {
            ISchedulerFactory factory = new StdSchedulerFactory();
            _scheduler = factory.GetScheduler();
            _scheduler.Start();
            StartCleanJob();
            StartFileProcessingJob();
        }

        public void Shutdown()
        {
            if (null != _scheduler)
                _scheduler.Shutdown();
        }

        public void StartCleanJob()
        {
            var hours = Int16.Parse(ConfigurationManager.AppSettings["CleanJobHours"]);
            log.InfoFormat("Start Clean job. Execute once in {0} hours", hours);

            IJobDetail job = JobBuilder.Create<Jobs.CleanJob>()
                .WithIdentity("CleanJob", "group1")
                .Build();

            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity("CleanJobTrigger", "group1")
                .StartNow()
                .WithSimpleSchedule(x => x.WithIntervalInHours(hours).RepeatForever())
                .Build();

            _scheduler.ScheduleJob(job, trigger);
        }


        public void StartFileProcessingJob()
        {
            var minutes = Int16.Parse(ConfigurationManager.AppSettings["FileProcessingJobMinutes"]);
            log.InfoFormat("Start FileProcessing job. Execute once in {0} minutes", minutes);

            IJobDetail job = JobBuilder.Create<Jobs.FileProcessingJob>()
                .WithIdentity("FileProcessing", "group1")
                .Build();

            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity("FileProcessingTrigger", "group1")
                .StartNow()
                .WithSimpleSchedule(x => x.WithIntervalInMinutes(minutes).RepeatForever())
                .Build();

            _scheduler.ScheduleJob(job, trigger);
        }
    }
}

