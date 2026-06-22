using System;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Windows;
using LibroFiscal.Application;
using LibroFiscal.Desktop.ViewModels;
using LibroFiscal.Integrations;
using LibroFiscal.Persistence;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

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
        var builder = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

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
        services.AddTransient<SettingsViewModel>();
        services.AddTransient<SalesViewModel>();
        services.AddTransient<UsersManagementViewModel>();
        // Views
        services.AddTransient<Views.LoginView>();
        services.AddTransient<Views.CompanyView>();
        services.AddTransient<Views.DteListView>();
        services.AddTransient<Views.CreateDteView>();
        services.AddTransient<Views.OcrScannerView>();
        services.AddTransient<Views.PurchasesView>();
        services.AddTransient<Views.VatBooksView>();
        services.AddTransient<Views.IngestionView>();
        services.AddTransient<Views.SalesView>();
        services.AddTransient<Views.UsersManagementView>();
        services.AddTransient<MainWindow>();

        // Services
        services.AddSingleton<LibroFiscal.Application.Abstractions.Services.ICurrentUserService, Desktop.Services.CurrentUserService>();
        services.AddSingleton<LibroFiscal.Application.Abstractions.Services.IEmpresaActivaService, Desktop.Services.EmpresaActivaService>();
        services.AddSingleton<LibroFiscal.Application.Abstractions.Services.IDialogService, Desktop.Services.DialogService>();
        services.AddSingleton<LibroFiscal.Application.Abstractions.Services.IErrorLogger, Desktop.Services.ErrorLogger>();
        services.AddSingleton<Desktop.Services.CsvExportService>();

        return services.BuildServiceProvider();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        this.DispatcherUnhandledException += (s, args) =>
        {
            var logsDir = System.IO.Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), "LibroFiscal", "Logs");
            System.IO.Directory.CreateDirectory(logsDir);
            var errorPath = System.IO.Path.Combine(logsDir, "crash_empresa.txt");
            System.IO.File.WriteAllText(errorPath, $"FATAL: {args.Exception.Message}\n{args.Exception.StackTrace}\nInner: {args.Exception.InnerException?.Message}");
            MessageBox.Show($"FATAL ERROR: {args.Exception.Message}\n(Log guardado en {logsDir})", "Crash App", MessageBoxButton.OK, MessageBoxImage.Error);
            args.Handled = true;
        };

        // Prevents the application from shutting down when the LoginView closes
        ShutdownMode = ShutdownMode.OnExplicitShutdown;

        try
        {
            // Ensure SQLite DB is created (MVP feature) or Postgres is migrated
            var dbContext = Services.GetRequiredService<LibroFiscalDbContext>();
            
            var databaseProvider = Configuration["DatabaseProvider"];
            if (databaseProvider == "Sqlite")
            {
                dbContext.Database.EnsureCreated();
            }
            else
            {
                // Apply migrations for PostgreSQL
                dbContext.Database.Migrate();
            }

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
        catch (Exception ex)
        {
            MessageBox.Show($"FATAL ERROR: {ex.Message}\n{ex.InnerException?.Message}", "Crash", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown();
        }
    }

    public void Logout()
    {
        var currentUserService = Services.GetRequiredService<LibroFiscal.Application.Abstractions.Services.ICurrentUserService>();
        currentUserService.Clear();

        var loginView = Services.GetRequiredService<Views.LoginView>();
        
        var currentMainWindow = MainWindow;
        MainWindow = loginView;
        
        if (loginView.ShowDialog() == true)
        {
            var mainWindow = Services.GetRequiredService<MainWindow>();
            MainWindow = mainWindow;
            mainWindow.Show();
        }
        else
        {
            Shutdown();
        }

        currentMainWindow?.Close();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        base.OnExit(e);

        try
        {
            var databaseProvider = Configuration["DatabaseProvider"];
            if (databaseProvider == "Sqlite")
            {
                var connString = Configuration.GetConnectionString("DefaultConnection");
                if (!string.IsNullOrEmpty(connString))
                {
                    // Basic parsing of "Data Source=LibroFiscal.db"
                    var parts = connString.Split(';');
                    string dbPath = string.Empty;
                    foreach (var part in parts)
                    {
                        if (part.Trim().StartsWith("Data Source", StringComparison.OrdinalIgnoreCase))
                        {
                            dbPath = part.Split('=')[1].Trim();
                            break;
                        }
                    }

                    if (!string.IsNullOrEmpty(dbPath) && File.Exists(dbPath))
                    {
                        var backupDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "LibroFiscal_Backups");
                        Directory.CreateDirectory(backupDir);

                        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture);
                        var zipPath = Path.Combine(backupDir, $"LibroFiscal_Backup_{timestamp}.zip");

                        // Ponytail: Backup silencioso nativo
                        using (var archive = ZipFile.Open(zipPath, ZipArchiveMode.Create))
                        {
                            archive.CreateEntryFromFile(dbPath, Path.GetFileName(dbPath));
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // Ponytail: Silent fail for backups so it doesn't crash on exit, just log it.
            var logsDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "LibroFiscal", "Logs");
            Directory.CreateDirectory(logsDir);
            File.AppendAllText(Path.Combine(logsDir, "backup_errors.txt"), $"[{DateTime.Now}] {ex.Message}\n");
        }
    }
}
