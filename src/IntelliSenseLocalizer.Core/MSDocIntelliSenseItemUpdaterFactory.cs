using Microsoft.Extensions.Logging;

namespace IntelliSenseLocalizer;

/// <summary>
/// 基于微软文档的 IntelliSense 项更新器工厂
/// 负责创建基于微软文档的更新器实例
/// </summary>
public class MSDocIntelliSenseItemUpdaterFactory : IIntelliSenseItemUpdaterFactory
{
    private readonly ILoggerFactory _loggerFactory;

    /// <summary>
    /// 初始化基于微软文档的更新器工厂
    /// </summary>
    /// <param name="loggerFactory">日志工厂</param>
    public MSDocIntelliSenseItemUpdaterFactory(ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
    }

    /// <summary>
    /// 获取适用于指定生成上下文的更新器
    /// </summary>
    /// <param name="generateContext">生成上下文</param>
    /// <returns>基于微软文档的 IntelliSense 项更新器</returns>
    public IIntelliSenseItemUpdater GetUpdater(GenerateContext generateContext)
    {
        return new MSDocIntelliSenseItemUpdater(generateContext, _loggerFactory.CreateLogger<MSDocIntelliSenseItemUpdater>());
    }
}
