using System;
using System.Net;
using System.Threading.Tasks;
using Application.Handlers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace WebAPI.Middlewares
{
    public class ExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlerMiddleware> _logger;

        public ExceptionHandlerMiddleware(RequestDelegate next, ILogger<ExceptionHandlerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }
        public async Task Invoke (HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await ExceptionHandlerAsync(context, ex, _logger);
            }
        }

        private async Task ExceptionHandlerAsync(HttpContext context, Exception ex, ILogger<ExceptionHandlerMiddleware> logger)
        {
            object errors = null;

            switch (ex)
            {
                case ExceptionHandler exceptionHandler:
                    logger.LogError(ex, "<-- Custom Exception Handler -->");
                    errors = exceptionHandler.Errors;
                    context.Response.StatusCode = (int)exceptionHandler.Code;
                    break;

                case Exception e:
                    logger.LogError(ex, "<-- Server Error -->");
                    errors = string.IsNullOrWhiteSpace(e.Message)? "Error" : e.Message;
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    break;
            }

            context.Response.ContentType = "application/json";

            if(errors != null) {
                var results = JsonConvert.SerializeObject(new { errors });
                await context.Response.WriteAsync(results);
            }
        }
    }
}