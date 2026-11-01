using App.Application.Messaging;
using App.Domain.Data;
using App.Domain.Results;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace App.Application.Features.V1.Users;

// Example feature. Copy this single file pattern for each query/command feature.
public sealed record GetStudentQuery(long CustomerId) : IQuery<GetStudentResponse>;
public sealed record GetStudentResponse(string Name, string Email, string Mobile, string City);
public sealed class GetStudentQueryValidator : AbstractValidator<GetStudentQuery>
{
 public GetStudentQueryValidator() => RuleFor(x=>x.CustomerId).GreaterThan(0).WithMessage("Invalid customer id").WithName("CustomerId");
}
public sealed class GetStudentQueryHandler(IDbManager db, IUserInfoProvider userInfo, Microsoft.Extensions.Logging.ILogger<GetStudentQueryHandler> logger) : IQueryHandler<GetStudentQuery,GetStudentResponse>
{
 public async Task<Result<GetStudentResponse>> HandleAsync(GetStudentQuery query, CancellationToken cancellationToken)
 {
  logger.LogInformation("Handling GetStudentQuery for user {UserId}",userInfo.UserId);
  var student = await db.QuerySingleOrDefaultAsync<GetStudentResponse>("GetStudentByCustomerId",new { query.CustomerId },cancellationToken);
  return student is null ? Result<GetStudentResponse>.Failure("Student not found","404",ResultStatus.NotFound) : Result<GetStudentResponse>.Success(student,"Retrieved successfully");
 }
}


