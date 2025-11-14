using System.Globalization;

using Cuture.Http;

using IntelliSenseLocalizer.Models;

namespace IntelliSenseLocalizer;

/// <summary>
/// 默认 IntelliSense 项网页下载器
/// 负责从微软文档网站下载成员文档页面
/// </summary>
public sealed class DefaultIntelliSenseItemWebPageDownloader : IIntelliSenseItemWebPageDownloader
{
    /// <summary>
    /// 未找到页面的内容标识
    /// </summary>
    public const string NotFoundPageContent = "404NotFound";

    /// <summary>
    /// 缓存根目录
    /// </summary>
    private readonly string _cacheRoot;

    /// <summary>
    /// 区域设置
    /// </summary>
    private readonly string _locale;

    /// <summary>
    /// 并行信号量，用于控制并发下载数量
    /// </summary>
    private readonly SemaphoreSlim _parallelSemaphore;

    /// <summary>
    /// 初始化默认 IntelliSense 项网页下载器
    /// </summary>
    /// <param name="cultureInfo">区域信息</param>
    /// <param name="cacheRoot">缓存根目录</param>
    /// <param name="parallelCount">并行下载数量</param>
    public DefaultIntelliSenseItemWebPageDownloader(CultureInfo cultureInfo, string cacheRoot, int parallelCount)
    {
        _locale = cultureInfo.Name.ToLowerInvariant();
        _cacheRoot = cacheRoot;
        _parallelSemaphore = new SemaphoreSlim(parallelCount, parallelCount);

        DirectoryUtil.CheckDirectory(cacheRoot);
    }

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        _parallelSemaphore.Dispose();
    }

    /// <summary>
    /// 异步下载成员文档页面
    /// </summary>
    /// <param name="memberDescriptor">成员描述符</param>
    /// <param name="ignoreCache">是否忽略缓存</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>包含 HTML 内容和 URL 的元组</returns>
    /// <exception cref="MSOnlineDocNotFoundException">当文档页面不存在时抛出</exception>
    public async Task<(string html, string url)> DownloadAsync(IntelliSenseItemDescriptor memberDescriptor, bool ignoreCache, CancellationToken cancellationToken = default)
    {
        var queryKey = memberDescriptor.GetMicrosoftDocsQueryKey();
        var intelliSenseFile = memberDescriptor.IntelliSenseFileDescriptor;
        var frameworkMoniker = intelliSenseFile.Moniker;

        // 构建缓存路径
        var cacheDriectory = Path.Combine(_cacheRoot, intelliSenseFile.Name, frameworkMoniker, _locale);
        var cacheFilePath = Path.Combine(cacheDriectory, $"{queryKey}.html");

        DirectoryUtil.CheckDirectory(cacheDriectory);

        // 构建微软文档 URL
        var url = $"https://learn.microsoft.com/{_locale}/dotnet/api/{queryKey}?view={frameworkMoniker}";

        // 检查缓存
        if (!ignoreCache
            && File.Exists(cacheFilePath))
        {
            var existedHtml = await File.ReadAllTextAsync(cacheFilePath, cancellationToken);
            if (existedHtml.EqualsOrdinalIgnoreCase(NotFoundPageContent))
            {
                throw NotFoundException();
            }
            return (existedHtml, url);
        }

        HttpOperationResult<string> response;

        // 创建 HTTP 请求
        using var request = url.CreateHttpRequest(true)
                               .UseUserAgent("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/99.0.4844.74 Safari/537.36 Edg/99.0.1150.52")
                               .UseSystemProxy()
                               .AutoRedirection()
                               .WithCancellation(cancellationToken);

        // 使用信号量控制并发
        await _parallelSemaphore.WaitAsync(cancellationToken);
        try
        {
            response = await request.TryGetAsStringAsync();
            // 如果状态码不成功且不是 404，则重试一次
            if (!response.IsSuccessStatusCode
                && response.ResponseMessage?.StatusCode != System.Net.HttpStatusCode.NotFound)
            {
                response = await request.TryGetAsStringAsync();
            }
        }
        finally
        {
            _parallelSemaphore.Release();
        }

        using var disposable = response;

        // 处理 404 情况
        if (response.ResponseMessage?.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            await File.WriteAllTextAsync(cacheFilePath, NotFoundPageContent, cancellationToken);
            throw NotFoundException();
        }

        // 处理请求异常
        if (response.Exception is not null)
        {
            throw response.Exception;
        }

        response.ResponseMessage!.EnsureSuccessStatusCode();

        var html = response.Data!;

        // 保存到缓存
        await File.WriteAllTextAsync(cacheFilePath, html, cancellationToken);

        return (html, url);

        /// <summary>
        /// 创建未找到文档异常
        /// </summary>
        MSOnlineDocNotFoundException NotFoundException()
        {
            return new MSOnlineDocNotFoundException($"{url}");
        }
    }
}
