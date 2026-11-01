using App.Application.Messaging;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace App.Application;
public static class DependencyInjection
{
 public static IServiceCollection AddApplication(this IServiceCollection services, Assembly? assembly = null) {
  assembly ??= Assembly.GetExecutingAssembly(); services.AddScoped<IDispatcher, Dispatcher>();
  foreach (var type in assembly.DefinedTypes.Where(t => !t.IsAbstract && !t.IsInterface)) {
   foreach (var contract in type.ImplementedInterfaces.Where(i=>i.IsGenericType && (i.GetGenericTypeDefinition()==typeof(IQueryHandler<,>) || i.GetGenericTypeDefinition()==typeof(ICommandHandler<,>)))) services.AddScoped(contract,type);
  }
  services.AddValidatorsFromAssembly(assembly); return services;
 }
}
