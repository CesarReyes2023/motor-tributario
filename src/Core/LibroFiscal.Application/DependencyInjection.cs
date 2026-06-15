using FluentValidation;
using LibroFiscal.Application.Abstractions.Behaviors;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace LibroFiscal.Application;

/// <summary>
/// Dependency injection registration for the Application layer.
/// Registers MediatR, FluentValidation, and pipeline behaviors.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        // Register MediatR handlers from this assembly
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);

            // Pipeline behaviors execute in registration order:
            // 1. Logging (outermost — captures total time including validation)
            // 2. Validation (reject invalid commands before handler)
            // 3. Transaction (commit UoW after successful handler)
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));
        });

        // Register all FluentValidation validators from this assembly
        services.AddValidatorsFromAssembly(assembly, includeInternalTypes: true);

        // Register Domain Services
        services.AddSingleton<LibroFiscal.Domain.Taxes.Services.ITaxEngine, LibroFiscal.Domain.Taxes.Services.TaxEngine>();

        return services;
    }
}
