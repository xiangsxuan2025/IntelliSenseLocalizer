using System.Globalization;

namespace IntelliSenseLocalizer.ThirdParty;

/// <summary>
/// 内容翻译器接口
/// 定义内容翻译的标准方法
/// </summary>
public interface IContentTranslator
{
    #region Public 方法

    /// <summary>
    /// 异步翻译内容
    /// </summary>
    /// <param name="content">要翻译的内容</param>
    /// <param name="from">源区域信息</param>
    /// <param name="to">目标区域信息</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>翻译后的内容</returns>
    Task<string> TranslateAsync(string content, CultureInfo from, CultureInfo to, CancellationToken cancellationToken = default);

    #endregion Public 方法
}
