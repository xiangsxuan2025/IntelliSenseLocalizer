using System.CommandLine;
using System.Globalization;
using IntelliSenseLocalizer.Properties;
using IntelliSenseLocalizer.ThirdParty;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace IntelliSenseLocalizer;

internal partial class Program
{
    #region Private 方法

    /// <summary>
    /// 构建 translate 命令
    /// </summary>
    /// <returns>配置好的 translate 命令</returns>
    private static Command BuildTranslateCommand()
    {
        var fileOption = new Option<string>(["-f", "--file"], Resources.StringCMDTranslateOptionFileDescription);
        var serverOption = new Option<string>(["-s", "--server"], Resources.StringCMDTranslateOptionServerDescription);
        var fromLocaleOption = new Option<string>(["-fl", "--from-locale"], () => "en-us", Resources.StringCMDTranslateOptionFromLocaleDescription);
        var targetLocalesOption = new Option<string>(["-tl", "--target-locales"], () => LocalizerEnvironment.CurrentLocale, Resources.StringCMDTranslateOptionLocalesDescription);
        var contentCompareTypeOption = new Option<ContentCompareType>(["-cc", "--content-compare"], () => ContentCompareType.OriginFirst, Resources.StringCMDBuildOptionContentCompareDescription);
        var separateLineOption = new Option<string?>(["-sl", "--separate-line"], Resources.StringCMDBuildOptionSeparateLineDescription);
        var outputOption = new Option<string?>(["-o", "--output"], () => null, Resources.StringCMDTranslateOptionOutputDescription);
        var parallelCountOption = new Option<int>(["-pc", "--parallel-count"], () => 2, Resources.StringCMDBuildOptionParallelCountDescription);
        var patchOption = new Option<bool>(["-p", "--patch"], () => false, Resources.StringCMDTranslateOptionPatchDescription);

        var translateCommand = new Command("translate", Resources.StringCMDTranslateDescription)
        {
            fileOption,
            serverOption,
            fromLocaleOption,
            targetLocalesOption,
            contentCompareTypeOption,
            separateLineOption,
            outputOption,
            parallelCountOption,
            patchOption,
        };

        translateCommand.SetHandler<string, string, string, string, ContentCompareType, string?, string?, int, bool, int?>(TranslateLocalizedIntelliSenseFile,
                                                                                                                           fileOption,
                                                                                                                           serverOption,
                                                                                                                           fromLocaleOption,
                                                                                                                           targetLocalesOption,
                                                                                                                           contentCompareTypeOption,
                                                                                                                           separateLineOption,
                                                                                                                           outputOption,
                                                                                                                           parallelCountOption,
                                                                                                                           patchOption,
                                                                                                                           s_logLevelOption);

        return translateCommand;
    }

    /// <summary>
    /// 获取区域信息
    /// </summary>
    /// <param name="locale">区域字符串</param>
    /// <returns>对应的 CultureInfo 对象</returns>
    private static CultureInfo GetCultureInfo(string locale)
    {
        CultureInfo cultureInfo;
        try
        {
            cultureInfo = CultureInfo.GetCultureInfo(locale);
        }
        catch
        {
            s_logger.LogCritical("\"{locale}\" is not a effective locale.", locale);
            Environment.Exit(1);
            throw;
        }

        return cultureInfo;
    }

    /// <summary>
    /// 翻译本地化的 IntelliSense 文件
    /// </summary>
    /// <param name="file">要翻译的 XML 文件路径</param>
    /// <param name="server">翻译服务器地址</param>
    /// <param name="fromLocale">源区域设置</param>
    /// <param name="targetLocalesString">目标区域设置字符串（多个用分号分隔）</param>
    /// <param name="contentCompareType">内容比较类型</param>
    /// <param name="separateLine">分隔线内容</param>
    /// <param name="outputRoot">输出根目录</param>
    /// <param name="parallelCount">并行处理数量</param>
    /// <param name="isPatch">是否为补丁模式</param>
    /// <param name="logLevel">日志级别</param>
    private static void TranslateLocalizedIntelliSenseFile(string file,
                                                           string server,
                                                           string fromLocale,
                                                           string targetLocalesString,
                                                           ContentCompareType contentCompareType,
                                                           string? separateLine,
                                                           string? outputRoot,
                                                           int parallelCount,
                                                           bool isPatch,
                                                           int? logLevel)
    {
        // 设置默认内容比较类型
        if (contentCompareType == ContentCompareType.Default)
        {
            contentCompareType = ContentCompareType.OriginFirst;
        }

        // 验证文件是否存在
        if (!File.Exists(file))
        {
            s_logger.LogCritical("xml file \"{File}\" not found.", file);
            Environment.Exit(1);
            return;
        }

        // 验证服务器地址
        if (string.IsNullOrWhiteSpace(server))
        {
            s_logger.LogCritical("\"server\" must be specified.");
            Environment.Exit(1);
            return;
        }

        // 验证源区域设置
        if (string.IsNullOrWhiteSpace(fromLocale))
        {
            s_logger.LogCritical("\"from-locale\" must be specified.");
            Environment.Exit(1);
            return;
        }

        // 获取源区域信息
        var sourceCultureInfo = GetCultureInfo(fromLocale);

        // 解析目标区域设置
        var targetLocales = string.IsNullOrWhiteSpace(targetLocalesString)
                            ? [LocalizerEnvironment.CurrentLocale]
                            : targetLocalesString.Split(';');

        if (targetLocales.Length == 0)
        {
            s_logger.LogCritical("\"target-locales\" must be specified.");
            Environment.Exit(1);
            return;
        }

        // 获取目标区域信息
        var targetCultureInfos = targetLocales.Select(GetCultureInfo).Distinct().ToArray();

        // 设置输出目录
        if (string.IsNullOrWhiteSpace(outputRoot))
        {
            outputRoot = Path.GetDirectoryName(file);
        }

        DirectoryUtil.CheckDirectory(outputRoot);

        s_logger.LogInformation("Start translate. Xml: {file}, Locale: {locale}, ContentCompareType: {ContentCompareType}.",
                                file,
                                targetLocales,
                                contentCompareType);

        // 设置日志级别
        SetLogLevel(logLevel);

        // 创建内容翻译器
        using var contentTranslator = new DefaultContentTranslator(server);

        TranslateAsync().Wait();

        /// <summary>
        /// 异步执行翻译任务
        /// </summary>
        async Task TranslateAsync()
        {
            var translator = s_serviceProvider.GetRequiredService<LocalizeIntelliSenseTranslator>();

            // 为每个目标区域创建翻译任务
            foreach (var targetCultureInfo in targetCultureInfos)
            {
                var outputDirectory = Path.Combine(outputRoot!, targetCultureInfo.Name);
                DirectoryUtil.CheckDirectory(outputDirectory);

                var outputPath = Path.Combine(outputDirectory, Path.GetFileName(file));

                // 创建翻译上下文
                var context = new TranslateContext(file, contentCompareType, separateLine, outputPath, sourceCultureInfo, targetCultureInfo, contentTranslator, isPatch);

                await translator.TranslateAsync(context, default);
            }
        }
    }

    #endregion Private 方法
}
