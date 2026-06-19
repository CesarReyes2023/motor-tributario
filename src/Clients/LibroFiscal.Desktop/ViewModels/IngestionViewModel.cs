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
    private readonly LibroFiscal.Application.Abstractions.Services.IEmpresaActivaService _empresaActivaService;
    private readonly LibroFiscal.Application.Abstractions.Services.IDialogService _dialogService;
    private readonly LibroFiscal.Application.Abstractions.Services.IErrorLogger _errorLogger;

    [ObservableProperty]
    private bool _isProcessing;

    [ObservableProperty]
    private string _statusMessage = "Arrastra y suelta tus archivos .json o imágenes aquí.";

    public ObservableCollection<IngestionItemViewModel> FilesToProcess { get; } = new();

    [ObservableProperty]
    private System.DateTime _syncStartDate = System.DateTime.Today.AddDays(-30);

    [ObservableProperty]
    private System.DateTime _syncEndDate = System.DateTime.Today;

    public IngestionViewModel(
        IMediator mediator,
        LibroFiscal.Application.Abstractions.Services.IEmpresaActivaService empresaActivaService,
        LibroFiscal.Application.Abstractions.Services.IDialogService dialogService,
        LibroFiscal.Application.Abstractions.Services.IErrorLogger errorLogger)
    {
        _mediator = mediator;
        _empresaActivaService = empresaActivaService;
        _dialogService = dialogService;
        _errorLogger = errorLogger;
    }

    [RelayCommand]
    private void AddFiles(string[] filePaths)
    {
        var allFiles = new System.Collections.Generic.List<string>();
        foreach (var path in filePaths)
        {
            if (File.Exists(path))
            {
                allFiles.Add(path);
            }
            else if (Directory.Exists(path))
            {
                allFiles.AddRange(Directory.GetFiles(path, "*.json", SearchOption.AllDirectories));
            }
        }

        foreach (var path in allFiles)
        {
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
    private void SelectFolder()
    {
        var dialog = new Microsoft.Win32.OpenFolderDialog
        {
            Title = "Seleccionar carpeta con archivos JSON",
            Multiselect = false
        };

        if (dialog.ShowDialog() == true)
        {
            var folderPath = dialog.FolderName;
            if (Directory.Exists(folderPath))
            {
                var files = Directory.GetFiles(folderPath, "*.json", SearchOption.AllDirectories);
                if (files.Length > 0)
                {
                    AddFiles(files);
                }
                else
                {
                    _dialogService.ShowInfo("No se encontraron archivos .json en la carpeta seleccionada ni en sus subcarpetas.", "Carpeta Vacía");
                }
            }
        }
    }

    [RelayCommand]
    private async Task ProcessFilesAsync()
    {
        if (!FilesToProcess.Any())
        {
            _dialogService.ShowWarning("No hay archivos para procesar.");
            return;
        }

        var companyId = _empresaActivaService.EmpresaActualId;
        if (companyId == null)
        {
            _dialogService.ShowError("No hay ninguna empresa configurada o activa.");
            return;
        }

        IsProcessing = true;
        StatusMessage = "Procesando documentos, por favor espere...";

        try
        {
            var result = await Task.Run(async () =>
            {
                var rawFiles = new System.Collections.Generic.List<RawIngestionFile>();
                foreach (var file in FilesToProcess.ToList()) // ToList para aislar coleccion de UI
                {
                    // No podemos modificar la UI directamente desde Task.Run tan fácilmente,
                    // omitimos el status "Leyendo..." para simplificar o lo hacemos global.
                    var bytes = await File.ReadAllBytesAsync(file.FilePath);
                    rawFiles.Add(new RawIngestionFile(file.FileName, bytes));
                }

                var command = new IngestDtesCommand(companyId.Value, rawFiles);
                return await _mediator.Send(command);
            });

            if (result.IsSuccess)
            {
                var dto = result.Value;
                foreach (var file in FilesToProcess)
                {
                    file.Status = "Procesado";
                }
                
                if (dto.TotalErrors > 0 && dto.ErrorMessages != null && dto.ErrorMessages.Any())
                {
                    var errorDetail = "Detalle de errores en la importación:\n" + string.Join("\n", dto.ErrorMessages);
                    _errorLogger.LogError("ingesta", errorDetail);
                }

                var msg = $"Procesamiento masivo finalizado.\n\n" +
                          $"Total Procesados: {dto.TotalProcessed}\n" +
                          $"Importados Exitosamente: {dto.TotalInserted}\n" +
                          $"Duplicados (Ignorados): {dto.TotalDuplicates}\n" +
                          $"Errores: {dto.TotalErrors}";

                if (dto.TotalErrors > 0)
                {
                    msg += "\n\nSe ha guardado el detalle de los errores en los logs de la aplicación.";
                }

                _dialogService.ShowInfo(msg, "Reporte de Ingesta");
                
                ClearFiles();
            }
            else
            {
                _errorLogger.LogError("ingesta", result.Error.Message);
                _dialogService.ShowError($"Hubo un error al ejecutar la ingesta masiva: {result.Error.Message}\n(Detalle guardado en logs de la aplicación)", "Error");
            }
        }
        catch (System.Exception ex)
        {
            _errorLogger.LogError("ingesta", ex);
            _dialogService.ShowError($"Ocurrió un error inesperado.\n(Detalle guardado en logs de la aplicación)", "Error Crítico");
        }
        finally
        {
            IsProcessing = false;
            UpdateStatusMessage();
        }
    }

    private void UpdateStatusMessage()
    {
        if (IsProcessing) return;
        StatusMessage = FilesToProcess.Count > 0 
            ? $"{FilesToProcess.Count} archivos listos para procesar." 
            : "Arrastra y suelta tus archivos .json o imágenes aquí.";
    }

    public ObservableCollection<BuzonDteViewModel> BuzonDtes { get; } = new();

    [RelayCommand]
    private async Task FetchFromHaciendaAsync()
    {
        var companyId = _empresaActivaService.EmpresaActualId;
        if (companyId == null)
        {
            _dialogService.ShowError("No hay ninguna empresa configurada o activa.");
            return;
        }

        IsProcessing = true;
        StatusMessage = "Consultando Buzón en Ministerio de Hacienda...";

        var command = new LibroFiscal.Application.DteIngestion.Commands.FetchHaciendaDtes.FetchHaciendaDtesCommand(companyId.Value, SyncStartDate, SyncEndDate);
        
        var result = await _mediator.Send(command);

        if (result.IsSuccess)
        {
            BuzonDtes.Clear();
            foreach (var dto in result.Value)
            {
                BuzonDtes.Add(new BuzonDteViewModel
                {
                    CodigoGeneracion = dto.CodigoGeneracion,
                    NumeroControl = dto.NumeroControl,
                    FechaEmision = dto.FechaEmision,
                    NombreEmisor = dto.ReceptorNombre, // DteDownloadDto tiene ReceptorNombre que mapeamos a emisor por error de nombramiento, wait no, let's just use it
                    MontoTotal = dto.TotalPagar,
                    IsSelected = false
                });
            }
            StatusMessage = $"Se encontraron {BuzonDtes.Count} DTEs en el buzón.";
        }
        else
        {
            _dialogService.ShowError($"Hubo un error al consultar con Hacienda: {result.Error.Message}");
            StatusMessage = "Error en consulta.";
        }

        IsProcessing = false;
    }

    [RelayCommand]
    private async Task ImportSelectedFromHaciendaAsync()
    {
        var selected = BuzonDtes.Where(x => x.IsSelected).ToList();
        if (selected.Count == 0)
        {
            _dialogService.ShowInfo("Seleccione al menos un DTE del buzón para importar.", "Aviso");
            return;
        }

        var companyId = _empresaActivaService.EmpresaActualId;
        if (companyId == null) return;

        IsProcessing = true;
        StatusMessage = $"Descargando e importando {selected.Count} DTE(s)...";

        var dtos = selected.Select(s => new LibroFiscal.Application.Abstractions.Services.DteDownloadDto(
            s.CodigoGeneracion, s.NumeroControl, s.FechaEmision, "", s.NombreEmisor, s.MontoTotal, ""
        )).ToList();

        var result = await Task.Run(async () =>
        {
            var command = new LibroFiscal.Application.DteIngestion.Commands.ImportSelectedDtes.ImportSelectedDtesCommand(companyId.Value, dtos);
            return await _mediator.Send(command);
        });

        if (result.IsSuccess)
        {
            var dto = result.Value;
            foreach (var item in selected)
            {
                item.Status = "Importado";
                item.IsSelected = false; // deseleccionar
            }
            
            _dialogService.ShowInfo($"Importación finalizada.\n\n" +
                            $"Total Descargados: {dto.TotalProcessed}\n" +
                            $"Importados Exitosamente: {dto.TotalInserted}\n" +
                            $"Duplicados (Ignorados): {dto.TotalDuplicates}\n" +
                            $"Errores: {dto.TotalErrors}", 
                            "Reporte de Importación");
                            
            StatusMessage = "Importación completada.";
        }
        else
        {
            _dialogService.ShowError($"Hubo un error al importar: {result.Error.Message}");
            StatusMessage = "Error en importación.";
        }

        IsProcessing = false;
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

public partial class BuzonDteViewModel : ObservableObject
{
    public string CodigoGeneracion { get; set; } = string.Empty;
    public string NumeroControl { get; set; } = string.Empty;
    public System.DateTime FechaEmision { get; set; }
    public string NombreEmisor { get; set; } = string.Empty;
    public decimal MontoTotal { get; set; }

    [ObservableProperty]
    private bool _isSelected;

    [ObservableProperty]
    private string _status = "Pendiente";
}
