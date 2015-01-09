using System;
using log4net;

namespace FileWatcher.FileProcessor
{
    public class FileProcessor : IFileProcessor
    {
        static readonly ILog log = LogManager.GetLogger(typeof(FileProcessor));
        IFilePostProcessor PostProcessor { get; set;}


        public FileProcessor()
        {
            PostProcessor = new FileMover();
        }


        public void Process(dynamic parameters)
        {
            string filename = (string)parameters.FileName;
            Uri url = (Uri)parameters.Uri;

            PostProcessor.Process(filename);

            log.InfoFormat("Process file: {0}", filename);
        }
    }
}

