using System.Diagnostics;
using System.Reflection;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IntelliSenseLocalizer.App.Models;

namespace IntelliSenseLocalizer.App.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    public MainWindowViewModel()
    {
        // 设置默认状态消息
        StatusMessage = "选择显示模式和 .NET 版本后点击安装";
    }

    [RelayCommand]
    public async Task StartToTransAsync()
    {
        try
        {
            IsProcessing = true;
            StatusMessage = "正在检查工具和环境...";

            using var proc = new Process();
            proc.StartInfo = CreateProcessStartInfo();

            var outputBuilder = new StringBuilder();
            var errorBuilder = new StringBuilder();

            proc.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    outputBuilder.AppendLine(e.Data);
                    Debug.WriteLine($"Output: {e.Data}");

                    // 更新状态信息
                    if (e.Data.Contains("Download") || e.Data.Contains("download"))
                    {
                        StatusMessage = "正在下载本地化包...";
                    }
                    else if (e.Data.Contains("Install") || e.Data.Contains("install"))
                    {
                        StatusMessage = "正在安装本地化文档...";
                    }
                    else if (e.Data.Contains("Created File"))
                    {
                        StatusMessage = "正在创建本地化文件...";
                    }
                }
            };

            proc.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    errorBuilder.AppendLine(e.Data);
                    Debug.WriteLine($"Error: {e.Data}");
                }
            };

            if (!proc.Start())
            {
                StatusMessage = "启动进程失败";
                IsProcessing = false;
                return;
            }

            proc.BeginOutputReadLine();
            proc.BeginErrorReadLine();

            StatusMessage = "正在执行安装过程...";

            await proc.WaitForExitAsync().ConfigureAwait(false);

            if (proc.ExitCode == 0)
            {
                StatusMessage = "安装完成！重启 IDE 后生效";
            }
            else
            {
                StatusMessage = $"安装过程中出现错误 (退出代码: {proc.ExitCode})";
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Exception in StartToTransAsync: {ex}");
            StatusMessage = $"安装失败: {ex.Message}";
        }
        finally
        {
            IsProcessing = false;
            _ = ClearStatusAfterDelay();
        }
    }

    private async Task ClearStatusAfterDelay()
    {
        await Task.Delay(5000);
        StatusMessage = string.Empty;
    }

    private ProcessStartInfo CreateProcessStartInfo()
    {
        var start = new ProcessStartInfo();
        start.FileName = "cmd.exe";
        start.UseShellExecute = false;
        start.CreateNoWindow = true;
        start.Verb = "runas"; // 请求管理员权限
        start.RedirectStandardOutput = true;
        start.RedirectStandardError = true;
        start.StandardOutputEncoding = Encoding.UTF8;
        start.StandardErrorEncoding = Encoding.UTF8;

        var command = $"dotnet tool install -g islocalizer --version 1.2.4 && " +
                     $"islocalizer install auto -m net{DotnetVersion} -l zh-cn -cc {EnumHelpers.GetEnumMemberName(ContentCompareType.Mode)}";

        start.Arguments = $"/C {command}";

        return start;
    }

    [ObservableProperty]
    private KeepOriginalMode _contentCompareType = KeepOriginalMode.KeepEnglishAhead;

    [ObservableProperty]
    private bool _isProcessing = false;

    [ObservableProperty]
    private string? _dotnetVersion = DotNetVersion.Supported.First();

    [ObservableProperty]
    private string _statusMessage = "准备就绪";

    public bool HasStatusMessage => !string.IsNullOrEmpty(StatusMessage);


    public static string Version => s_version.Value;
    private static readonly Lazy<string> s_version = new Lazy<string>(() =>
    {
        var version = Assembly.GetEntryAssembly()?.GetName().Version;
        return $"v{version?.Major}.{version?.Minor}.{version?.Build}";
    });
}
