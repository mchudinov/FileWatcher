using System;

namespace FileWatcher.FileProcessor
{
    public interface IFilePostProcessor
    {
        void Process(string filename);
    }
}

