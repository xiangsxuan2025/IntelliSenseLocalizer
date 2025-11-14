using IntelliSenseLocalizer.Utils;

namespace IntelliSenseLocalizer.Models;

/// <summary>
/// 应用程序包描述符
/// 描述 C:\Program Files\dotnet\packs\*.App.Ref 目录结构
/// 例如：C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref
/// </summary>
public class ApplicationPackDescriptor : IEquatable<ApplicationPackDescriptor>
{
    private IReadOnlyList<ApplicationPackVersionDescriptor>? _versions;

    /// <summary>
    /// 应用程序包名称（例如：Microsoft.NETCore.App）
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// 此包的根目录路径
    /// </summary>
    public string RootPath { get; }

    /// <summary>
    /// 应用程序包版本描述列表（延迟加载）
    /// </summary>
    public IReadOnlyList<ApplicationPackVersionDescriptor> Versions => _versions ??= new List<ApplicationPackVersionDescriptor>(EnumerateApplicationPackVersions(this));

    /// <summary>
    /// 初始化应用程序包描述符
    /// </summary>
    /// <param name="name">包名称</param>
    /// <param name="rootPath">根目录路径</param>
    public ApplicationPackDescriptor(string name, string rootPath)
    {
        Name = name;
        RootPath = rootPath;
    }

    /// <summary>
    /// 枚举应用程序包的所有版本
    /// </summary>
    /// <param name="applicationPack">应用程序包描述符</param>
    /// <returns>版本描述符的枚举</returns>
    public static IEnumerable<ApplicationPackVersionDescriptor> EnumerateApplicationPackVersions(ApplicationPackDescriptor applicationPack)
    {
        var path = applicationPack.RootPath;
        if (!Directory.Exists(path))
        {
            yield break;
        }

        // 遍历目录中的所有子目录（每个子目录代表一个版本）
        // 例如：C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Ref\*
        foreach (var versionPath in Directory.EnumerateDirectories(path, "*", SearchOption.TopDirectoryOnly))
        {
            // 尝试解析目录名为版本号
            if (VersionUtil.TryParse(Path.GetFileName(versionPath), out var version))
            {
                yield return new ApplicationPackVersionDescriptor(applicationPack, version, versionPath);
            }
        }
    }

    /// <summary>
    /// 返回格式化的字符串表示
    /// </summary>
    /// <returns>格式化的字符串</returns>
    public override string ToString()
    {
        return $"[{Name}] at [{RootPath}]";
    }

    #region Equals

    /// <summary>
    /// 比较两个应用程序包描述符是否相等
    /// </summary>
    /// <param name="other">另一个描述符</param>
    /// <returns>如果相等返回 true，否则返回 false</returns>
    public bool Equals(ApplicationPackDescriptor? other)
    {
        return other is not null
               && string.Equals(Name, other.Name)
               && string.Equals(RootPath, other.RootPath);
    }

    /// <summary>
    /// 比较对象是否相等
    /// </summary>
    /// <param name="obj">要比较的对象</param>
    /// <returns>如果相等返回 true，否则返回 false</returns>
    public override bool Equals(object? obj)
    {
        return Equals(obj as ApplicationPackDescriptor);
    }

    /// <summary>
    /// 获取哈希码
    /// </summary>
    /// <returns>哈希码</returns>
    public override int GetHashCode()
    {
        return HashCode.Combine(Name, RootPath);
    }

    #endregion Equals
}
