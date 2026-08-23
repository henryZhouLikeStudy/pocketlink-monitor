using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PocketLink.App.Views;

/// <summary>
/// bool? -> Visibility：true 显示，false/null 均隐藏，避免在 fallback 标记为 null 时误显示。
/// </summary>
public sealed class NullableBoolToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        value is true ? Visibility.Visible : Visibility.Collapsed;

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
