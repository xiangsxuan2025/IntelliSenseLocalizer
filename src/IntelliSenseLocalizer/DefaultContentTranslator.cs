using System.Globalization;
using System.Text;
using System.Web;
using IntelliSenseLocalizer.ThirdParty;

namespace IntelliSenseLocalizer;

/// <summary>
/// 默认内容翻译器实现
/// 通过 HTTP 请求与翻译服务器通信
/// </summary>
internal sealed class DefaultContentTranslator : IContentTranslator, IDisposable
{
    #region Private 字段

    /// <summary>
    /// HTTP 客户端实例
    /// </summary>
    private readonly HttpClient _httpClient;

    #endregion Private 字段

    #region Public 构造函数

    /// <summary>
    /// 初始化默认内容翻译器
    /// </summary>
    /// <param name="address">翻译服务器地址</param>
    public DefaultContentTranslator(string address)
    {
        _httpClient = new HttpClient()
        {
            BaseAddress = new Uri(address)
        };
    }

    #endregion Public 构造函数

    #region Public 方法

    /// <summary>
    /// 释放资源
    /// </summary>
    public void Dispose()
    {
        _httpClient.Dispose();
    }

    /// <summary>
    /// 异步翻译内容
    /// </summary>
    /// <param name="content">要翻译的内容</param>
    /// <param name="from">源区域信息</param>
    /// <param name="to">目标区域信息</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>翻译后的内容</returns>
    public async Task<string> TranslateAsync(string content, CultureInfo from, CultureInfo to, CancellationToken cancellationToken = default)
    {
        // 构建翻译请求 URL 并发送 POST 请求
        using var responseMessage = await _httpClient.PostAsync($"/?from={HttpUtility.UrlEncode(from.Name)}&to={HttpUtility.UrlEncode(to.Name)}", new ByteArrayContent(Encoding.UTF8.GetBytes(content)), cancellationToken);
        return await responseMessage.Content.ReadAsStringAsync(cancellationToken);
    }

    #endregion Public 方法
}
