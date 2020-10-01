using System;
using System.Collections.Generic;

namespace CascadeWorker.Logging.Implementations
{
    public class ConsoleLogger : ILogger
    {
        private readonly Dictionary<LogLevel, ConsoleColor> _colorsForLogType = new Dictionary<LogLevel, ConsoleColor> {
            {LogLevel.Trace , ConsoleColor.White},
            {LogLevel.Success , ConsoleColor.Green},
            {LogLevel.Warning ,    ConsoleColor.Yellow},
            {LogLevel.Debug ,   ConsoleColor.Cyan},
            {LogLevel.Error ,   ConsoleColor.Red},
        };
        
        public void Trace(string message)
        {
            Log(message, LogLevel.Trace);
        }

        public void Warning(string message)
        {
            Log(message, LogLevel.Warning);
        }

        public void Debug(string message)
        {
            Log(message, LogLevel.Debug);
        }

        public void Success(string message)
        {
            Log(message, LogLevel.Success);
        }

        public void Error(string message)
        {
            Log(message, LogLevel.Error);
        }
        
        public void Exception(Exception e)
        {
            Log("Exception caught: " + Environment.NewLine + e, LogLevel.Error);
        }
        
        private void Log(string message, LogLevel level)
        {
            var oldColor = Console.ForegroundColor;
            var newColor = _colorsForLogType[level];
            
            Console.ForegroundColor = newColor;
            Console.WriteLine($"[{DateTime.Now:MM/dd HH:mm:ss}] " + message);
            Console.ForegroundColor = oldColor;
        }
    }
}