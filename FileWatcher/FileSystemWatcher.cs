﻿using System.IO;
using log4net;
using System.Threading;
using System.Reflection;
using FileWatcher.FileProcessor;

namespace FileWatcher
{
    public class FileSystemWatcher
    {
        static readonly ILog log = LogManager.GetLogger(typeof(FileWatcher.Service));
        static System.IO.FileSystemWatcher _fileSystemWatcher;
        public static IFileProcessor _fileProcessor;


        public FileSystemWatcher()
        {
            _fileProcessor = Service.FileProcessor;
        }


        static void OnCreated(object sender, FileSystemEventArgs e)
        {
            log.InfoFormat("New file ({0}): {1} | {2}", MethodInfo.GetCurrentMethod().Name, e.ChangeType, e.FullPath);
            ThreadPool.QueueUserWorkItem(_fileProcessor.Process, new {FileName=e.FullPath, Uri=Service.UriApi});
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


        public void Start(string folderRoot)
        {
            _fileSystemWatcher = new System.IO.FileSystemWatcher(folderRoot);
            _fileSystemWatcher.Filter = "*";
            _fileSystemWatcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            _fileSystemWatcher.InternalBufferSize = 1024 * 64; //64k buffer
            _fileSystemWatcher.Created += new FileSystemEventHandler(OnCreated);
            _fileSystemWatcher.Error += new ErrorEventHandler(OnError);
            _fileSystemWatcher.IncludeSubdirectories = true;
            _fileSystemWatcher.EnableRaisingEvents = true;
            log.InfoFormat("Watching directory: {0}", folderRoot);
        }
    }
}

