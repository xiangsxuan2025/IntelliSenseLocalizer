using System.CommandLine;
using System.Diagnostics;
using System.Globalization;

using IntelliSenseLocalizer.Models;
using IntelliSenseLocalizer.Properties;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace IntelliSenseLocalizer;

internal partial class Program
{
    /// <summary>
    /// 打包项目文件名
    /// </summary>
    private const string PackCsprojFileName = "pack.csproj";

    /// <summary>
    /// NuGet 包名称
    /// </summary>
    private const string NugetPackageName = "IntelliSenseLocalizer.LanguagePack";

    /// <summary>
    /// 打包项目文件内容模板
    /// </summary>
    private const string PackCsprojContent = $@" <Project Sdk=""Microsoft.NET.Sdk"">

	<PropertyGroup>
		<TargetFramework>netstandard2.0</TargetFramework>
		<IncludeBuildOutput>false</IncludeBuildOutput>
	</PropertyGroup>

	<ItemGroup>
		<None Include="".\**\*.xml"" Pack=""True"" PackagePath=""content"" />
	</ItemGroup>

	<ItemGroup>
	  <None Update=""islocalizer.manifest.json"" Pack=""True"" PackagePath=""/"" />
	</ItemGroup>

	<!--Package Info-->
	<PropertyGroup>
		<Description>Localized IntelliSense files pack. 本地化IntelliSense文件包。</Description>

		<Authors>stratos</Authors>
		<PackageLicenseExpression>MIT</PackageLicenseExpression>
		<PackageProjectUrl>https://github.com/stratosblue/intellisenselocalizer</PackageProjectUrl>

		<RepositoryType>git</RepositoryType>
		<RepositoryUrl>$(PackageProjectUrl)</RepositoryUrl>

		<PackageTags>localized-intellisense-files intellisense-files localization-files</PackageTags>
	</PropertyGroup>
</Project>
";

    #region Private 方法

    /// <summary>
    /// 构建 build 命令
    /// </summary>
    /// <returns>配置好的 build 命令</returns>
    private static Command BuildBuildCommand()
    {
        // 定义命令选项
        var packNameOption = new Option<string>(["-p", "--pack"], Resources.StringCMDBuildOptionPackDescription);
        var monikerOption = new Option<string>(["-m", "--moniker"], Resources.StringCMDBuildOptionMonikerDescription);
        var localeOption = new Option<string>(["-l", "--locale"], () => LocalizerEnvironment.CurrentLocale, Resources.StringCMDBuildOptionLocaleDescription);
        var contentCompareTypeOption = new Option<ContentCompareType>(["-cc", "--content-compare"], () => ContentCompareType.OriginFirst, Resources.StringCMDBuildOptionContentCompareDescription);
        var separateLineOption = new Option<string?>(["-sl", "--separate-line"], Resources.StringCMDBuildOptionSeparateLineDescription);
        var outputOption = new Option<string>(["-o", "--output"], () => LocalizerEnvironment.OutputRoot, Resources.StringCMDBuildOptionOutputDescription);
        var parallelCountOption = new Option<int>(["-pc", "--parallel-count"], () => 2, Resources.StringCMDBuildOptionParallelCountDescription);
        var nocacheOption = new Option<bool>(["-nc", "--no-cache"], () => false, Resources.StringCMDBuildOptionNoCacheDescription);

        // 创建 build 命令并添加选项
        var buildCommand = new Command("build", Resources.StringCMDBuildDescription)
        {
            packNameOption,
            monikerOption,
            localeOption,
            contentCompareTypeOption,
            separateLineOption,
            outputOption,
            parallelCountOption,
            nocacheOption,
        };

        // 设置命令处理器
        buildCommand.SetHandler<string, string, string, ContentCompareType, string?, string, bool, int, int?>(BuildLocalizedIntelliSenseFile, packNameOption, monikerOption, localeOption, contentCompareTypeOption, separateLineOption, outputOption, nocacheOption, parallelCountOption, s_logLevelOption);

        return buildCommand;
    }

