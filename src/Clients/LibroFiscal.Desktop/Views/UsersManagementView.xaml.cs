using System.Windows.Controls;
using LibroFiscal.Desktop.ViewModels;

namespace LibroFiscal.Desktop.Views;

public partial class UsersManagementView : UserControl
{
    public UsersManagementView(UsersManagementViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
