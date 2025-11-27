using System;
using System.Globalization;
using Avalonia.Data.Converters;
using static OllamaInteract.GUI.ViewModels.MainWindowViewModel;

namespace OllamaInteract.GUI.Converters;

public class ModelWindowModeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is ModelWindowMode Mode)
        {
            if (parameter is string targetModeString &&
                Enum.TryParse(typeof(ModelWindowMode), targetModeString, out var targetMode))
            {
                return Mode == (ModelWindowMode)targetMode;
            }
        }
        return false;
    }
    
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
