using System.CommandLine;
using System.Globalization;

using IntelliSenseLocalizer.Models;
using IntelliSenseLocalizer.Properties;

namespace IntelliSenseLocalizer;

internal partial class Program
{
    #region Private 方法

    /// <summary>
    /// 构建 clean 命令
    /// </summary>
    /// <returns>配置好的 clean 命令</returns>
    private static Command BuildCleanCommand()
    {
        var cleanCommand = new Command("clean", Resources.StringCMDCleanDescription);

        cleanCommand.SetHandler(Clean);

        return cleanCommand;
    }

    /// <summary>
    /// 清理悬空的引用文件夹
    /// </summary>
    private static void Clean()
    {
        try
        {
            // 获取应该删除的包路径
            var shouldDeletePacks = DotNetEnvironmentUtil.GetAllApplicationPacks()
                                                         .SelectMany(m => m.Versions)
                                                         .Where(m => IsLocaleRefOnly(m))
                                                         .Select(m => m.RootPath)
                                                         .ToArray();
            if (shouldDeletePacks.Length == 0)
            {
                Console.WriteLine("No folder can delete.");
                return;
            }

            // 确认删除
            Console.WriteLine($"Delete this folders? {Environment.NewLine}{Environment.NewLine}{string.Join(Environment.NewLine, shouldDeletePacks)}{Environment.NewLine}{Environment.NewLine}(input y to delete)");
            var ensureInput = Console.ReadLine()?.Trim();
            if (!string.Equals("y", ensureInput, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            // 执行删除
            foreach (var item in shouldDeletePacks)
            {
                Directory.Delete(item, true);
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            RunAsAdminUtil.TryReRunAsAdmin(ex);
            return;
        }
    }

    /// <summary>
    /// 检查版本描述符是否只包含本地化引用
    /// </summary>
    /// <param name="versionDescriptor">版本描述符</param>
    /// <returns>如果只包含本地化引用返回 true，否则返回 false</returns>
    private static bool IsLocaleRefOnly(ApplicationPackVersionDescriptor versionDescriptor)
    {
        if (!NoFiles(versionDescriptor.RootPath))
        {
            return false;
        }

        if (SingleTargetSubDir(versionDescriptor.RootPath, "ref", out var refDirPath)
            && (NoFiles(refDirPath)
                && Directory.GetDirectories(refDirPath) is string[] refDirSubDirs
                && refDirSubDirs.Length == 1
                && refDirSubDirs[0] is string monikerDir)
            && versionDescriptor.Monikers.Any(m => m.Moniker.EqualsOrdinalIgnoreCase(Path.GetFileName(monikerDir)))
            && NoFiles(monikerDir)
            && Directory.GetDirectories(monikerDir).All(IsCultureDir)
            && Directory.GetDirectories(monikerDir).All(XmlFilesOnly))
        {
            return true;
        }
        return false;

        /// <summary>
        /// 检查目录是否没有文件
        /// </summary>
        static bool NoFiles(string dir) => !Directory.EnumerateFiles(dir).Any();

        /// <summary>
        /// 检查目录是否没有子目录
        /// </summary>
        static bool NoDirs(string dir) => !Directory.EnumerateDirectories(dir).Any();

        /// <summary>
        /// 检查目录是否只包含 XML 文件
        /// </summary>
        static bool XmlFilesOnly(string dir) => NoDirs(dir) && Directory.GetFiles(dir).All(m => Path.GetExtension(m).EqualsOrdinalIgnoreCase(".xml"));

        /// <summary>
        /// 检查目录名是否是有效的区域设置
        /// </summary>
        static bool IsCultureDir(string dir)
        {
            try
            {
                CultureInfo.GetCultureInfo(Path.GetFileName(dir));
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 检查根目录是否只有一个目标子目录
        /// </summary>
        static bool SingleTargetSubDir(string root, string targetDirName, out string targetDirFullPath)
        {
            var dirs = Directory.GetDirectories(root);
            if (dirs.Length == 1
                && targetDirName.EqualsOrdinalIgnoreCase(Path.GetFileName(dirs[0])))
            {
                targetDirFullPath = dirs[0];
                return true;
            }

            targetDirFullPath = string.Empty;
            return false;
        }
    }

    #endregion Private 方法
}
