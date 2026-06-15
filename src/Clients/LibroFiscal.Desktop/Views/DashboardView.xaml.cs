using System.Windows;
using System.Windows.Controls;
using LibroFiscal.Desktop.ViewModels;

namespace LibroFiscal.Desktop.Views;

public partial class DashboardView : UserControl
{
    public DashboardView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is DashboardViewModel vm)
        {
            if (vm.LoadDashboardCommand.CanExecute(null))
            {
                vm.LoadDashboardCommand.Execute(null);
            }
        }
    }
}
