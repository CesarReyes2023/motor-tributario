using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace LibroFiscal.Persistence;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<LibroFiscalDbContext>
{
    public LibroFiscalDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<LibroFiscalDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Database=librofiscal;Username=postgres;Password=zeref");

        return new LibroFiscalDbContext(optionsBuilder.Options);
    }
}
