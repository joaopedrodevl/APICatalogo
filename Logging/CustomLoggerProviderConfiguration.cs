namespace APICatalogo.Logging
{
    public class CustomLoggerProviderConfiguration
    {
        public LogLevel LogLevel { get; set; } = LogLevel.Warning; // Define o nível de log padrão
        public int EventId { get; set; } = 0; // Define o Id do evento padrão
    }
}
