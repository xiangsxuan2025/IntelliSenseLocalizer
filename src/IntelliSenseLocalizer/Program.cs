using System.CommandLine;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text.RegularExpressions;

using Cuture.Http;

using IntelliSenseLocalizer.Properties;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace IntelliSenseLocalizer;

/// <summary>
/// IntelliSenseLocalizer 主程序类
/// 负责命令行解析、依赖注入配置和新版本检查
/// </summary>
internal partial class Program
{
    /// <summary>
    /// 控制台日志级别开关
    /// </summary>
    private static readonly LoggingLevelSwitch s_consoleLoggingLevelSwitch = new(LogEventLevel.Verbose);

    /// <summary>
    /// 日志级别选项
    /// </summary>
    private static readonly Option<int?> s_logLevelOption = new(["-ll", "--log-level"], Resources.StringOptionLogLevelDescription);

    /// <summary>
    /// 日志记录器实例
    /// </summary>
    private static Microsoft.Extensions.Logging.ILogger s_logger = null!;

    /// <summary>
    /// 服务提供者实例
    /// </summary>
    private static IServiceProvider s_serviceProvider = null!;

    /// <summary>
    /// 应用程序主入口点
    /// </summary>
    /// <param name="args">命令行参数</param>
    /// <returns>退出代码</returns>
    private static int Main(string[] args)
    {
        // 检查新版本
        if (TryCheckNewVersion(out var newVersion))
        {
            Console.WriteLine("-----------------------");
            var colorBackup = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(string.Format(Resources.StringNewVersionFoundTip, newVersion.ToString()));
            Console.ForegroundColor = colorBackup;
            Console.WriteLine("-----------------------");
        }

        // 异步检查在线新版本（不等待）
        _ = TryGetNewVersionOnlineAsync();

        // 构建依赖注入容器
        s_serviceProvider = BuildServiceProvider();
        s_logger = s_serviceProvider.GetRequiredService<ILogger<Program>>();

        // 构建根命令
        var rootCommand = new RootCommand(Resources.StringRootCommandDescription)
        {
            BuildInstallCommand(),
            BuildUnInstallCommand(),
            BuildShowCommand(),
            BuildBuildCommand(),
            BuildClearCommand(),
            BuildCleanCommand(),
            BuildTranslateCommand(),
        };

        var customOption = new Option<string?>("--custom", "Custom addon options.");

        rootCommand.AddGlobalOption(s_logLevelOption);
        rootCommand.AddGlobalOption(customOption);

        // 执行命令
        var result = rootCommand.Invoke(args);

        var optionParseResult = customOption.Parse(args);

        // 处理自定义选项（如 delay-exit-20s）
        if (optionParseResult.CommandResult.GetValueForOption(customOption) is string customOptionString
            && Regex.Match(customOptionString, @"delay-exit-(\d+)s") is Match waitSecondsMatch
            && waitSecondsMatch.Groups.Count > 1
            && int.TryParse(waitSecondsMatch.Groups[1].Value, out var waitSeconds)
            && waitSeconds > 0)
        {
            DelayExitProcess(waitSeconds, 0);
        }

        return result;
    }

    #region Base

    /// <summary>
    /// 延迟退出进程
    /// </summary>
    /// <param name="waitSeconds">等待秒数</param>
    /// <param name="exitCode">退出代码</param>
    [DoesNotReturn]
    private static void DelayExitProcess(int waitSeconds, int exitCode)
    {
        new Thread(_ =>
        {
            Console.WriteLine($"Program will exit at {waitSeconds} seconds later or press enter to exit.");
            Thread.Sleep(waitSeconds * 1000);
            Environment.Exit(exitCode);
        })
        { IsBackground = true }
        .Start();
        Console.ReadLine();
        Environment.Exit(exitCode);
    }

    /// <summary>
    /// 构建服务提供者
    /// </summary>
    /// <returns>配置好的服务提供者</returns>
    private static ServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();

        // 配置日志
        services.AddLogging(builder =>
        {
            var logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u1}] {Message:lj}{NewLine}{Exception}", levelSwitch: s_consoleLoggingLevelSwitch)
                .WriteTo.File(Path.Combine(LocalizerEnvironment.LogRoot, "log.log"), rollingInterval: RollingInterval.Day, retainedFileTimeLimit: TimeSpan.FromDays(3))
                .CreateLogger();

            builder.AddSerilog(logger);
        });

        // 添加 IntelliSenseLocalizer 服务
        services.AddIntelliSenseLocalizer();

#if DEBUG
        return services.BuildServiceProvider(true);
#else
        return services.BuildServiceProvider();
