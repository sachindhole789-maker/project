using App.Application.Messaging;
using App.Application.Common;
using App.Domain.DTO.User;
using App.Domain.Enums;
using App.Domain.IInterfaces;
using FluentValidation;
using Microsoft.Extensions.Logging;
namespace App.Application.Features.V1.Users;

// Keep the request, response, validator and handler together per use case.
public sealed record GetStudentQuery(long CustomerId) : IQuery<GetStudentResponse>;
public sealed record GetStudentResponse(long CustomerId, string Name, string Email, string Mobile, string City);
public sealed class GetStudentQueryValidator : AbstractValidator<GetStudentQuery>
{
    public GetStudentQueryValidator() => RuleFor(x => x.CustomerId).GreaterThan(0).WithMessage("Invalid customer id").WithName("CustomerId");
}
public sealed class GetStudentQueryHandler(IStudentRepository studentRepository, IUserInfoProvider userInfo, ILogger<GetStudentQueryHandler> logger) : IQueryHandler<GetStudentQuery, GetStudentResponse>
{
    public async Task<Result<GetStudentResponse>> HandleAsync(GetStudentQuery query, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling GetStudentQuery for user {UserId}", userInfo.UserId);
        var student = await studentRepository.GetByCustomerIdAsync(new GetStudentByCustomerIdRequestDTO(query.CustomerId), cancellationToken);
        if (student is null) return Result<GetStudentResponse>.Failure("Student not found", "404", ResultStatus.NotFound);
        var response = new GetStudentResponse(student.CustomerId, student.Name, student.Email, student.Mobile, student.City);
        return Result<GetStudentResponse>.Success(response, "Retrieved successfully");
    }
}
