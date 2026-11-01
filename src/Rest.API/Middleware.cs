using System.Diagnostics;
using System.Text;
using System.Text.Json;
using App.Domain.Data;
using Microsoft.AspNetCore.Mvc;
namespace Rest.API.Middleware;
public sealed class AuditMiddleware(RequestDelegate next,ILogger<AuditMiddleware> logger)
{
 public async Task InvokeAsync(HttpContext context,IAuditStore store){var sw=Stopwatch.StartNew();var reqBody="";var original=context.Response.Body;await using var capture=new MemoryStream();context.Response.Body=capture;
 try{if(context.Request.ContentLength is >0 and <65536 && context.Request.ContentType?.StartsWith("application/json",StringComparison.OrdinalIgnoreCase)==true){context.Request.EnableBuffering();using var reader=new StreamReader(context.Request.Body,Encoding.UTF8,leaveOpen:true);reqBody=await reader.ReadToEndAsync();context.Request.Body.Position=0;}await next(context);}finally{sw.Stop();capture.Position=0;string response="";if(capture.Length<65536 && context.Response.ContentType?.StartsWith("application/json",StringComparison.OrdinalIgnoreCase)==true){using var reader=new StreamReader(capture,Encoding.UTF8,leaveOpen:true);response=await reader.ReadToEndAsync();capture.Position=0;}await capture.CopyToAsync(original);context.Response.Body=original;try{await store.WriteAsync(new(context.TraceIdentifier,context.User.FindFirst("sub")?.Value??context.User.Identity?.Name,context.Request.Method,context.Request.Path,context.Response.StatusCode,sw.ElapsedMilliseconds,DateTimeOffset.UtcNow,Redact(reqBody),Redact(response)),context.RequestAborted);}catch(Exception ex){logger.LogError(ex,"Audit record failed for {RequestId}",context.TraceIdentifier);}}
 }
 private static string? Redact(string s)=>string.IsNullOrEmpty(s)?null:System.Text.RegularExpressions.Regex.Replace(s,"(?i)(\"(?:password|token|secret|authorization|apiKey)\"\\s*:\\s*\")[^\"]*(\")","$1[REDACTED]$2");
}
public static class ApiResultExtensions
{
 public static IActionResult ToActionResult<T>(this ControllerBase controller,App.Domain.Results.Result<T> result){var body=new App.Domain.Results.ApiResponse<T>(controller.HttpContext.TraceIdentifier,result.Data,result.Message,result.Status.ToString(),result.StatusCode);return controller.Ok(body);}
 public static IActionResult ToActionResult<T>(this ControllerBase controller,App.Domain.Results.PaginatedResult<T> result){var body=new App.Domain.Results.ApiResponse<IReadOnlyList<T>>(controller.HttpContext.TraceIdentifier,result.Items,result.Message,result.Status.ToString(),result.StatusCode,result.Pagination);return controller.Ok(body);}
}

