using IntelliSenseLocalizer.Models;

namespace IntelliSenseLocalizer;

/// <summary>
/// IntelliSense 项网页下载器接口
/// 定义从网页下载 IntelliSense 项文档的标准方法
/// </summary>
public interface IIntelliSenseItemWebPageDownloader : IDisposable
{
    /// <summary>
    /// 异步下载 IntelliSense 项的网页文档
    /// </summary>
    /// <param name="memberDescriptor">成员描述符</param>
    /// <param name="ignoreCache">是否忽略缓存</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>包含 HTML 内容和 URL 的元组</returns>
    Task<(string html, string url)> DownloadAsync(IntelliSenseItemDescriptor memberDescriptor, bool ignoreCache, CancellationToken cancellationToken = default);
}
