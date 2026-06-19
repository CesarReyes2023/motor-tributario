using LibroFiscal.Persistence.Repositories;
using LibroFiscal.SharedKernel.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LibroFiscal.Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var databaseProvider = configuration["DatabaseProvider"];
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<LibroFiscalDbContext>(options =>
        {
            if (databaseProvider == "Sqlite")
            {
                options.UseSqlite(connectionString);
            }
            else
            {
                options.UseNpgsql(
                    connectionString,
                    builder => builder.MigrationsAssembly(typeof(LibroFiscalDbContext).Assembly.FullName));
            }
        });

        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<LibroFiscalDbContext>());
        services.AddScoped(typeof(IRepository<,>), typeof(Repository<,>));
        services.AddScoped<LibroFiscal.Application.DTE.Queries.GetDtes.IDteReadService, LibroFiscal.Persistence.Services.DteReadService>();
        services.AddSingleton<LibroFiscal.Application.Abstractions.Services.IPasswordHasher, LibroFiscal.Persistence.Security.PasswordHasher>();
        services.AddSingleton<LibroFiscal.Application.Abstractions.Services.IEncryptionService, LibroFiscal.Persistence.Security.EncryptionService>();

        return services;
    }
}
