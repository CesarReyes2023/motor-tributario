using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LibroFiscal.Application.DTE.Queries.GetDtes;
using MediatR;

namespace LibroFiscal.Desktop.ViewModels;

public partial class DteListViewModel : ObservableObject, IDisposable
{
    private readonly IMediator _mediator;
    private readonly LibroFiscal.Application.Abstractions.Services.IEmpresaActivaService _empresaActivaService;
    private readonly LibroFiscal.Application.Abstractions.Services.IDialogService _dialogService;
    private readonly LibroFiscal.Application.Abstractions.Services.IErrorLogger _errorLogger;
    public Action? NavigateToCreateDteAction { get; set; }

    [ObservableProperty]
    private ObservableCollection<DteSummaryDto> _dtes = new();

    public DteListViewModel(
        IMediator mediator,
        LibroFiscal.Application.Abstractions.Services.IEmpresaActivaService empresaActivaService,
        LibroFiscal.Application.Abstractions.Services.IDialogService dialogService,
        LibroFiscal.Application.Abstractions.Services.IErrorLogger errorLogger)
    {
        _mediator = mediator;
        _empresaActivaService = empresaActivaService;
        _dialogService = dialogService;
        _errorLogger = errorLogger;
        
        _empresaActivaService.EmpresaCambiadaEvent += OnEmpresaCambiada;
    }

    private void OnEmpresaCambiada(object? sender, Guid e) => _ = LoadDtesAsync();

    public void Dispose()
    {
        _empresaActivaService.EmpresaCambiadaEvent -= OnEmpresaCambiada;
        GC.SuppressFinalize(this);
    }

    public async Task LoadDtesAsync()
    {
        var companyId = _empresaActivaService.EmpresaActualId;
        if (companyId == null)
        {
            Dtes.Clear();
            return;
        }

        try
        {
            var result = await _mediator.Send(new GetDtesQuery(companyId.Value));
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
                _errorLogger.LogError("dte_list", $"Query failed: {result.Error.Code} - {result.Error.Message}");
            }
        }
        catch (Exception ex)
        {
            _errorLogger.LogError("dte_list", ex);
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
                _dialogService.ShowError($"Error generando PDF: {result.Error.Message}");
            }
        }
        catch (Exception ex)
        {
            _dialogService.ShowError($"Ocurrió un error inesperado: {ex.Message}");
        }
    }
}
