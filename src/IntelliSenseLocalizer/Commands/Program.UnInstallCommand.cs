using System.CommandLine;
using System.Globalization;

using IntelliSenseLocalizer.Properties;

using Microsoft.Extensions.Logging;

namespace IntelliSenseLocalizer;

internal partial class Program
{
    /// <summary>
    /// 构建 uninstall 命令
    /// </summary>
    /// <returns>配置好的 uninstall 命令</returns>
    private static Command BuildUnInstallCommand()
    {
        var uninstallCommand = new Command("uninstall", Resources.StringCMDUnInstallDescription);
        Argument<string> monikerArgument = new("moniker", Resources.StringCMDUnInstallArgumentMonikerDescription);
        Argument<string> localeArgument = new("locale", () => LocalizerEnvironment.CurrentLocale, Resources.StringCMDUnInstallArgumentLocaleDescription);
        Option<string> targetOption = new(["-t", "--target"], () => LocalizerEnvironment.DefaultSdkRoot, Resources.StringCMDUnInstallOptionTargetDescription);

        uninstallCommand.Add(monikerArgument);
        uninstallCommand.Add(localeArgument);
        uninstallCommand.Add(targetOption);

        uninstallCommand.SetHandler<string, string, string>(UnInstall, monikerArgument, localeArgument, targetOption);

        return uninstallCommand;
    }

    /// <summary>
    /// 卸载本地化的 IntelliSense 文件
    /// </summary>
    /// <param name="moniker">目标框架名称</param>
    /// <param name="locale">区域设置</param>
    /// <param name="target">目标 SDK 目录</param>
    private static void UnInstall(string moniker, string locale, string target)
    {
        // 验证区域设置
        CultureInfo culture;
        try
        {
            culture = CultureInfo.GetCultureInfo(locale);
        }
        catch
        {
            s_logger.LogCritical("\"{locale}\" is not a effective locale.", locale);
            Environment.Exit(1);
            throw;
        }

        // 验证目标目录
        var packRoot = DotNetEnvironmentUtil.GetSDKPackRoot(target);
        if (!Directory.Exists(packRoot))
        {
            WriteMessageAndExit($"not found any pack at the target SDK folder {target}. please check input.");
        }

        // 获取所有匹配的包
        var allPack = DotNetEnvironmentUtil.GetAllApplicationPacks(packRoot)
                                           .SelectMany(m => m.Versions)
                                           .SelectMany(m => m.Monikers)
                                           .Where(m => m.Moniker.EqualsOrdinalIgnoreCase(moniker))
                                           .SelectMany(m => m.Refs)
                                           .Where(m => m.Culture == culture)
                                           .ToArray();

        var count = 0;
        try
        {
            // 删除文件
            foreach (var item in allPack.SelectMany(m => m.IntelliSenseFiles))
            {
                File.Delete(item.FilePath);
                Console.WriteLine(item.FilePath);
                count++;
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            RunAsAdminUtil.TryReRunAsAdmin(ex);
            return;
        }
        Console.WriteLine($"UnInstall Done. {count} item deleted.");

        // 清理空目录
        try
        {
            foreach (var packRefRoot in allPack.Select(m => m.RootPath).Distinct())
            {
                DeleteEmptyDirectory(packRefRoot, 4);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }

        /// <summary>
        /// 递归删除空目录
        /// </summary>
        /// <param name="path">目录路径</param>
        /// <param name="count">递归深度</param>
        static void DeleteEmptyDirectory(string? path, int count)
        {
            if (count > 0
                && !string.IsNullOrWhiteSpace(path)
                && Directory.GetDirectories(path).Length == 0
                && Directory.GetFiles(path).Length == 0)
            {
                Directory.Delete(path);

                DeleteEmptyDirectory(Path.GetDirectoryName(path), count - 1);
            }
        }
    }
}
