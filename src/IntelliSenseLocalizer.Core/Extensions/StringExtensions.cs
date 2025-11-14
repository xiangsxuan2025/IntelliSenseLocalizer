namespace System;

/// <summary>
/// 字符串扩展方法
/// 提供字符串操作的实用扩展功能
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// 使用序数比较规则检查两个字符串是否相等
    /// </summary>
    /// <param name="left">第一个字符串</param>
    /// <param name="right">第二个字符串</param>
    /// <returns>如果相等返回 true，否则返回 false</returns>
    public static bool EqualsOrdinal(this string? left, string? right) => string.Equals(left, right, StringComparison.Ordinal);

    /// <summary>
    /// 使用忽略大小写的序数比较规则检查两个字符串是否相等
    /// </summary>
    /// <param name="left">第一个字符串</param>
    /// <param name="right">第二个字符串</param>
    /// <returns>如果相等返回 true，否则返回 false</returns>
    public static bool EqualsOrdinalIgnoreCase(this string? left, string? right) => string.Equals(left, right, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// 检查字符串是否不为 null 或空字符串
    /// </summary>
    /// <param name="value">要检查的字符串</param>
    /// <returns>如果不为 null 或空字符串返回 true，否则返回 false</returns>
    public static bool IsNotNullOrEmpty(this string? value) => !string.IsNullOrEmpty(value);

    /// <summary>
    /// 检查字符串是否不为 null、空字符串或空白字符
    /// </summary>
    /// <param name="value">要检查的字符串</param>
    /// <returns>如果不为 null、空字符串或空白字符返回 true，否则返回 false</returns>
    public static bool IsNotNullOrWhiteSpace(this string? value) => !string.IsNullOrWhiteSpace(value);

    /// <summary>
    /// 检查字符串是否为 null 或空字符串
    /// </summary>
    /// <param name="value">要检查的字符串</param>
    /// <returns>如果为 null 或空字符串返回 true，否则返回 false</returns>
    public static bool IsNullOrEmpty(this string? value) => string.IsNullOrEmpty(value);

    /// <summary>
    /// 检查字符串是否为 null、空字符串或空白字符
    /// </summary>
    /// <param name="value">要检查的字符串</param>
    /// <returns>如果为 null、空字符串或空白字符返回 true，否则返回 false</returns>
    public static bool IsNullOrWhiteSpace(this string? value) => string.IsNullOrWhiteSpace(value);
}
