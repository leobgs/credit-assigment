using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using PengajuanKredit.Api.Exceptions;

namespace PengajuanKredit.Api.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
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
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = context.Response;
        response.ContentType = "application/problem+json";

        ProblemDetails problemDetails;

        switch (exception)
        {
            case NotFoundException notFoundEx:
                _logger.LogWarning(notFoundEx, "Resource tidak ditemukan: {Message}", notFoundEx.Message);
                response.StatusCode = StatusCodes.Status404NotFound;
                problemDetails = new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Data Tidak Ditemukan",
                    Detail = notFoundEx.Message,
                    Instance = context.Request.Path
                };
                break;

            case ValidationException validationEx:
                _logger.LogWarning(validationEx, "Validasi gagal: {Message}", validationEx.Message);
                response.StatusCode = StatusCodes.Status400BadRequest;
                var errors = validationEx.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

                var validationProblem = new HttpValidationProblemDetails(errors)
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Validasi Gagal",
                    Detail = "Satu atau lebih kesalahan validasi terjadi.",
                    Instance = context.Request.Path
                };
                problemDetails = validationProblem;
                break;

            case ArgumentException argEx:
                _logger.LogWarning(argEx, "Argumen tidak valid: {Message}", argEx.Message);
                response.StatusCode = StatusCodes.Status400BadRequest;
                problemDetails = new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Input Tidak Valid",
                    Detail = argEx.Message,
                    Instance = context.Request.Path
                };
                break;

            default:
                _logger.LogError(exception, "Terjadi kesalahan tak terduga pada server.");
                response.StatusCode = StatusCodes.Status500InternalServerError;
                problemDetails = new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "Terjadi Kesalahan pada Server",
                    Detail = "Terjadi kesalahan internal pada server. Silakan coba beberapa saat lagi.",
                    Instance = context.Request.Path
                };
                break;
        }

        var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        await response.WriteAsync(JsonSerializer.Serialize(problemDetails, problemDetails.GetType(), jsonOptions));
    }
}
