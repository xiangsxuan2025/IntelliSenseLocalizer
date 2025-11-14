namespace System.IO;

/// <summary>
/// 目录工具类
/// 提供目录操作的实用工具方法
/// 主要用于确保目录存在性的检查和处理
/// </summary>
public static class DirectoryUtil
{
    /// <summary>
    /// 检查并确保目录存在
    /// 如果目录不存在，则创建该目录
    /// 如果目录路径为 null 或空字符串，则忽略操作
    /// </summary>
    /// <param name="directory">要检查的目录路径</param>
    public static void CheckDirectory(string? directory)
    {
        // 检查目录路径是否为空
        if (string.IsNullOrEmpty(directory))
        {
            return;
        }

        // 如果目录不存在，则创建目录
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }
}
