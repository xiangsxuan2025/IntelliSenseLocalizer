using System.Diagnostics.CodeAnalysis;

namespace IntelliSenseLocalizer.Utils;

/// <summary>
/// 版本工具类
/// 提供版本字符串解析和处理的实用工具方法
/// 支持包含预发布标签的版本字符串解析
/// </summary>
internal static class VersionUtil
{
    /// <summary>
    /// 尝试解析版本字符串
    /// 支持标准版本格式和包含预发布标签的版本格式
    /// </summary>
    /// <param name="versionString">要解析的版本字符串</param>
    /// <param name="version">解析成功的 Version 对象</param>
    /// <returns>如果解析成功返回 true，否则返回 false</returns>
    public static bool TryParse(string versionString, [NotNullWhen(true)] out Version? version)
    {
        // 处理包含预发布标签的版本字符串（如 "1.0.0-beta"）
        var index = versionString.IndexOf('-');
        if (index >= 0)
        {
            versionString = versionString[..index];
        }

        // 尝试使用 Version.TryParse 解析版本
        if (Version.TryParse(versionString, out version))
        {
            return true;
        }

        version = null;
        return false;
    }
}
