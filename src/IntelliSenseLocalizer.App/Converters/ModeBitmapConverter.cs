using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace IntelliSenseLocalizer.App.Converters;

public class ModeBitmapConverter : IValueConverter
{
    public static ModeBitmapConverter Instance { get; } = new();

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return GetCache()[(KeepEnglishMode)value];
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }

    private Dictionary<KeepEnglishMode, Bitmap> GetCache()
    {
        return s_cache ??= Enum.GetValues(typeof(KeepEnglishMode)).OfType<KeepEnglishMode>().ToDictionary(
            t => t,
            t => new Bitmap(AssetLoader.Open(new Uri($"avares://IntelliSenseLocalizer.App/Assets/{t}.png"))));
    }
    private static Dictionary<KeepEnglishMode, Bitmap>? s_cache;
}
