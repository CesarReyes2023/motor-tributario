using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LibroFiscal.Application.Purchases.Queries.GetPurchases;
using MediatR;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;

namespace LibroFiscal.Desktop.ViewModels;

public partial class PurchasesViewModel : ObservableObject, IDisposable
{
    private readonly IMediator _mediator;
    private readonly LibroFiscal.Application.Abstractions.Services.IEmpresaActivaService _empresaActivaService;

    public PurchasesViewModel(
        IMediator mediator,
        LibroFiscal.Application.Abstractions.Services.IEmpresaActivaService empresaActivaService)
    {
        _mediator = mediator;
        _empresaActivaService = empresaActivaService;
        Purchases = new ObservableCollection<PurchaseDto>();
        
        _empresaActivaService.EmpresaCambiadaEvent += OnEmpresaCambiada;
    }

    private void OnEmpresaCambiada(object? sender, Guid e) => _ = LoadPurchasesAsync();

    public void Dispose()
    {
        _empresaActivaService.EmpresaCambiadaEvent -= OnEmpresaCambiada;
        GC.SuppressFinalize(this);
    }

    [ObservableProperty]
    private ObservableCollection<PurchaseDto> _purchases;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [RelayCommand]
    public async Task LoadPurchasesAsync()
    {
        if (IsLoading) return;
        
        var companyId = _empresaActivaService.EmpresaActualId;
        if (companyId == null) return;

        IsLoading = true;
        ErrorMessage = string.Empty;
        Purchases.Clear();

        try
        {
            var result = await _mediator.Send(new GetPurchasesQuery(companyId.Value));

            if (result.IsSuccess)
            {
                foreach (var purchase in result.Value)
                {
                    Purchases.Add(purchase);
                }
            }
            else
            {
                ErrorMessage = result.Error.Message;
            }
        }
        catch (System.Exception ex)
        {
            ErrorMessage = $"Error al cargar compras: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
}
