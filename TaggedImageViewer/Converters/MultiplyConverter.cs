using System.Globalization;
using System.Windows.Data;

namespace TaggedImageViewer.Converters;

public class MultiplyConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is double doubleValue && parameter is double multiplier)
            return doubleValue * multiplier;
        
        return value;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}