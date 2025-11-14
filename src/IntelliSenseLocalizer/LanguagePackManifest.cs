using System.Collections.Immutable;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IntelliSenseLocalizer;

/// <summary>
/// 语言包清单信息
/// 描述语言包的元数据和内容信息
/// </summary>
/// <param name="version">清单版本</param>
/// <param name="moniker">目标框架名称</param>
/// <param name="cultureName">区域名称</param>
/// <param name="contentCompareType">内容比较类型</param>
/// <param name="packs">包含的包列表</param>
/// <param name="metadata">元数据字典</param>
public class LanguagePackManifest(uint version, string moniker, string cultureName, ContentCompareType contentCompareType, IReadOnlyList<string> packs, IReadOnlyDictionary<string, string> metadata)
{
    /// <summary>
    /// 当前清单版本号
    /// </summary>
    public const uint CurrentVersion = 1;

    /// <summary>
    /// 清单文件名
    /// </summary>
    public const string ManifestFileName = "islocalizer.manifest.json";

    /// <summary>
    /// JSON 序列化选项
    /// </summary>
    private static readonly JsonSerializerOptions s_jsonSerializerOptions = new()
    {
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        IncludeFields = true,
        WriteIndented = true,
        PropertyNameCaseInsensitive = true,
    };

    /// <summary>
    /// 区域信息（从 CultureName 转换）
    /// </summary>
    [JsonIgnore]
    public CultureInfo Culture => CultureInfo.GetCultureInfo(CultureName);

    /// <summary>
    /// 区域名称（如 zh-cn、en-us）
    /// </summary>
    public string CultureName { get; } = cultureName;

    /// <summary>
    /// 元数据字典
    /// </summary>
    public IReadOnlyDictionary<string, string> Metadata { get; } = metadata.ToImmutableDictionary();

    /// <summary>
    /// 包含的包列表
    /// </summary>
    public IReadOnlyList<string> Packs { get; } = packs.ToImmutableArray();

    /// <summary>
    /// 清单版本
    /// </summary>
    public uint Version { get; } = version;

    /// <summary>
    /// 目标框架名称（如 net6.0、net8.0）
    /// </summary>
    public string Moniker { get; } = moniker;

    /// <summary>
    /// 内容比较类型
    /// </summary>
    public ContentCompareType ContentCompareType { get; } = contentCompareType;

    /// <summary>
    /// 从 JSON 字符串创建 LanguagePackManifest 对象
    /// </summary>
    /// <param name="json">JSON 字符串</param>
    /// <returns>LanguagePackManifest 对象</returns>
    /// <exception cref="ArgumentException">当 JSON 格式无效时抛出</exception>
    /// <exception cref="InvalidOperationException">当版本不受支持时抛出</exception>
    public static LanguagePackManifest FromJson(string json)
    {
        var jsonDocument = JsonDocument.Parse(json);
        if (!jsonDocument.RootElement.TryGetProperty("Version", out var versionNode)
            || !versionNode.TryGetUInt32(out var version))
        {
            throw new ArgumentException($"not found field \"Version\" in json \"{json}\"");
        }

        // 根据版本号选择不同的反序列化逻辑
        return version switch
        {
            CurrentVersion => jsonDocument.Deserialize<LanguagePackManifest>()!,
            _ => throw new InvalidOperationException($"unsupported version \"{version}\""),
        };
    }

    /// <summary>
    /// 将 LanguagePackManifest 对象序列化为 JSON 字符串
    /// </summary>
    /// <returns>JSON 字符串</returns>
    public string ToJson()
    {
        return JsonSerializer.Serialize(this, GetType(), s_jsonSerializerOptions);
    }
}
