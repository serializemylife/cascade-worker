using System;

namespace CascadeWorker.Logging.Implementations
{
    public class HybridLogger : ILogger
    {
        private readonly ILogger _consoleLogger;
        private readonly IPersistLogger _persistLogger;
        
        public HybridLogger(ILogger consoleLogger, IPersistLogger persistLogger)
        {
            _consoleLogger = consoleLogger;
            _persistLogger = persistLogger;
        }

        public void Trace(string message)
        {    
            _consoleLogger.Trace(message);
        }

        public void Warning(string message)
        {
            _consoleLogger.Warning(message);
        }

        public void Debug(string message)
        {
            _consoleLogger.Debug(message);
        }

        public void Success(string message)
        {
            _consoleLogger.Success(message);
        }

        public void Error(string message)
        {
            _consoleLogger.Error(message);
        }

        public void Exception(Exception e)
        {
            _consoleLogger.Exception(e);
            _persistLogger.Persist(e);
        }
    }
}