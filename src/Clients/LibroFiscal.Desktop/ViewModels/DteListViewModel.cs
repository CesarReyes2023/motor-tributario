using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LibroFiscal.Application.DTE.Queries.GetDtes;
using MediatR;

namespace LibroFiscal.Desktop.ViewModels;

public partial class DteListViewModel : ObservableObject
{
    private readonly IMediator _mediator;
    public Action? NavigateToCreateDteAction { get; set; }

    [ObservableProperty]
    private ObservableCollection<DteSummaryDto> _dtes = new();

    public DteListViewModel(IMediator mediator)
    {
        _mediator = mediator;
    }

    public async Task LoadDtesAsync()
    {
        try
        {
            var result = await _mediator.Send(new GetDtesQuery());
            if (result.IsSuccess)
            {
                Dtes.Clear();
                foreach (var dte in result.Value)
                {
                    Dtes.Add(dte);
                }
            }
            else
            {
                System.IO.File.WriteAllText("error_list.txt", "Query failed: " + result.Error.Code + " - " + result.Error.Message);
            }
        }
        catch (Exception ex)
        {
            System.IO.File.WriteAllText("error_list.txt", ex.ToString());
        }
    }

    [RelayCommand]
    private void NavigateToCreateDte()
    {
        NavigateToCreateDteAction?.Invoke();
    }

    [RelayCommand]
    private async Task ExportToPdfAsync(DteSummaryDto? dte)
    {
        if (dte is null) return;

        try
        {
            var result = await _mediator.Send(new LibroFiscal.Application.DTE.Queries.GetDtePdf.GetDtePdfQuery(dte.Id));
            if (result.IsSuccess)
            {
                var filePath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"DTE_{dte.NumeroControl}_{DateTime.Now:yyyyMMddHHmmss}.pdf");
                await System.IO.File.WriteAllBytesAsync(filePath, result.Value);

                // Abrir el PDF usando la aplicación predeterminada del sistema
                var processInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = filePath,
                    UseShellExecute = true
                };
                System.Diagnostics.Process.Start(processInfo);
            }
            else
            {
                System.Windows.MessageBox.Show($"Error generando PDF: {result.Error.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Ocurrió un error inesperado: {ex.Message}", "Error Crítico", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }
    }
}
