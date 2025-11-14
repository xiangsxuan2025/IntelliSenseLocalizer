using System.CommandLine;

using IntelliSenseLocalizer.Properties;

namespace IntelliSenseLocalizer;

internal partial class Program
{
    /// <summary>
    /// 构建 clear 命令
    /// </summary>
    /// <returns>配置好的 clear 命令</returns>
    private static Command BuildClearCommand()
    {
        var clearCommand = new Command("clear", Resources.StringCMDClearDescription);

        Argument<ClearType> argument = new("type");

        clearCommand.Add(argument);

        clearCommand.SetHandler<ClearType>(Clear, argument);

        return clearCommand;
    }

    /// <summary>
    /// 清理指定类型的文件或目录
    /// </summary>
    /// <param name="type">清理类型</param>
    private static void Clear(ClearType type)
    {
        try
        {
            switch (type)
            {
                case ClearType.All:
                    Directory.Delete(LocalizerEnvironment.CacheRoot, true);
                    Directory.Delete(LocalizerEnvironment.LogRoot, true);
                    Directory.Delete(LocalizerEnvironment.OutputRoot, true);
                    Directory.Delete(LocalizerEnvironment.BuildRoot, true);
                    break;

                case ClearType.Cache:
                    Directory.Delete(LocalizerEnvironment.CacheRoot, true);
                    break;

                case ClearType.Output:
                    Directory.Delete(LocalizerEnvironment.OutputRoot, true);
                    break;

                case ClearType.Logs:
                    Directory.Delete(LocalizerEnvironment.LogRoot, true);
                    break;

                case ClearType.Build:
                    Directory.Delete(LocalizerEnvironment.BuildRoot, true);
                    break;
            }
            Console.WriteLine($"[{type}] Cleared.");
        }
        finally
        {
            LocalizerEnvironment.CheckDirectories();
        }
    }

    /// <summary>
    /// 清理类型枚举
    /// </summary>
    private enum ClearType
    {
        /// <summary>
        /// 清理所有内容
        /// </summary>
        All = 1,

        /// <summary>
        /// 只清理缓存
        /// </summary>
        Cache = 1 << 1,

        /// <summary>
        /// 只清理输出
        /// </summary>
        Output = 1 << 2,

        /// <summary>
        /// 只清理日志
        /// </summary>
        Logs = 1 << 3,

        /// <summary>
        /// 只清理构建文件
        /// </summary>
        Build = 1 << 4,
    }
}
