using System.Diagnostics;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IntelliSenseLocalizer.App.Models;

namespace IntelliSenseLocalizer.App.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    public MainWindowViewModel()
    {
    }
    [RelayCommand]
    public async Task StartToTransAsync()
    {
        try
        {
            using var proc = new Process();
            proc.StartInfo = CreateProcessStartInfo(true);
            IsProcessing = true;

            if (!proc.Start())
            {
                IsProcessing = false;
                return;
            }

            if (proc.StartInfo.RedirectStandardOutput)
            {
                proc.BeginOutputReadLine();
                proc.BeginErrorReadLine();
            }

            await proc.WaitForExitAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Exception in StartToTransAsync: {ex}");

            throw;
        }
        finally
        {
            IsProcessing = false;
        }

    }
    private ProcessStartInfo CreateProcessStartInfo(bool redirect)
    {
        var start = new ProcessStartInfo();
        start.FileName = "cmd.exe";
        start.UseShellExecute = false;
        start.CreateNoWindow = true;
        //start.Verb = "runas";
        start.RedirectStandardOutput = true;
        start.RedirectStandardError = true;
        start.StandardOutputEncoding = Encoding.UTF8;
        start.StandardErrorEncoding = Encoding.UTF8;
        start.Arguments = $"/C dotnet tool install -g islocalizer &&"
            + $"islocalizer install auto -m net{DotnetVersion!} -l zh-cn -cc {EnumHelpers.GetEnumMemberName(ContentCompareType!.Mode)}";

        return start;
    }

    [ObservableProperty]
    private KeepOriginalMode _contentCompareType = KeepOriginalMode.KeepEnglishAhead;


    [ObservableProperty]
    private bool _isProcessing = false;
    [ObservableProperty]
    private string? _dotnetVersion = DotNetVersion.Supported.First();
}
