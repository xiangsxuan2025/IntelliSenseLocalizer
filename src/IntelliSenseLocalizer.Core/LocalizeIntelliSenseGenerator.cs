using System.Globalization;
using System.Xml;

using IntelliSenseLocalizer.Models;

using Microsoft.Extensions.Logging;

namespace IntelliSenseLocalizer;

/// <summary>
/// 生成本地化 IntelliSense 的上下文信息
/// </summary>
public class GenerateContext
{
    /// <summary>
    /// 内容比较类型
    /// </summary>
    public ContentCompareType ContentCompareType { get; }

    /// <summary>
    /// 区域信息
    /// </summary>
    public CultureInfo CultureInfo { get; }

    /// <summary>
    /// IntelliSense 文件描述符
    /// </summary>
    public IntelliSenseFileDescriptor Descriptor { get; }

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
    /// 初始化生成上下文
    /// </summary>
    /// <param name="descriptor">IntelliSense 文件描述符</param>
    /// <param name="contentCompareType">内容比较类型</param>
    /// <param name="separateLine">分隔线内容</param>
    /// <param name="outputPath">输出文件路径</param>
    /// <param name="cultureInfo">区域信息</param>
    /// <exception cref="ArgumentException">当输出路径为空时抛出</exception>
    /// <exception cref="ArgumentNullException">当描述符或区域信息为空时抛出</exception>
    public GenerateContext(IntelliSenseFileDescriptor descriptor, ContentCompareType contentCompareType, string? separateLine, string outputPath, CultureInfo cultureInfo)
    {
        if (string.IsNullOrEmpty(outputPath))
        {
            throw new ArgumentException($"“{nameof(outputPath)}”不能为 null 或空。", nameof(outputPath));
        }

        Descriptor = descriptor ?? throw new ArgumentNullException(nameof(descriptor));
        ContentCompareType = contentCompareType;
        OutputPath = outputPath;
        SeparateLine = separateLine;
        CultureInfo = cultureInfo ?? throw new ArgumentNullException(nameof(cultureInfo));
    }
}

/// <summary>
/// 本地化 IntelliSense 生成器
/// 负责协调整个本地化生成过程
/// </summary>
public class LocalizeIntelliSenseGenerator
{
    private readonly IIntelliSenseItemProvider _intelliSenseItemProvider;
    private readonly IIntelliSenseItemUpdaterFactory _intelliSenseItemUpdaterFactory;
    private readonly ILogger _logger;

    /// <summary>
    /// 初始化本地化 IntelliSense 生成器
    /// </summary>
    /// <param name="intelliSenseItemProvider">IntelliSense 项提供者</param>
    /// <param name="intelliSenseItemUpdaterFactory">IntelliSense 项更新器工厂</param>
    /// <param name="logger">日志记录器</param>
    public LocalizeIntelliSenseGenerator(IIntelliSenseItemProvider intelliSenseItemProvider,
                                         IIntelliSenseItemUpdaterFactory intelliSenseItemUpdaterFactory,
                                         ILogger<LocalizeIntelliSenseGenerator> logger)
    {
        _intelliSenseItemProvider = intelliSenseItemProvider ?? throw new ArgumentNullException(nameof(intelliSenseItemProvider));
        _intelliSenseItemUpdaterFactory = intelliSenseItemUpdaterFactory ?? throw new ArgumentNullException(nameof(intelliSenseItemUpdaterFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// 异步生成本地化 IntelliSense 文件
    /// </summary>
    /// <param name="context">生成上下文</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>异步任务</returns>
    public virtual async Task GenerateAsync(GenerateContext context, CancellationToken cancellationToken)
    {
        // 加载原始 XML 文档
        var xmlDocument = new XmlDocument();
        xmlDocument.Load(context.Descriptor.FilePath);

        // 获取更新器
        using var intelliSenseItemUpdater = _intelliSenseItemUpdaterFactory.GetUpdater(context);

        // 获取所有 IntelliSense 项
        var intelliSenseItems = _intelliSenseItemProvider.GetItems(xmlDocument, context.Descriptor).ToList();

        // 配置并行选项
        var parallelOptions = new ParallelOptions()
        {
            MaxDegreeOfParallelism = context.ParallelCount > 0 ? context.ParallelCount : 1,
            CancellationToken = cancellationToken,
        };

        // 按查询键分组并反转顺序（优先处理复杂的项）
        var groups = intelliSenseItems.GroupBy(x => x.GetMicrosoftDocsQueryKey()).Reverse().ToArray();

        // 并行处理每个组
        await Parallel.ForEachAsync(groups, parallelOptions, async (intelliSenseItemGroup, token) =>
        {
            try
            {
                await intelliSenseItemUpdater.UpdateAsync(intelliSenseItemGroup, token);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Update IntelliSenseFile Group [{Key}]", intelliSenseItemGroup.Key);
            }
        });

        // 确保输出目录存在并保存文件
        var outDir = Path.GetDirectoryName(context.OutputPath);
        DirectoryUtil.CheckDirectory(outDir);

        _logger.LogDebug("[{Name}] processing completed. Save the file into {OutputPath}.", context.Descriptor.Name, context.OutputPath);

        xmlDocument.Save(context.OutputPath);
    }
}
