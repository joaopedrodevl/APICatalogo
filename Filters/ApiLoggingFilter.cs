using Microsoft.AspNetCore.Mvc.Filters;

namespace APICatalogo.Filters
{
    public class ApiLoggingFilter : IActionFilter // Implementa a interface IActionFilter
    {
        private readonly ILogger<ApiLoggingFilter> _logger; // Injeta a dependência do ILogger.

        public ApiLoggingFilter(ILogger<ApiLoggingFilter> logger)
        {
            _logger = logger;
        }

        public void OnActionExecuting(ActionExecutingContext context) // Implementa o método OnActionExecuting
        {
            _logger.LogInformation("### Executando -> OnActionExecuting"); // Loga a execução do método
            _logger.LogInformation("################################################");
            _logger.LogInformation($"{DateTime.Now.ToLongTimeString()}");
            _logger.LogInformation($"ModelState: {context.ModelState.IsValid}");
            _logger.LogInformation("################################################");
        }

        public void OnActionExecuted(ActionExecutedContext context) // Implementa o método OnActionExecuted
        {
            _logger.LogInformation("### Executando -> OnActionExecuted"); // Loga a execução do método
            _logger.LogInformation("################################################");
            _logger.LogInformation($"{DateTime.Now.ToLongTimeString()}");
            _logger.LogInformation($"StatusCode: {context.HttpContext.Response.StatusCode}");
            _logger.LogInformation("################################################");
        }
    }
}
