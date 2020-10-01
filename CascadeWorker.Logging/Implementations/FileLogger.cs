using System;
using System.Collections.Generic;
using System.IO;

namespace CascadeWorker.Logging.Implementations
{
    public class FileLogger : IPersistLogger
    {
        private readonly Dictionary<LogLevel, string> _fileNameForLogType = new Dictionary<LogLevel, string> {
            {LogLevel.Trace , "trace.log"},
            {LogLevel.Success , "success.log"},
            {LogLevel.Warning ,    "warn.log"},
            {LogLevel.Debug ,   "debug.log"},
            {LogLevel.Error ,   "error.log"},
        };

        public void Persist(string e, LogLevel level)
        {
            File.WriteAllText(_fileNameForLogType[level], e + Environment.NewLine);
        }

        public void Persist(Exception e)
        {
            Persist(e.ToString(), LogLevel.Error);
        }
    }
}