#endif
    }

    /// <summary>
    /// 构建字符串过滤函数
    /// </summary>
    /// <param name="filterString">过滤字符串（支持正则表达式）</param>
    /// <returns>过滤函数</returns>
    private static Func<string, bool> BuildStringFilterFunc(string filterString)
    {
        return string.IsNullOrEmpty(filterString)
                    ? _ => true
                    : value => Regex.IsMatch(value, filterString, RegexOptions.IgnoreCase);
    }

    /// <summary>
    /// 设置日志级别
    /// </summary>
    /// <param name="logLevel">日志级别（0-5）</param>
    private static void SetLogLevel(int? logLevel)
    {
        if (logLevel is null)
        {
            s_consoleLoggingLevelSwitch.MinimumLevel = LogEventLevel.Information;
            return;
        }
        var level = logLevel >= (int)LogEventLevel.Verbose
                    && logLevel <= (int)LogEventLevel.Fatal
                        ? (LogEventLevel)logLevel
                        : LogEventLevel.Verbose;
        s_consoleLoggingLevelSwitch.MinimumLevel = level;
    }

    /// <summary>
    /// 输出消息并退出程序
    /// </summary>
    /// <param name="message">要输出的消息</param>
    [DoesNotReturn]
    private static void WriteMessageAndExit(string message)
    {
        Console.WriteLine(message);
        DelayExitProcess(10, 1);
    }

    #endregion Base

    #region new version check

    /// <summary>
    /// 新版本缓存文件路径
    /// </summary>
    private static readonly string s_newVersionCacheFilePath = Path.Combine(LocalizerEnvironment.CacheRoot, "new_version");

    /// <summary>
    /// 尝试获取当前版本号
    /// </summary>
    /// <param name="version">输出参数，当前版本号</param>
    /// <returns>如果成功获取返回 true，否则返回 false</returns>
    private static bool TryGetCurrentVersion([NotNullWhen(true)] out Version? version)
    {
        var currentVersionString = Assembly.GetEntryAssembly()?.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version;

        if (currentVersionString is null
            || !Version.TryParse(currentVersionString.Split('-')[0], out version))
        {
            version = null;
            return false;
        }
        return true;
    }

    /// <summary>
    /// 尝试检查是否有新版本
    /// </summary>
    /// <param name="newVersion">输出参数，新版本号</param>
    /// <returns>如果发现新版本返回 true，否则返回 false</returns>
    private static bool TryCheckNewVersion([NotNullWhen(true)] out Version? newVersion)
    {
        try
        {
            if (TryGetCurrentVersion(out var currentVersion)
                && File.Exists(s_newVersionCacheFilePath)
                && Version.TryParse(File.ReadAllText(s_newVersionCacheFilePath), out var onlineVersion)
                && onlineVersion > currentVersion)
            {
                newVersion = onlineVersion;
                return true;
            }
        }
        catch (Exception ex)
        {
            s_logger.LogDebug(ex, "check new version fail.");
        }
        newVersion = null;
        return false;
    }

    /// <summary>
    /// 尝试从在线获取新版本信息
    /// </summary>
    private static async Task TryGetNewVersionOnlineAsync()
    {
        try
        {
            if (!TryGetCurrentVersion(out var currentVersion))
            {
                return;
            }

            // 获取 NuGet 索引
            var nugetIndex = await "https://api.nuget.org/v3/index.json".CreateHttpRequest()
                                                                    .AutoRedirection(true)
                                                                    .GetAsDynamicJsonAsync();

            IEnumerable<dynamic> resources = nugetIndex!.resources;

            var searchQueryServiceInfo = resources.FirstOrDefault(m => string.Equals("SearchQueryService", m["@type"] as string));

            if (searchQueryServiceInfo is null)
            {
                return;
            }

            var searchQueryBaseUrl = searchQueryServiceInfo["@id"] as string;

            var searchQueryUrl = $"{searchQueryBaseUrl}?q=islocalizer&skip=0&take=10&prerelease=false&semVerLevel=2.0.0";

            var searchQueryResult = await searchQueryUrl.CreateHttpRequest()
                                                    .AutoRedirection(true)
                                                    .GetAsDynamicJsonAsync();

            if (searchQueryResult is null)
            {
                return;
            }

            IEnumerable<dynamic> searchQueryResultItems = searchQueryResult.data;

            var targetPacakgeInfo = searchQueryResultItems.FirstOrDefault(m => string.Equals("islocalizer", m.id as string));

            if (targetPacakgeInfo is null)
            {
                return;
            }

            IEnumerable<dynamic> versions = targetPacakgeInfo.versions;

            // 查找比当前版本新的版本
            var newVersion = versions.Reverse().FirstOrDefault(m => Version.TryParse(m.version as string, out var version) && version > currentVersion);

            if (newVersion is null)
            {
                return;
            }

            // 缓存新版本信息
            await File.WriteAllTextAsync(s_newVersionCacheFilePath, newVersion.version as string);
        }
        catch (Exception ex)
        {
            s_logger.LogDebug(ex, "get new version online fail.");
        }
    }

    #endregion new version check
}
