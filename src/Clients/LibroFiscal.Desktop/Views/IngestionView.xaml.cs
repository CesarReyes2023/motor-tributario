using LibroFiscal.Desktop.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace LibroFiscal.Desktop.Views;

public partial class IngestionView : UserControl
{
    public IngestionView()
    {
        InitializeComponent();
    }

    private void Border_Drop(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            
            if (DataContext is IngestionViewModel viewModel)
            {
                viewModel.AddFilesCommand.Execute(files);
            }
        }
    }
}
