using System;

namespace FileWatcher.FileProcessor
{
    public interface IFileProcessor
    {
        void Process(dynamic parameters);
    }
}
