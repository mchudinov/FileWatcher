using System;
using log4net;
using System.ServiceProcess;
using Quartz;
using Quartz.Impl;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using FileWatcher.FileProcessor;

namespace FileWatcher
{
    public class Service : ServiceBase
    {
        public static readonly IFileProcessor FileProcessor = BuildFileProcessor(); 
        static readonly ILog log = LogManager.GetLogger(typeof(FileWatcher.Service));
        static IScheduler Scheduler { get; set; }
        static FileSystemWatcher FileSystemWatcher { get; set;}
        public static Uri UriApi { get; private set; }
        public static string FolderWatcherRoot  { get; private set;}

        public Service() {}

        protected override void OnStart(string[] args)
        {
            log.Info("Service starting");
            bool development = (args.Length > 0 && null !=args[0] && (args[0].ToLower().Equals("dev") || args[0].ToLower().Equals("devel")));
            log.InfoFormat("Developement mode is {0}",development?"On":"Off");

            FolderWatcherRoot = ConfigurationManager.AppSettings["FolderWatcherRoot"];
            if (!Directory.Exists(FolderWatcherRoot))
            {
                log.Error("File system watcher root directory does not exist.");
                log.Error("Exit program.");
                Stop();
            }

            UriApi = new Uri(ConfigurationManager.AppSettings["UriProd"]);
            if (development) {
                UriApi = new Uri(ConfigurationManager.AppSettings["UriDevel"]);
            }
            log.InfoFormat("Api url {0}",UriApi.ToString());

            StartFileSystemWatcher(FolderWatcherRoot);
            StartScheduler();
            StartCleanJob();
            StartFileProcessingJob();
        }

        protected override void OnStop()
        {
            log.Info("Service shutting down");
            Scheduler.Shutdown();
        }

        void StartScheduler()
        {
            ISchedulerFactory schedFact = new StdSchedulerFactory();
            Scheduler = schedFact.GetScheduler();
            Scheduler.Start();
        }

        void StartCleanJob()
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

            Scheduler.ScheduleJob(job, trigger);
        }


        void StartFileProcessingJob()
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

            Scheduler.ScheduleJob(job, trigger);
        }


        static void StartFileSystemWatcher(string folderRoot)
        {
            FileSystemWatcher = new FileSystemWatcher(folderRoot);
            FileSystemWatcher.Filter = "*";
            FileSystemWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            FileSystemWatcher.InternalBufferSize = 1024 * 64; //64k buffer
            FileSystemWatcher.Created += new FileSystemEventHandler(OnCreated);
            FileSystemWatcher.Error += new ErrorEventHandler(OnError);
            FileSystemWatcher.IncludeSubdirectories = true;
            FileSystemWatcher.EnableRaisingEvents = true;
            log.InfoFormat("Watching directory: {0}", folderRoot);
        }

        static void OnCreated(object sender, FileSystemEventArgs e)
        {
            log.InfoFormat("New file ({0}): {1} | {2}", MethodInfo.GetCurrentMethod().Name, e.ChangeType, e.FullPath);
            ThreadPool.QueueUserWorkItem(FileProcessor.Process, new {FileName=e.FullPath, Uri=UriApi});
        }

        static void OnError(object sender, ErrorEventArgs e)
        {
            log.ErrorFormat("({0}): {1}", MethodInfo.GetCurrentMethod().Name, e.GetException().Message);
            if (e.GetException().GetType() == typeof(InternalBufferOverflowException))
            {
                //  This can happen if OS is reporting many file system events quickly 
                //  and internal buffer of the  FileSystemWatcher is not large enough to handle this
                //  rate of events. The InternalBufferOverflowException error informs the application
                //  that some of the file system events are being lost.
                log.InfoFormat(("The file system watcher experienced an internal buffer overflow: " + e.GetException().Message));
            }
        }


        public static IFileProcessor BuildFileProcessor()
        {
            return new FileProcessor.FileProcessor();
        }


        #if DEBUG
        // This method is for debugging of OnStart() method only.
        // Switch to Debug config, set a breakpoint here and a breakpoint in OnStart()
        // How to: Debug the OnStart Method http://msdn.microsoft.com/en-us/library/cktt23yw.aspx
        // How to: Debug Windows Service Applications http://msdn.microsoft.com/en-us/library/7a50syb3%28v=vs.110%29.aspx
        public static void Main(String[] args)
        {      
            (new FileWatcher.Service()).OnStart(new string[1]);
            ServiceBase.Run( new FileWatcher.Service() );
        }
        #endif
    }
}

