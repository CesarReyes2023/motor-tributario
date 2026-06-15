using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LibroFiscal.Application.DteIngestion.Commands.IngestDtes;
using MediatR;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace LibroFiscal.Desktop.ViewModels;

public partial class IngestionViewModel : ObservableObject
{
    private readonly IMediator _mediator;

    [ObservableProperty]
    private bool _isProcessing;

    [ObservableProperty]
    private string _statusMessage = "Arrastra y suelta tus archivos .json o imágenes aquí.";

    public ObservableCollection<IngestionItemViewModel> FilesToProcess { get; } = new();

    public IngestionViewModel(IMediator mediator)
    {
        _mediator = mediator;
    }

    [RelayCommand]
    private void AddFiles(string[] filePaths)
    {
        foreach (var path in filePaths)
        {
            if (!File.Exists(path)) continue;

            // Evitar duplicados en la lista de UI
            if (FilesToProcess.Any(f => f.FilePath == path)) continue;

            FilesToProcess.Add(new IngestionItemViewModel
            {
                FileName = Path.GetFileName(path),
                FilePath = path,
                Status = "Pendiente"
            });
        }
        UpdateStatusMessage();
    }

    [RelayCommand]
    private void ClearFiles()
    {
        FilesToProcess.Clear();
        UpdateStatusMessage();
    }

    [RelayCommand]
    private async Task ProcessFilesAsync()
    {
        if (!FilesToProcess.Any())
        {
            MessageBox.Show("No hay archivos para procesar.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        IsProcessing = true;
        StatusMessage = "Procesando documentos, por favor espere...";

        var rawFiles = new System.Collections.Generic.List<RawIngestionFile>();

        foreach (var file in FilesToProcess)
        {
            file.Status = "Leyendo...";
            var bytes = await File.ReadAllBytesAsync(file.FilePath);
            rawFiles.Add(new RawIngestionFile(file.FileName, bytes));
        }

        var command = new IngestDtesCommand(rawFiles);
        var result = await _mediator.Send(command);

        if (result.IsSuccess)
        {
            var dto = result.Value;
            foreach (var file in FilesToProcess)
            {
                file.Status = "Procesado";
            }
            
            MessageBox.Show($"Procesamiento masivo finalizado.\n\n" +
                            $"Total Procesados: {dto.TotalProcessed}\n" +
                            $"Importados Exitosamente: {dto.TotalInserted}\n" +
                            $"Duplicados (Ignorados): {dto.TotalDuplicates}\n" +
                            $"Errores: {dto.TotalErrors}", 
                            "Reporte de Ingesta", MessageBoxButton.OK, MessageBoxImage.Information);
            
            ClearFiles();
        }
        else
        {
            MessageBox.Show($"Hubo un error al ejecutar la ingesta masiva: {result.Error.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        IsProcessing = false;
        UpdateStatusMessage();
    }

    private void UpdateStatusMessage()
    {
        if (IsProcessing) return;
        StatusMessage = FilesToProcess.Count > 0 
            ? $"{FilesToProcess.Count} archivos listos para procesar." 
            : "Arrastra y suelta tus archivos .json o imágenes aquí.";
    }
}

public partial class IngestionItemViewModel : ObservableObject
{
    [ObservableProperty]
    private string _fileName = string.Empty;

    [ObservableProperty]
    private string _filePath = string.Empty;

    [ObservableProperty]
    private string _status = string.Empty;
}
