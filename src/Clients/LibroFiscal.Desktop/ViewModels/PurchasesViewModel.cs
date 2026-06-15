using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LibroFiscal.Application.Purchases.Queries.GetPurchases;
using MediatR;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;

namespace LibroFiscal.Desktop.ViewModels;

public partial class PurchasesViewModel : ObservableObject
{
    private readonly IMediator _mediator;

    public PurchasesViewModel(IMediator mediator)
    {
        _mediator = mediator;
        Purchases = new ObservableCollection<PurchaseDto>();
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

        IsLoading = true;
        ErrorMessage = string.Empty;
        Purchases.Clear();

        try
        {
            var result = await _mediator.Send(new GetPurchasesQuery());

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
