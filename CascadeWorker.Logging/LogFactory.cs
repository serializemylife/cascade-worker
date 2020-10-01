using CascadeWorker.Logging.Implementations;

namespace CascadeWorker.Logging
{
    public class LogFactory
    {
        public ILogger GetLogger()
        {
            return new HybridLogger(
                new ConsoleLogger(), 
                new FileLogger() // fkfk
            );

        }
    }
}