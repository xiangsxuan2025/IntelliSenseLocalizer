using System.Globalization;
using Avalonia;
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
        return s_cache ??= new Dictionary<KeepEnglishMode, Bitmap>
        {
            { KeepEnglishMode.KeepEnglishAhead, LoadBitmap("KeepEnglishAhead.png") },
            { KeepEnglishMode.KeepEnglishAfter, LoadBitmap("KeepEnglishAfter.png") },
            { KeepEnglishMode.DontKeepEnglish, LoadBitmap("DontKeepEnglish.png") }
        };
    }

    private Bitmap LoadBitmap(string fileName)
    {
        try
        {
            // 尝试从资源加载
            var uri = new Uri($"avares://IntelliSenseLocalizer.App/Assets/{fileName}");
            return new Bitmap(AssetLoader.Open(uri));
        }
        catch
        {
            // 如果资源加载失败，创建一个简单的占位位图
            return CreatePlaceholderBitmap();
        }
    }

    private Bitmap CreatePlaceholderBitmap()
    {
        // 创建一个简单的 16x16 的占位位图
        var writeableBitmap = new WriteableBitmap(new PixelSize(16, 16), new Vector(96, 96), PixelFormat.Bgra8888);
        using (var frameBuffer = writeableBitmap.Lock())
        {
            unsafe
            {
                var ptr = (uint*)frameBuffer.Address;
                for (int y = 0; y < 16; y++)
                {
                    for (int x = 0; x < 16; x++)
                    {
                        ptr[y * 16 + x] = 0xFF808080; // 灰色
                    }
                }
            }
        }
        return writeableBitmap;
    }

    private static Dictionary<KeepEnglishMode, Bitmap>? s_cache;
}
