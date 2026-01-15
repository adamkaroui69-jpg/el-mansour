using System;
using System.Globalization;
using System.Windows.Data;
using MaterialDesignThemes.Wpf;

namespace ElMansourSyndicManager.Converters;

public class FileIconConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string fileName)
        {
            var ext = System.IO.Path.GetExtension(fileName).ToLower();
            return ext switch
            {
                ".pdf" => PackIconKind.FilePdfBox,
                ".doc" or ".docx" => PackIconKind.FileWord,
                ".xls" or ".xlsx" => PackIconKind.FileExcel,
                ".ppt" or ".pptx" => PackIconKind.FilePowerpoint,
                ".jpg" or ".jpeg" or ".png" or ".gif" => PackIconKind.FileImage,
                ".txt" => PackIconKind.FileDocument,
                ".zip" or ".rar" => PackIconKind.FolderZip,
                _ => PackIconKind.File
            };
        }
        return PackIconKind.File;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
