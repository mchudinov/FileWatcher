using System;
using Quartz;
using log4net;
using FileWatcher.FileProcessor;
using System.IO;
using System.Linq;

namespace FileWatcher.Jobs
{
    public class FileProcessingJob : IJob
    {
        static readonly ILog log = LogManager.GetLogger(typeof(FileProcessingJob));
        string FolderWatcherRoot { get; set;}
        Uri UriApi { get; set; }
        readonly IFileProcessor FileProcessor; 


        public FileProcessingJob()
        {
            FolderWatcherRoot = Service.FolderWatcherRoot;
            UriApi = Service.UriApi;
            FileProcessor = Service.FileProcessor;
        }


        public void Execute(IJobExecutionContext context)
        {
            log.Info("FileProcessing job started");

            DateTime processTime = DateTime.Now.AddMinutes(-5);

            var fileInfos = new DirectoryInfo(FolderWatcherRoot).GetFiles("*", SearchOption.AllDirectories)
                .Where(fi => fi.CreationTime < processTime);

            fileInfos.All(fi => {ExecuteInternal(fi.FullName); return true;});
        }


        void ExecuteInternal(string filename)
        {
            log.DebugFormat("FileProcessingJob.FileProcessing file {0}", filename);
            FileProcessor.Process(new {FileName = filename, Uri = UriApi});
        }
    }
}

