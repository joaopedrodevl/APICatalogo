using APICatalogo.Models;
using Microsoft.AspNetCore.Diagnostics;
using System.Net;

namespace APICatalogo.Extensions
{
    public static class ApiExceptionMiddlewareExtensions // Cria uma classe de extensão para o IApplicationBuilder. Uma classe de extensão é uma classe estática que contém métodos estáticos, e é marcada com o modificador "this" no primeiro parâmetro. O método de extensão é chamado como se fosse um método de instância no tipo estendido.
    {
        public static void ConfigureExceptionHandler(this IApplicationBuilder app) // Método de extensão para configurar o tratamento de exceções
        {
            app.UseExceptionHandler(appError => // Usa o middleware de tratamento de exceções
            {
                appError.Run(async context => // Contexto de resposta
                {
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    context.Response.ContentType = "application/json";

                    var contextFeature = context.Features.Get<IExceptionHandlerFeature>(); // Obtém o recurso de tratamento de exceções
                    if (contextFeature != null) // Se houver um erro na requisição
                    {
                        var ex = contextFeature.Error; // Obtém a exceção
                        var errorDetails = new ErrorDetails() // Cria um objeto ErrorDetails
                        {
                            StatusCode = context.Response.StatusCode,
                            Message = ex.Message,
                            Trace = ex.StackTrace
                        };

                        await context.Response.WriteAsync(errorDetails.ToString()); // Escreve o objeto ErrorDetails na resposta
                    }
                });
            });
        }
    }
}
