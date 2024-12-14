using System.Net;
using Baigiamasis.DTOs.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Baigiamasis.Middleware
{
    public class GlobalExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

        public GlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred while processing the request.");
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            
            var (statusCode, message) = exception switch
            {
                // Validation errors - safe to expose
                ArgumentException => (HttpStatusCode.BadRequest, 
                    exception.Message),
                ValidationException => (HttpStatusCode.BadRequest, 
                    exception.Message),
                    
                // Authentication/Authorization - generic messages
                UnauthorizedAccessException => (HttpStatusCode.Unauthorized, 
                    "You are not authorized to perform this action"),
                    
                // Not Found - generic message
                KeyNotFoundException => (HttpStatusCode.NotFound, 
                    "The requested resource was not found"),
                    
                // Business logic errors - generic messages
                InvalidOperationException => (HttpStatusCode.BadRequest, 
                    "The requested operation cannot be completed"),
                    
                // Database errors - hide implementation details
                DbUpdateConcurrencyException => (HttpStatusCode.Conflict, 
                    "The resource was modified by another user. Please try again"),
                DbUpdateException => (HttpStatusCode.BadRequest, 
                    "Unable to save changes. Please verify your data"),
                    
                // Unexpected errors - very generic message
                _ => (HttpStatusCode.InternalServerError, 
                    "An unexpected error occurred. Please try again later")
            };

            // Log the actual exception for debugging
            _logger.LogError(exception, 
                "Error handling request: {Message}", exception.Message);

            var response = ApiResponse<object>.Failure(message, (int)statusCode);
            await context.Response.WriteAsJsonAsync(response);
        }
    }
}