    /// <summary>
    /// 构建本地化的 IntelliSense 文件
    /// </summary>
    /// <param name="packName">包名称</param>
    /// <param name="moniker">目标框架名称</param>
    /// <param name="locale">区域设置</param>
    /// <param name="contentCompareType">内容比较类型</param>
    /// <param name="separateLine">分隔线内容</param>
    /// <param name="outputRoot">输出根目录</param>
    /// <param name="noCache">是否禁用缓存</param>
    /// <param name="parallelCount">并行处理数量</param>
    /// <param name="logLevel">日志级别</param>
    private static void BuildLocalizedIntelliSenseFile(string packName,
                                                       string moniker,
                                                       string locale,
                                                       ContentCompareType contentCompareType,
                                                       string? separateLine,
                                                       string outputRoot,
                                                       bool noCache,
                                                       int parallelCount,
                                                       int? logLevel)
    {
        // 设置默认内容比较类型
        if (contentCompareType == ContentCompareType.Default)
        {
            contentCompareType = ContentCompareType.OriginFirst;
        }

        // 处理区域设置
        locale = string.IsNullOrWhiteSpace(locale) ? LocalizerEnvironment.CurrentLocale : locale;

        if (string.IsNullOrWhiteSpace(locale))
        {
            s_logger.LogCritical("\"locale\" must be specified.");
            Environment.Exit(1);
            return;
        }

        // 验证区域设置
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

        // 处理包名称，确保以 .Ref 结尾
        if (!string.IsNullOrEmpty(packName)
            && !packName.EndsWith(".Ref", StringComparison.OrdinalIgnoreCase))
        {
            packName = $"{packName}.Ref";
        }

        // 处理输出目录
        outputRoot = string.IsNullOrWhiteSpace(outputRoot) ? LocalizerEnvironment.OutputRoot : outputRoot;
        DirectoryUtil.CheckDirectory(outputRoot);

        // 构建过滤器函数
        var packNameFilterFunc = BuildStringFilterFunc(packName);
        var monikerFilterFunc = BuildStringFilterFunc(moniker);

        // 获取所有应用包描述符并过滤
        var applicationPackDescriptors = DotNetEnvironmentUtil.GetAllApplicationPacks().ToArray();

        var refDescriptors = applicationPackDescriptors.Where(m => packNameFilterFunc(m.Name))
                                                       .SelectMany(m => m.Versions)
                                                       .SelectMany(m => m.Monikers)
                                                       .Where(m => monikerFilterFunc(m.Moniker))
                                                       .GroupBy(m => m.Moniker)
                                                       .SelectMany(GetDistinctApplicationPackRefMonikerDescriptors)
                                                       .SelectMany(m => m.Refs)
                                                       .Where(m => m.Culture is null)
                                                       .ToArray();

        // 检查是否有可本地化的文件
        if (!refDescriptors.Any())
        {
            s_logger.LogCritical("Not found localizeable files.");
            Environment.Exit(1);
        }

        s_logger.LogInformation("Start generate. PackName: {PackName}, Moniker: {Moniker}, Locale: {locale}, ContentCompareType: {ContentCompareType}.",
                                packName,
                                moniker,
                                locale,
                                contentCompareType);

        // 设置日志级别并开始生成
        SetLogLevel(logLevel);
        GenerateAsync().Wait();

        /// <summary>
        /// 异步生成本地化文件
        /// </summary>
        async Task GenerateAsync()
        {
            var generator = s_serviceProvider.GetRequiredService<LocalizeIntelliSenseGenerator>();
            var parallelOptions = new ParallelOptions() { MaxDegreeOfParallelism = parallelCount };

            // 处理文件
            int refCount = 0;
            foreach (var refDescriptor in refDescriptors)
            {
                refCount++;
                var applicationPackRefMoniker = refDescriptor.OwnerMoniker;
                var moniker = applicationPackRefMoniker.Moniker;

                s_logger.LogInformation("Processing pack [{PackName}:{Moniker}]. Progress {packRefCount}/{packRefAll}.",
                                        refDescriptor.OwnerMoniker.OwnerVersion.OwnerPack.Name,
                                        applicationPackRefMoniker.Moniker,
                                        refCount,
                                        refDescriptors.Length);

                int intelliSenseFileCount = 0;
                await Parallel.ForEachAsync(refDescriptor.IntelliSenseFiles, parallelOptions, async (intelliSenseFileDescriptor, cancellationToken) =>
                {
                    var count = Interlocked.Increment(ref intelliSenseFileCount);

                    var applicationPackVersion = applicationPackRefMoniker.OwnerVersion;
                    var applicationPack = applicationPackVersion.OwnerPack;

                    var buildPath = Path.Combine(LocalizerEnvironment.BuildRoot, $"{moniker}@{locale}@{contentCompareType}", applicationPack.Name, intelliSenseFileDescriptor.FileName);

                    s_logger.LogInformation("Progress PackRef[{packRefCount}/{packRefAll}]->File[{fileCount}/{fileAll}]. Processing [{packName}:{version}:{name}] now.",
                                            refCount,
                                            refDescriptors.Length,
                                            count,
                                            refDescriptor.IntelliSenseFiles.Count,
                                            applicationPack.Name,
                                            refDescriptor.OwnerMoniker.OwnerVersion.Version,
                                            intelliSenseFileDescriptor.Name);

                    var context = new GenerateContext(intelliSenseFileDescriptor, contentCompareType, separateLine, buildPath, cultureInfo)
                    {
                        ParallelCount = parallelCount
                    };

                    await generator.GenerateAsync(context, default);
                });
            }

            // 创建压缩文件
            var outputPackNames = refDescriptors.Select(m => $"{m.OwnerMoniker.Moniker}@{locale}@{contentCompareType}").ToHashSet();
            foreach (var outputPackName in outputPackNames)
            {
                s_logger.LogInformation("start create language pack for {outputPackName}.", outputPackName);

                var rootPath = Path.Combine(LocalizerEnvironment.BuildRoot, outputPackName);
                DirectoryUtil.CheckDirectory(rootPath);

                var finalZipFilePath = await PackLanguagePackAsync(rootPath, moniker, locale, contentCompareType, outputRoot);

                s_logger.LogWarning("localization pack is saved at {finalZipFilePath}.", finalZipFilePath);
            }
        }

        /// <summary>
        /// 获取去重的应用包引用描述符
        /// </summary>
        /// <param name="descriptors">描述符集合</param>
        /// <returns>去重后的描述符</returns>
        static IEnumerable<ApplicationPackRefMonikerDescriptor> GetDistinctApplicationPackRefMonikerDescriptors(IEnumerable<ApplicationPackRefMonikerDescriptor> descriptors)
        {
            var dic = new Dictionary<string, ApplicationPackRefMonikerDescriptor>(StringComparer.OrdinalIgnoreCase);
            foreach (var descriptor in descriptors)
            {
                dic[descriptor.OwnerVersion.OwnerPack.Name] = descriptor;
            }
            return dic.Values;
        }
    }

