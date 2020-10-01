using System;

namespace CascadeWorker.Logging
{
    public interface ILogger
    {
        void Trace(string message);
        void Warning(string message);
        void Debug(string message);
        void Success(string message);
        void Error(string message);
        void Exception(Exception e);
    }
}