using App.Application.Messaging;
using App.Domain.Results;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
namespace Rest.API.Middleware;
public sealed class RequestExceptionMiddleware(RequestDelegate next,ILogger<RequestExceptionMiddleware> logger)
{
 public async Task InvokeAsync(HttpContext context){try{await next(context);}catch(RequestValidationException ex){context.Response.StatusCode=StatusCodes.Status400BadRequest;context.Response.ContentType="application/json";var body=new ApiResponse<IReadOnlyCollection<string>>(context.TraceIdentifier,ex.Errors,"Validation failed",ResultStatus.ValidationError.ToString(),"400");await context.Response.WriteAsJsonAsync(body);}catch(Exception ex){logger.LogError(ex,"Unhandled request error {RequestId}",context.TraceIdentifier);context.Response.StatusCode=StatusCodes.Status500InternalServerError;context.Response.ContentType="application/json";var body=new ApiResponse<object>(context.TraceIdentifier,null,"An unexpected error occurred",ResultStatus.Failure.ToString(),"500");await context.Response.WriteAsJsonAsync(body);}}
}
