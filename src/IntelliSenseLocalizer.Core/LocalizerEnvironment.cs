using System.Globalization;

namespace IntelliSenseLocalizer;

/// <summary>
/// 本地化器环境配置
/// 提供应用程序运行所需的各种路径和配置信息
/// </summary>
public static class LocalizerEnvironment
{
    /// <summary>
    /// 构建根目录
    /// </summary>
    public static string BuildRoot { get; }

    /// <summary>
    /// 缓存根目录
    /// </summary>
    public static string CacheRoot { get; }

    /// <summary>
    /// 当前区域设置
    /// </summary>
    public static string CurrentLocale { get; }

    /// <summary>
    /// 默认 SDK 根目录
    /// </summary>
    public static string DefaultSdkRoot { get; }

    /// <summary>
    /// 日志根目录
    /// </summary>
    public static string LogRoot { get; }

    /// <summary>
    /// 输出根目录
    /// </summary>
    public static string OutputRoot { get; }

    /// <summary>
    /// 工作根目录
    /// </summary>
    public static string WorkRootDirectory { get; }

    /// <summary>
    /// 静态构造函数，初始化所有环境路径和配置
    /// </summary>
    static LocalizerEnvironment()
    {
        // 设置工作根目录（系统临时目录下的 IntelliSenseLocalizer）
        WorkRootDirectory = Path.Combine(Path.GetTempPath(), "IntelliSenseLocalizer");

        // 初始化各功能目录
        LogRoot = Path.Combine(WorkRootDirectory, "logs");
        OutputRoot = Path.Combine(WorkRootDirectory, "output");
        CacheRoot = Path.Combine(WorkRootDirectory, "cache");
        BuildRoot = Path.Combine(WorkRootDirectory, "build");

        // 确保所有目录存在
        CheckDirectories();

        // 设置当前区域设置（小写）
        CurrentLocale = CultureInfo.CurrentCulture.Name.ToLowerInvariant();

        // 获取默认 SDK 根目录（第一个安装的 SDK 路径）
        DefaultSdkRoot = DotNetEnvironmentUtil.GetAllInstalledSDKPaths().FirstOrDefault() ?? string.Empty;
    }

    /// <summary>
    /// 检查并创建所有必要的目录
    /// </summary>
    public static void CheckDirectories()
    {
        DirectoryUtil.CheckDirectory(WorkRootDirectory);
        DirectoryUtil.CheckDirectory(LogRoot);
        DirectoryUtil.CheckDirectory(OutputRoot);
        DirectoryUtil.CheckDirectory(CacheRoot);
        DirectoryUtil.CheckDirectory(BuildRoot);
    }
}
