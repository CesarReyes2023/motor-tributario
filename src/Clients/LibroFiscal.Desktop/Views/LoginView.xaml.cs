using System.Windows;
using LibroFiscal.Desktop.ViewModels;

namespace LibroFiscal.Desktop.Views;

public partial class LoginView : Window
{
    public LoginView(LoginViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;

        viewModel.LoginSuccessful += ViewModel_LoginSuccessful;
    }

    private void ViewModel_LoginSuccessful(object? sender, EventArgs e)
    {
        DialogResult = true;
        Close();
    }

    private void TxtPassword_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (DataContext is LoginViewModel viewModel && sender is System.Windows.Controls.PasswordBox pb)
        {
            viewModel.Password = pb.Password;
        }
    }
}
