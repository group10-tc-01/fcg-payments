using FCG.Payments.WebApi.Models;
using FCG.Users.Domain.Exceptions;
using FCG.Users.Messages;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace FCG.Payments.WebApi.Middlewares
{
    [ExcludeFromCodeCoverage]
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IHostEnvironment _env;
        private const string CorrelationIdKey = "CorrelationId";

        public GlobalExceptionMiddleware(RequestDelegate next, IHostEnvironment env)
        {
            _next = next;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }

        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var traceId = context!.TraceIdentifier;
            var correlationId = context.Items.ContainsKey(CorrelationIdKey) ? context.Items[CorrelationIdKey]?.ToString() : string.Empty;

            context!.Response.ContentType = "application/json";

            if (exception is ValidationException validationException)
            {
                await HandleValidationExceptionAsync(context, validationException, correlationId);
                return;
            }

            if (exception is BaseException apiException)
            {
                await HandleApiExceptionAsync(context, apiException, correlationId);
                return;
            }

            await HandleGenericExceptionAsync(context, exception, traceId, correlationId);
        }

        private async Task HandleValidationExceptionAsync(HttpContext context, ValidationException exception, string? correlationId)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;

            var validationErrors = exception.Errors.Select(error => error.ErrorMessage).ToList();
            var response = ApiResponse<object>.ErrorResponse(validationErrors, System.Net.HttpStatusCode.BadRequest);

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };

            var jsonResponse = JsonSerializer.Serialize(response, options);

            await context.Response.WriteAsync(jsonResponse);
        }

        private async Task HandleApiExceptionAsync(HttpContext context, BaseException exception, string? correlationId)
        {
            context.Response.StatusCode = (int)exception.StatusCode;

            var response = ApiResponse<object>.ErrorResponse(new List<string> { exception.Message }, exception.StatusCode);

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };

            var jsonResponse = JsonSerializer.Serialize(response, options);

            await context.Response.WriteAsync(jsonResponse);
        }

        private async Task HandleGenericExceptionAsync(HttpContext context, Exception exception, string traceId, string? correlationId)
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;

            var problemDetails = new ProblemDetails
            {
                Title = ResourceMessages.UnexpectedErrorOccurred,
                Status = context.Response.StatusCode,
                Instance = context.Request.Path,
                Detail = ResourceMessages.PleaseContactSupport,
            };

            problemDetails.Extensions["traceId"] = traceId;

            if (!string.IsNullOrEmpty(correlationId))
            {
                problemDetails.Extensions["correlationId"] = correlationId;
            }

            if (_env.IsDevelopment())
            {
                problemDetails.Extensions["stackTrace"] = exception.StackTrace;
            }

            var jsonResponse = JsonSerializer.Serialize(problemDetails, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            });

            await context.Response.WriteAsync(jsonResponse);
        }
    }
}