    /// <summary>
    /// 打包语言包
    /// </summary>
    /// <param name="sourceRootPath">源根路径</param>
    /// <param name="moniker">目标框架名称</param>
    /// <param name="locale">区域设置</param>
    /// <param name="contentCompareType">内容比较类型</param>
    /// <param name="outputRoot">输出根目录</param>
    /// <returns>打包后的文件路径</returns>
    private static async Task<string> PackLanguagePackAsync(string sourceRootPath, string moniker, string locale, ContentCompareType contentCompareType, string outputRoot)
    {
        var cultureInfo = CultureInfo.GetCultureInfo(locale);

        // 获取包列表
        var packs = Directory.GetDirectories(sourceRootPath).Select(m => Path.GetFileName(m)).ToList();

        // 创建元数据
        var metadata = new Dictionary<string, string>()
        {
            { "CreateTime", DateTime.UtcNow.ToString("yyyy-mm-dd HH:mm:ss.fff")},
        };

        // 创建语言包清单
        var languagePackManifest = new LanguagePackManifest(LanguagePackManifest.CurrentVersion, moniker, locale, contentCompareType, packs, metadata);

        // 写入清单文件
        await File.WriteAllTextAsync(Path.Combine(sourceRootPath, LanguagePackManifest.ManifestFileName), languagePackManifest.ToJson(), default);

        // 创建打包项目文件
        var packCsprojFullName = Path.Combine(sourceRootPath, PackCsprojFileName);
        await File.WriteAllTextAsync(packCsprojFullName, PackCsprojContent, default);

        // 生成版本号并打包
        var langPackVersion = new LangPackVersion(moniker, DateTime.UtcNow, contentCompareType, cultureInfo);
        var nugetVersion = langPackVersion.Encode();

        using var packProcess = Process.Start("dotnet", $"pack {packCsprojFullName} -o {outputRoot} -c Release --nologo /p:PackageId={NugetPackageName} /p:Version={nugetVersion}");

        await packProcess.WaitForExitAsync();

        if (packProcess.ExitCode != 0)
        {
            WriteMessageAndExit($"create package fail with code \"{packProcess.ExitCode}\"");
        }

        return Path.Combine(outputRoot, $"{NugetPackageName}.{nugetVersion}.nupkg");
    }

    #endregion Private 方法
}
