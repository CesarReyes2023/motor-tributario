using System.Windows;
using LibroFiscal.Application;
using LibroFiscal.Desktop.ViewModels;
using LibroFiscal.Integrations;
using LibroFiscal.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LibroFiscal.Desktop;

public partial class App : System.Windows.Application
{
    public static new App Current => (App)System.Windows.Application.Current;
    public IServiceProvider Services { get; }
    public IConfiguration Configuration { get; }

    public App()
    {
        Configuration = BuildConfiguration();
        Services = ConfigureServices(Configuration);
    }

    private static IConfiguration BuildConfiguration()
    {
        var builder = new ConfigurationBuilder();
        
        // Simulating appsettings.json for desktop app:
        var inMemorySettings = new Dictionary<string, string?>
        {
            // For portable V1: Use Sqlite. To return to PostgreSQL, change DatabaseProvider to "PostgreSql"
            { "DatabaseProvider", "PostgreSql" },
            { "ConnectionStrings:DefaultConnection", "Host=localhost;Database=librofiscal;Username=postgres;Password=zeref;" }
        };
        
        builder.AddInMemoryCollection(inMemorySettings);
        return builder.Build();
    }

    private static ServiceProvider ConfigureServices(IConfiguration configuration)
    {
        var services = new ServiceCollection();

        // Register Architecture Layers
        services.AddApplication();
        services.AddPersistence(configuration);
        services.AddIntegrations();

        // ViewModels
        services.AddTransient<LoginViewModel>();
        services.AddTransient<MainViewModel>();
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<CompanyViewModel>();
        services.AddTransient<DteListViewModel>();
        services.AddTransient<CreateDteViewModel>();
        services.AddTransient<OcrScannerViewModel>();
        services.AddTransient<PurchasesViewModel>();
        services.AddTransient<VatBooksViewModel>();
        services.AddTransient<IngestionViewModel>();

        // Views
        services.AddTransient<Views.LoginView>();
        services.AddTransient<Views.CompanyView>();
        services.AddTransient<Views.DteListView>();
        services.AddTransient<Views.CreateDteView>();
        services.AddTransient<Views.OcrScannerView>();
        services.AddTransient<Views.PurchasesView>();
        services.AddTransient<Views.VatBooksView>();
        services.AddTransient<Views.IngestionView>();
        services.AddTransient<MainWindow>();

        // Services
        services.AddSingleton<LibroFiscal.Application.Abstractions.Services.ICurrentUserService, Desktop.Services.CurrentUserService>();
        services.AddSingleton<Desktop.Services.CsvExportService>();

        return services.BuildServiceProvider();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Prevents the application from shutting down when the LoginView closes
        ShutdownMode = ShutdownMode.OnExplicitShutdown;

        // Ensure SQLite DB is created (MVP feature)
        var dbContext = Services.GetRequiredService<LibroFiscalDbContext>();
        dbContext.Database.EnsureCreated();

        var loginView = Services.GetRequiredService<Views.LoginView>();
        
        if (loginView.ShowDialog() == true)
        {
            var mainWindow = Services.GetRequiredService<MainWindow>();
            System.Windows.Application.Current.MainWindow = mainWindow;
            ShutdownMode = ShutdownMode.OnLastWindowClose;
            mainWindow.Show();
        }
        else
        {
            Shutdown();
        }
    }
}
