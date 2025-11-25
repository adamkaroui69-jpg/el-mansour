using System.Globalization;
using System.Windows.Data;
using MaterialDesignThemes.Wpf;

namespace ElMansourSyndicManager.Converters;

/// <summary>
/// Converts notification filter string to French display text
/// </summary>
public class NotificationFilterConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value?.ToString() switch
        {
            "All" => "Toutes",
            "Unread" => "Non lues",
            "Read" => "Lues",
            _ => value?.ToString() ?? string.Empty
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value?.ToString() switch
        {
            "Toutes" => "All",
            "Non lues" => "Unread",
            "Lues" => "Read",
            _ => value?.ToString() ?? string.Empty
        };
    }
}

/// <summary>
/// Converts notification type to French display text
/// </summary>
public class NotificationTypeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value?.ToString() switch
        {
            "All" => "Tous les types",
            "UnpaidHouse" => "Maisons Non Payées",
            "MaintenanceDue" => "Maintenance",
            "System" => "Système",
            "Info" => "Information",
            _ => value?.ToString() ?? string.Empty
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value?.ToString() switch
        {
            "Tous les types" => "All",
            "Maisons Non Payées" => "UnpaidHouse",
            "Maintenance" => "MaintenanceDue",
            "Système" => "System",
            "Information" => "Info",
            _ => value?.ToString() ?? string.Empty
        };
    }
}

/// <summary>
/// Converts notification type to PackIcon kind
/// </summary>
public class NotificationIconConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // This is a simplified version - in a real implementation, you'd need to convert to PackIconKind enum
        // For now, return a string that can be used in XAML with a proper converter
        return value?.ToString() switch
        {
            "UnpaidHouse" => "CashAlert",
            "MaintenanceDue" => "Tools",
            "System" => "Information",
            "Info" => "Information",
            _ => "Bell"
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts priority string to color
/// </summary>
public class PriorityColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var color = value?.ToString() switch
        {
            "Urgent" => "#FF0000",
            "High" => "#FF8800",
            "Normal" => "#2196F3",
            "Low" => "#4CAF50",
            _ => "#757575"
        };
        
        return System.Windows.Media.ColorConverter.ConvertFromString(color);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts boolean to inverse visibility
/// </summary>
public class InverseBooleanToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            return boolValue ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;
        }
        return System.Windows.Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is System.Windows.Visibility visibility)
        {
            return visibility != System.Windows.Visibility.Visible;
        }
        return false;
    }
}

/// <summary>
/// Converts boolean IsRead to background color
/// </summary>
public class NotificationBackgroundConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isRead && !isRead)
        {
            // Light blue background for unread notifications
            return new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#E3F2FD"));
        }
        // Transparent/White for read notifications
        return System.Windows.Media.Brushes.White;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

