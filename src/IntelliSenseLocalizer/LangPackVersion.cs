using System.Globalization;
using System.Text.RegularExpressions;

namespace IntelliSenseLocalizer;

/// <summary>
/// 语言包版本类
/// 用于编码和解码语言包版本信息
/// </summary>
public class LangPackVersion
{
    /// <summary>
    /// 初始化语言包版本
    /// </summary>
    /// <param name="moniker">目标框架名称</param>
    /// <param name="time">创建时间</param>
    /// <param name="contentCompareType">内容比较类型</param>
    /// <param name="culture">区域信息</param>
    public LangPackVersion(string moniker, DateTime time, ContentCompareType contentCompareType, CultureInfo culture)
    {
        Moniker = moniker;
        Time = time;
        ContentCompareType = contentCompareType;
        Culture = culture;
    }

    /// <summary>
    /// 目标框架名称（如 net6.0、net8.0）
    /// </summary>
    public string Moniker { get; }

    /// <summary>
    /// 语言包创建时间
    /// </summary>
    public DateTime Time { get; }

    /// <summary>
    /// 内容比较类型
    /// </summary>
    public ContentCompareType ContentCompareType { get; }

    /// <summary>
    /// 区域信息
    /// </summary>
    public CultureInfo Culture { get; }

    /// <summary>
    /// 编码语言包版本为字符串格式
    /// 格式：{monikerVersion}.0-{cultureName}-{contentCompareType}-{timestamp}
    /// 示例：6.0.0-zh-cn-OriginFirst-20231114123045
    /// </summary>
    /// <returns>编码后的版本字符串</returns>
    public string Encode()
    {
        // 从 moniker 中提取版本号（如从 "net6.0" 中提取 "6.0"）
        var monikerVersion = Regex.Match(Moniker, "\\d+\\.+\\d").Value;

        return $"{monikerVersion}.0-{Culture.Name.ToLowerInvariant()}-{ContentCompareType}-{DateTime.UtcNow:yyyyMMddHHmmss}";
    }

    /// <summary>
    /// 从字符串解码语言包版本信息
    /// </summary>
    /// <param name="packVersionString">版本字符串</param>
    /// <returns>解码后的 LangPackVersion 对象</returns>
    /// <exception cref="ArgumentException">当版本字符串格式无效时抛出</exception>
    public static LangPackVersion Decode(string packVersionString)
    {
        var span = packVersionString.AsSpan();

        //TODO 友好错误处理

        // 解析版本号部分
        var index = span.IndexOf('-');
        var version = new Version(span.Slice(0, index).ToString());

        span = span.Slice(index + 1);

        // 解析时间戳部分
        index = span.LastIndexOf('-');
        var time = DateTime.ParseExact(span.Slice(index + 1, span.Length - index - 1), "yyyyMMddHHmmss", CultureInfo.InvariantCulture);

        span = span.Slice(0, index);

        // 解析内容比较类型
        index = span.LastIndexOf('-');
        var contentCompareType = (ContentCompareType)Enum.Parse(typeof(ContentCompareType), span.Slice(index + 1, span.Length - index - 1).ToString());

        span = span.Slice(0, index);

        // 解析区域信息
        var culture = CultureInfo.GetCultureInfo(span.ToString());

        // 构建 moniker（如 net6.0）
        return new LangPackVersion($"net{version.Major}.{version.Minor}", time, contentCompareType, culture);
    }
}
