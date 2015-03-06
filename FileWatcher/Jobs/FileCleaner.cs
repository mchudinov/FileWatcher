using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using log4net;

namespace FileWatcher.Jobs
{
    public class FileCleaner
    {
        static readonly ILog log = LogManager.GetLogger(typeof(FileCleaner));

        public void Clean(string folder, int days)
        {
            DateTime expireDate = DateTime.Now.AddDays(-days);

            try
            {
                var fileInfos = new DirectoryInfo(folder).GetFiles("*", SearchOption.AllDirectories)
                    .Where(fi => fi.CreationTime < expireDate);

                fileInfos.All(fi => {CleanInternal(fi.FullName); return true;});    
            }
            catch (Exception ex)
            {
                log.Error("Error cleaning files: "+  ex.Message);
            }
        }

        void CleanInternal(string filename)
        {
            log.DebugFormat("FileCleaner file {0}", filename);
            File.Delete(filename);
        }
    }
}

