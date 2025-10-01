using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace UndauntedPledges.Converters;

public class VisibilityConverter : IValueConverter
{
    /// <inheritdoc />
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not bool valueAsBool)
        {
            return Visibility.Visible;
        }

        valueAsBool = parameter != null ? !valueAsBool : valueAsBool;

        return valueAsBool ? Visibility.Visible : Visibility.Collapsed;
    }

    /// <inheritdoc />
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not Visibility visibility)
        {
            return true;
        }

        var isParameterIsNotNull = parameter != null;

        return visibility == Visibility.Hidden || visibility == Visibility.Collapsed
            ? isParameterIsNotNull
            : !isParameterIsNotNull;
    }
}