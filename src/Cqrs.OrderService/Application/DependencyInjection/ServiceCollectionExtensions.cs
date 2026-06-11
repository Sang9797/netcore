using System.Reflection;
using Cqrs.OrderService.Application.Common.Behaviors;
using Cqrs.OrderService.Application.IntegrationEvents;
using Cqrs.OrderService.Application.Abstractions.Messaging;
using FluentValidation;
using MediatR;

namespace Cqrs.OrderService.Application.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        services.AddMediatR(configuration => configuration.RegisterServicesFromAssembly(assembly));
        services.AddValidatorsFromAssembly(assembly);
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));
        RegisterIntegrationEventHandlers(services, assembly);

        return services;
    }

    private static void RegisterIntegrationEventHandlers(IServiceCollection services, Assembly assembly)
    {
        var implementations = assembly.GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false })
            .ToArray();

        foreach (var implementation in implementations)
        {
            foreach (var contract in implementation.GetInterfaces().Where(IsIntegrationEventHandlerContract))
            {
                services.AddScoped(contract, implementation);
            }
        }
    }

    private static bool IsIntegrationEventHandlerContract(Type type) =>
        type.IsGenericType &&
        type.GetGenericTypeDefinition() == typeof(IIntegrationEventHandler<>) &&
        typeof(IntegrationEvent).IsAssignableFrom(type.GenericTypeArguments[0]);
}
