namespace APICatalogo.Logging
{
    public class CustomerLogger : ILogger
    {
        readonly string loggerName;
        readonly CustomLoggerProviderConfiguration loggerConfig;

        public CustomerLogger(string name, CustomLoggerProviderConfiguration config)
        {
            loggerName = name;
            loggerConfig = config;
        }

        public bool IsEnabled(LogLevel logLevel) // Verifica se o log está habilitado
        {
            return logLevel == loggerConfig.LogLevel;
        }

        public IDisposable BeginScope<TState>(TState state) // Inicia o escopo do log
        {
            return null;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter) // Registra o log
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            string mensagem = $"{logLevel.ToString()}: {eventId.Id} - {formatter(state, exception)}";
            EscreverTextoNoArquivo(mensagem);
        }

        private void EscreverTextoNoArquivo(string mensagem)
        {
            string caminhoArquivo = @"d:\dados\log\logAPI.txt";

            using (StreamWriter streamWriter = new StreamWriter(caminhoArquivo, true))
            {
                try
                {
                    streamWriter.WriteLine(mensagem);
                    streamWriter.Close();
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
    }
}
