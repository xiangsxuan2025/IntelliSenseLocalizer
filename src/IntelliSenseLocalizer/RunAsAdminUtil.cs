using System.Diagnostics;

namespace IntelliSenseLocalizer;

/// <summary>
/// 管理员权限运行工具类
/// 提供以管理员权限重新运行应用程序的功能
/// </summary>
internal static class RunAsAdminUtil
{
    /// <summary>
    /// 尝试以管理员权限重新运行应用程序
    /// </summary>
    /// <param name="exception">触发重新运行的异常</param>
    public static void TryReRunAsAdmin(Exception exception)
    {
        // 检查是否是 Windows 系统且进程路径有效
        if (OperatingSystem.IsWindows()
           && Environment.ProcessPath is string processPath
           && File.Exists(processPath))
        {
            // 尝试以管理员身份运行
            try
            {
                var processStartInfo = new ProcessStartInfo(processPath, $"{Environment.CommandLine} --custom delay-exit-20s")
                {
                    Verb = "runas",  // 请求管理员权限
                    UseShellExecute = true,
                };

                var process = Process.Start(processStartInfo);
                if (process is not null)
                {
                    return;
                }
            }
            catch (Exception innerEx)
            {
                Console.WriteLine(innerEx.Message);
            }
        }
        else
        {
            Console.WriteLine(exception.Message);
        }

        Console.WriteLine("Please run as administrator again.");
        Environment.Exit(1);
    }
}
