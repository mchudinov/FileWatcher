using System;
using log4net;
using System.ServiceProcess;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using FileWatcher.FileProcessor;

namespace FileWatcher
{
    public class Service : ServiceBase
    {
        static readonly ILog log = LogManager.GetLogger(typeof(FileWatcher.Service));
        public static Uri UriApi { get; private set; }
        public static string FolderWatcherRoot  { get; private set;}
        FileWatcher.Scheduler _scheduler;
        FileWatcher.FileSystemWatcher _fileSystemWatcher;
        public static IFileProcessor FileProcessor {get; private set;}

        public Service() 
        {
            FileProcessor = new FileProcessor.FileProcessor();
            _scheduler = new FileWatcher.Scheduler();
            _fileSystemWatcher = new FileWatcher.FileSystemWatcher();
        }

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

            _fileSystemWatcher.Start(FolderWatcherRoot);
            _scheduler.Start();
        }


        protected override void OnStop()
        {
            log.Info("Service shutting down");
            _scheduler.Shutdown();
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

