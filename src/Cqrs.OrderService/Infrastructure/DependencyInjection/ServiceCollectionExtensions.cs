using System.Reflection;
using Cqrs.OrderService.Bus.Command;
using Cqrs.OrderService.Bus.Query;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Cqrs.OrderService.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var concreteTypes = assembly
            .GetTypes()
            .Where(type => type is { IsClass: true, IsAbstract: false, IsGenericTypeDefinition: false })
            .ToArray();

        foreach (var type in concreteTypes)
        {
            RegisterMatchingInterface(services, type);
            RegisterHandlers(services, type);
        }

        return services;
    }

    private static void RegisterMatchingInterface(IServiceCollection services, Type implementationType)
    {
        if (!implementationType.Name.EndsWith("Service", StringComparison.Ordinal) &&
            !implementationType.Name.EndsWith("Repository", StringComparison.Ordinal))
        {
            return;
        }

        services.TryAddScoped(implementationType);

        var matchingInterface = implementationType.GetInterfaces()
            .FirstOrDefault(@interface => string.Equals(@interface.Name, $"I{implementationType.Name}", StringComparison.Ordinal));

        if (matchingInterface is not null)
        {
            services.TryAddScoped(matchingInterface, implementationType);
        }
    }

    private static void RegisterHandlers(IServiceCollection services, Type implementationType)
    {
        foreach (var serviceType in implementationType.GetInterfaces().Where(IsHandlerInterface))
        {
            services.TryAddScoped(serviceType, implementationType);
        }
    }

    private static bool IsHandlerInterface(Type type) =>
        type.IsGenericType && type.GetGenericTypeDefinition() is var definition &&
        (definition == typeof(ICommandHandler<,>) || definition == typeof(IQueryHandler<,>));
}
