using System;

namespace CascadeWorker.Logging
{
    public interface IPersistLogger
    {
        void Persist(string message, LogLevel level);
        void Persist(Exception exception);
    }
}