using System;
using System.Linq;
using System.Windows;

namespace LibroFiscal.Desktop.Services;

public static class ThemeService
{
    private static bool _isDark;

    public static bool IsDark => _isDark;

    public static void ToggleTheme()
    {
        _isDark = !_isDark;
        
        string themeName = _isDark ? "DarkTheme" : "LightTheme";
        string sourceUri = $"pack://application:,,,/LibroFiscal.Desktop;component/Themes/{themeName}.xaml";

        var dictionaries = System.Windows.Application.Current.Resources.MergedDictionaries;
        
        // Find existing theme dictionary
        var existingTheme = dictionaries.FirstOrDefault(d => 
            d.Source != null && d.Source.OriginalString.Contains("Theme.xaml"));

        var newTheme = new ResourceDictionary() { Source = new Uri(sourceUri, UriKind.Absolute) };

        if (existingTheme != null)
        {
            // Reemplazar el diccionario existente
            int index = dictionaries.IndexOf(existingTheme);
            dictionaries.Insert(index, newTheme);
            dictionaries.Remove(existingTheme);
        }
        else
        {
            // Agregar al final
            dictionaries.Add(newTheme);
        }
    }
}
