using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using LibroFiscal.Application.Abstractions.Services;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;

namespace LibroFiscal.Integrations.Pdf;

/// <summary>
/// ponytail: Generador de PDF nativo. Utiliza el WebView2 de Microsoft Edge que ya viene 
/// en Windows 10/11 para evitar instalar Chromium (Puppeteer) o WkHtmlToPdf.
/// Esto cumple la regla: "Does a native platform feature cover it? Use it."
/// Como WebView2 es un componente UI, creamos un hilo STA invisible para procesarlo.
/// </summary>
public sealed class HtmlToPdfGenerator : IHtmlToPdfGenerator
{
    public async Task<byte[]> GeneratePdfAsync(string htmlContent, CancellationToken cancellationToken = default)
    {
        var tcs = new TaskCompletionSource<byte[]>();
        
        // El WebView2 requiere un hilo STA (Single-Threaded Apartment)
        var thread = new Thread(() =>
        {
            try
            {
                GeneratePdfSta(htmlContent, tcs).GetAwaiter().GetResult();
                
                // Mensaje de bomba de WPF (necesario para eventos de WebView2)
                System.Windows.Threading.Dispatcher.Run();
            }
            catch (Exception ex)
            {
                tcs.TrySetException(ex);
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.IsBackground = true;
        thread.Start();

        // En cuanto el PDF esté listo (o falle), liberamos la bomba de mensajes del hilo STA
        var resultBytes = await tcs.Task;
        
        // Cerramos el hilo
        System.Windows.Threading.Dispatcher.FromThread(thread)?.InvokeShutdown();

        return resultBytes;
    }

    private static async Task GeneratePdfSta(string htmlContent, TaskCompletionSource<byte[]> tcs)
    {
        WebView2 webView = null!;
        try
        {
            webView = new WebView2();
            
            // Inicializar el core
            var env = await CoreWebView2Environment.CreateAsync(null, Path.GetTempPath());
            await webView.EnsureCoreWebView2Async(env);

            // Suscribirse al evento de finalización de carga
            webView.NavigationCompleted += async (sender, args) =>
            {
                try
                {
                    if (!args.IsSuccess)
                    {
                        tcs.TrySetException(new InvalidOperationException($"WebView2 Navigation failed: {args.WebErrorStatus}"));
                        return;
                    }

                    // Imprimir a PDF usando la API de CoreWebView2
                    var tempFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.pdf");
                    
                    var printSettings = webView.CoreWebView2.Environment.CreatePrintSettings();
                    printSettings.ShouldPrintBackgrounds = true;
                    printSettings.ShouldPrintHeaderAndFooter = false;
                    printSettings.MarginBottom = 0;
                    printSettings.MarginTop = 0;
                    printSettings.MarginLeft = 0;
                    printSettings.MarginRight = 0;

                    var isPrinted = await webView.CoreWebView2.PrintToPdfAsync(tempFile, printSettings);

                    if (isPrinted && File.Exists(tempFile))
                    {
                        var bytes = await File.ReadAllBytesAsync(tempFile);
                        File.Delete(tempFile);
                        tcs.TrySetResult(bytes);
                    }
                    else
                    {
                        tcs.TrySetException(new InvalidOperationException("Failed to print HTML to PDF via WebView2."));
                    }
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            };

            // Iniciar la carga del HTML
            webView.NavigateToString(htmlContent);
        }
        catch (Exception ex)
        {
            tcs.TrySetException(ex);
        }
    }
}
