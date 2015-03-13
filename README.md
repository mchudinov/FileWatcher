FileWatcher
===========

A template C# solution for a Windows service and a Linux demon to watch changes in filesystem. Program listens to the file system change notifications and raises events when a directory, or file in a directory, changes. 

* Program uses FileSystemWatcher or INotify demon. 
* Linux shell script to start and stop demon
* Logging using log4net
* Service using System.ServiceProcess
* Jobs using Quartz
