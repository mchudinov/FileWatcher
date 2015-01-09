using System;
using System.ServiceProcess;

namespace FileWatcher
{
    class Program
    {
        #if (DEBUG != true)
        public static void Main(string[] args)
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[] { new FileWatcher.Service() };
            ServiceBase.Run(ServicesToRun);
        }
        #endif
    }
}
