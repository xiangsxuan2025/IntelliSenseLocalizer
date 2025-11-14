using System.Globalization;

namespace IntelliSenseLocalizer.ThirdParty;

/// <summary>
/// 翻译上下文类
/// 包含翻译操作所需的所有配置和信息
/// </summary>
public class TranslateContext
{
    #region Public 属性

    /// <summary>
    /// 内容比较类型
    /// </summary>
    public ContentCompareType ContentCompareType { get; }

    /// <summary>
    /// 内容翻译器实例
    /// </summary>
    public IContentTranslator ContentTranslator { get; }

    /// <summary>
    /// 要翻译的 XML 文件路径
    /// </summary>
    public string FilePath { get; set; }

    /// <summary>
    /// 是否为补丁模式
    /// </summary>
    public bool IsPatch { get; }

    /// <summary>
    /// 输出文件路径
    /// </summary>
    public string OutputPath { get; }

    /// <summary>
    /// 并行处理数量
    /// </summary>
    public int ParallelCount { get; set; } = 2;

    /// <summary>
    /// 分隔线内容
    /// </summary>
    public string? SeparateLine { get; }

    /// <summary>
    /// 源区域信息
    /// </summary>
    public CultureInfo SourceCultureInfo { get; }

    /// <summary>
    /// 目标区域信息
    /// </summary>
    public CultureInfo TargetCultureInfo { get; }

    #endregion Public 属性

    /// <summary>
    /// 初始化翻译上下文
    /// </summary>
    /// <param name="filePath">要翻译的 XML 文件路径</param>
    /// <param name="contentCompareType">内容比较类型</param>
    /// <param name="separateLine">分隔线内容</param>
    /// <param name="outputPath">输出文件路径</param>
    /// <param name="sourceCultureInfo">源区域信息</param>
    /// <param name="targetCultureInfo">目标区域信息</param>
    /// <param name="contentTranslator">内容翻译器实例</param>
    /// <param name="isPatch">是否配对？</param>
    public TranslateContext(string filePath, ContentCompareType contentCompareType, string? separateLine, string outputPath, CultureInfo sourceCultureInfo, CultureInfo targetCultureInfo, IContentTranslator contentTranslator, bool isPatch)
    {
        FilePath = filePath;
        ContentCompareType = contentCompareType;
        OutputPath = outputPath;
        SeparateLine = separateLine;
        SourceCultureInfo = sourceCultureInfo ?? throw new ArgumentNullException(nameof(sourceCultureInfo));
        TargetCultureInfo = targetCultureInfo ?? throw new ArgumentNullException(nameof(targetCultureInfo));
        ContentTranslator = contentTranslator;
        IsPatch = isPatch;
    }
}
