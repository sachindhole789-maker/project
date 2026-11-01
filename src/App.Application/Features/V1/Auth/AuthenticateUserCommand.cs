using App.Application.Common;
using App.Application.Messaging;
using App.Domain.DTO.Auth;
using App.Domain.Enums;
using App.Domain.IInterfaces;
using App.Domain.Service;
using FluentValidation;
namespace App.Application.Features.V1.Auth;

// Credentials stay in the repository DTO for verification and never enter the API response.
public sealed record AuthenticateUserCommand(string UserNameOrEmail, string Password) : ICommand<AuthenticateUserResponse>;
public sealed record AuthenticateUserResponse(long UserId, string UserName, string Email);
public sealed class AuthenticateUserCommandValidator : AbstractValidator<AuthenticateUserCommand>
{
    public AuthenticateUserCommandValidator()
    {
        RuleFor(x => x.UserNameOrEmail).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Password).NotEmpty().MaximumLength(256);
    }
}
public sealed class AuthenticateUserCommandHandler(IAuthRepository authRepository, IPasswordHasher passwordHasher) : ICommandHandler<AuthenticateUserCommand, AuthenticateUserResponse>
{
    public async Task<Result<AuthenticateUserResponse>> HandleAsync(AuthenticateUserCommand command, CancellationToken cancellationToken)
    {
        var account = await authRepository.GetLoginAsync(new GetLoginInfoRequestDTO(command.UserNameOrEmail), cancellationToken);
        if (account is null || !account.IsActive || !passwordHasher.Verify(command.Password, account.PasswordHash))
            return Result<AuthenticateUserResponse>.Failure("Invalid credentials", "401", ResultStatus.Failure);
        return Result<AuthenticateUserResponse>.Success(new AuthenticateUserResponse(account.UserId, account.UserName, account.Email), "Authenticated");
    }
}
