using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace APICatalogo.Filters
{
    public class ApiExceptionFilter : IExceptionFilter
    {
        private readonly ILogger<ApiExceptionFilter> _logger;

        public ApiExceptionFilter(ILogger<ApiExceptionFilter> logger)
        {
            _logger = logger;
        }

        public void OnException(ExceptionContext context)
        {
            _logger.LogError(context.Exception, "Ocorreu uma exceção não tratada: Status Code 500");

            context.Result = new JsonResult(new // Retorna um objeto JSON com a mensagem de erro
            {
                StatusCode = StatusCodes.Status500InternalServerError,
                Message = "Ocorreu um erro ao processar sua requisição. Por favor, tente novamente."
            })
            {
                StatusCode = StatusCodes.Status500InternalServerError // Define o status code da resposta
            };

        }
    }
}
