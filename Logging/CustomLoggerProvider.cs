using System.Collections.Concurrent;
using APICatalogo.Logging;

namespace APICatalogo.Logging
{
    public class CustomLoggerProvider : ILoggerProvider // Implementa a interface ILoggerProvider
    {
        readonly CustomLoggerProviderConfiguration loggerConfig;
        readonly ConcurrentDictionary<string, CustomerLogger> loggers = new ConcurrentDictionary<string, CustomerLogger>();

        public CustomLoggerProvider(CustomLoggerProviderConfiguration config)
        {
            loggerConfig = config;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return loggers.GetOrAdd(categoryName, name => new CustomerLogger(name, loggerConfig)); // Adiciona um novo logger
        }

        public void Dispose() // Limpa a lista de loggers
        {
            loggers.Clear();
        }
    }
}
