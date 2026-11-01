using App.Application.Common;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
namespace App.Application.Messaging;
public interface IQuery<TResponse> { }
public interface ICommand<TResponse> { }
public interface IQueryHandler<TQuery,TResponse> where TQuery : IQuery<TResponse> { Task<Result<TResponse>> HandleAsync(TQuery query,CancellationToken cancellationToken); }
public interface ICommandHandler<TCommand,TResponse> where TCommand : ICommand<TResponse> { Task<Result<TResponse>> HandleAsync(TCommand command,CancellationToken cancellationToken); }
public interface IDispatcher { Task<Result<TResponse>> ExecuteQuery<TResponse>(IQuery<TResponse> query,CancellationToken cancellationToken=default);Task<Result<TResponse>> ExecuteCommand<TResponse>(ICommand<TResponse> command,CancellationToken cancellationToken=default); }
public sealed class Dispatcher(IServiceProvider services):IDispatcher
{
 public async Task<Result<TResponse>> ExecuteQuery<TResponse>(IQuery<TResponse> query,CancellationToken ct=default){await ValidateAsync(query,ct);var type=typeof(IQueryHandler<,>).MakeGenericType(query.GetType(),typeof(TResponse));dynamic handler=services.GetRequiredService(type);return await handler.HandleAsync((dynamic)query,ct);}
 public async Task<Result<TResponse>> ExecuteCommand<TResponse>(ICommand<TResponse> command,CancellationToken ct=default){await ValidateAsync(command,ct);var type=typeof(ICommandHandler<,>).MakeGenericType(command.GetType(),typeof(TResponse));dynamic handler=services.GetRequiredService(type);return await handler.HandleAsync((dynamic)command,ct);}
 private async Task ValidateAsync<T>(T request,CancellationToken ct){var validators=services.GetServices<IValidator<T>>();var errors=new List<string>();foreach(var validator in validators){var result=await validator.ValidateAsync(request!,ct);errors.AddRange(result.Errors.Select(e=>e.ErrorMessage));}if(errors.Count>0)throw new RequestValidationException(errors);}
}
public sealed class RequestValidationException(IReadOnlyCollection<string> errors):Exception("Request validation failed"){public IReadOnlyCollection<string> Errors{get;}=errors;}
