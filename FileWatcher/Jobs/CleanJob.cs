using System;
using System.Configuration;
using Quartz;
using log4net;

namespace FileWatcher.Jobs
{
    class CleanJob : IJob
    {
        static readonly ILog log = LogManager.GetLogger(typeof(CleanJob));

        public void Execute(IJobExecutionContext context)
        {
            log.Info("Cleaner job started");

            int daysExpiredFiles = Int16.Parse(ConfigurationManager.AppSettings["ExpiryFileDays"]);
            string folderPostProcess = ConfigurationManager.AppSettings["FolderPostProcess"];
            string folderRoot = ConfigurationManager.AppSettings["FolderWatcherRoot"];

            var filecleaner = new FileCleaner();
            filecleaner.Clean(folderPostProcess, daysExpiredFiles);
            filecleaner.Clean(folderRoot, daysExpiredFiles);
        }
    }
}
