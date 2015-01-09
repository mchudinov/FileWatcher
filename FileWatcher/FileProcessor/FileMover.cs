using System;
using System.Configuration;
using System.IO;
using log4net;

namespace FileWatcher.FileProcessor
{
    public class FileMover : IFilePostProcessor
    {
        string FolderPostProcess { get; set; }
        string FolderRoot  { get; set;}
        static readonly ILog log = LogManager.GetLogger(typeof(FileMover));


        public FileMover()
        {
            FolderPostProcess = ConfigurationManager.AppSettings["FolderPostProcess"];
            FolderRoot = ConfigurationManager.AppSettings["FolderRoot"];

            if (String.IsNullOrEmpty(FolderPostProcess) || FolderPostProcess.Equals(FolderRoot))
            {
                FolderPostProcess = Path.GetTempPath();
            }

            if (!Directory.Exists(FolderPostProcess))
            {
                log.Error("Error FileMover.FileMover(). Post process directory does not exist.");
                try
                {
                    Directory.CreateDirectory(FolderPostProcess);    
                }
                catch (Exception e)
                {
                    log.ErrorFormat("Error FileMover.FileMover(). Try to create post process folder. Message: {0}",e.Message);    
                    FolderPostProcess = Path.GetTempPath();
                }
            }
            log.InfoFormat("Postprocess directory: {0}", FolderPostProcess);
        }


        public void Process(string filename)
        {
            log.DebugFormat("FileMover.Process {0}",filename);
            string filenameonly = Path.GetFileName(filename);
            try
            {
                log.DebugFormat("FileMover:Move {0}", FolderPostProcess+'/'+filenameonly);
                File.Move(filename, FolderPostProcess+'/'+filenameonly);    
            }
            catch (Exception e)
            {
                log.Error("Error FileMover.Process(). Message: " + e.Message + " Delete file.");
                File.Delete(filename);
            }
        }
    }
}
