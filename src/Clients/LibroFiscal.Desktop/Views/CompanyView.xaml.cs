using System.Windows;
using System.Windows.Controls;
using LibroFiscal.Desktop.ViewModels;

namespace LibroFiscal.Desktop.Views;

public partial class CompanyView : UserControl
{
    public CompanyView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is CompanyViewModel vm)
        {
            if (vm.LoadCompanyCommand.CanExecute(null))
            {
                vm.LoadCompanyCommand.Execute(null);
            }
        }
    }
}
