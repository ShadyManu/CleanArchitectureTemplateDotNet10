using Application.Common.Behaviours;
using Application.Common.Interfaces.CQRS;
using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class ApplicationDependencyInjection
{
    public static IServiceCollection AddApplicationDependencyInjection(this IServiceCollection services)
    {
        // Automatically register all the Queries and Commands
        services.Scan(scan => scan.FromAssembliesOf(typeof(ApplicationDependencyInjection))
            .AddClasses(classes => classes.AssignableTo(typeof(IQueryHandler<,>)), publicOnly: false)
                .AsImplementedInterfaces()
                .WithScopedLifetime()
            .AddClasses(classes => classes.AssignableTo(typeof(ICommandHandler<,>)), publicOnly: false)
                .AsImplementedInterfaces()
                .WithScopedLifetime());
        
        services.Decorate(typeof(IQueryHandler<,>), typeof(UnhandledExceptionDecorator.QueryHandlerUnhandledException<,>));        
        services.Decorate(typeof(IQueryHandler<,>), typeof(ValidationDecorator.QueryHandler<,>));
        
        services.Decorate(typeof(ICommandHandler<,>), typeof(UnhandledExceptionDecorator.CommandHandler<,>));
        services.Decorate(typeof(ICommandHandler<,>), typeof(ValidationDecorator.CommandHandler<,>));
        
        return services;
    }
}
