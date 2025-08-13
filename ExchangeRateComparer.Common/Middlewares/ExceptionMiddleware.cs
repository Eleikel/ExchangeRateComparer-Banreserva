using ExchangeRateComparer.Common.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ExchangeRateComparer.Common.Middlewares
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                _logger.LogError($"An exception has occur: {ex}");
                await HandleExceptionAsync(httpContext, ex);
            }
        }

        private Task HandleExceptionAsync(HttpContext httpContext, Exception exception)
        {
            var responseError = new ApiResponse();
            httpContext.Response.ContentType = "application/json";
            httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            responseError.Status = (int)HttpStatusCode.InternalServerError;

            if (IsValidationException(httpContext, exception, responseError))
                return httpContext.Response.WriteAsync(responseError.ToJson());

            if (IsExceptionBase(httpContext, exception, responseError))
                return httpContext.Response.WriteAsync(responseError.ToJson());

            responseError.Message = exception.ToString();
            return httpContext.Response.WriteAsync(responseError.ToJson());

        }

        private bool IsValidationException(
          HttpContext context,
          Exception exception,
          ApiResponse responseError)
        {
            var result = exception.GetType() == typeof(ValidationException);

            if (!result) return result;
            var validationException = (ValidationException)exception;
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            responseError.Status = (int)HttpStatusCode.BadRequest;
            responseError.Message = validationException.Message;
            return result;
        }

        private static bool IsExceptionBase(
            HttpContext context,
            Exception exception,
            ApiResponse responseError)
        {
            var result = exception.GetType().BaseType == typeof(ExceptionBase);

            if (!result) return result;

            var exceptionBase = (ExceptionBase)exception;
            responseError.Status = (int)exceptionBase.ErrorCode;
            responseError.Message = exceptionBase.Message;
            context.Response.StatusCode = (int)exceptionBase.ErrorCode;

            return result;
        }

    }
}
