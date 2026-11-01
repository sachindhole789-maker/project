namespace Rest.API.Contracts;
public sealed record ApiResponse<T>(string RequestId, T? Data, string Message, string Status, string StatusCode, App.Application.Common.PageMetadata? Pagination = null);
