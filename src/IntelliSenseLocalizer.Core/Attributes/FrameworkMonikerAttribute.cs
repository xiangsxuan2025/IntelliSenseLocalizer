using System.Reflection;

namespace IntelliSenseLocalizer;

/// <summary>
/// 标记.net名称
/// </summary>
[AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = true)]
public sealed class FrameworkMonikerAttribute : Attribute
{
    /// <summary>
    /// 框架标识符
    /// 例如：net6.0, netcoreapp3.1 等
    /// </summary>
    public string Moniker { get; }

    /// <summary>
    /// 初始化框架标识属性
    /// </summary>
    /// <param name="moniker">框架标识符</param>
    /// <exception cref="ArgumentException">当标识符为空或空白时抛出</exception>
    public FrameworkMonikerAttribute(string moniker)
    {
        // 验证输入参数
        if (string.IsNullOrWhiteSpace(moniker))
        {
            throw new ArgumentException($"“{nameof(moniker)}”不能为 null 或空白。", nameof(moniker));
        }
        Moniker = moniker;
    }
}

/// <summary>
/// 框架标识属性扩展方法
/// 提供从类型和枚举值获取框架标识符的功能
/// </summary>
public static class FrameworkMonikerAttributeExtensions
{
    /// <summary>
    /// 从类型获取框架标识符
    /// </summary>
    /// <param name="type">目标类型</param>
    /// <returns>框架标识符，如果未找到返回null</returns>
    public static string? GetFrameworkMoniker(this Type type)
    {
        if (type.GetCustomAttribute<FrameworkMonikerAttribute>() is FrameworkMonikerAttribute frameworkMonikerAttribute)
        {
            return frameworkMonikerAttribute.Moniker;
    }
        return null;
    }

    /// <summary>
    /// 从枚举值获取框架标识符
    /// </summary>
    /// <param name="enumValue">枚举值</param>
    /// <returns>框架标识符，如果未找到返回null</returns>
    public static string? GetFrameworkMoniker(this Enum enumValue)
    {
        if (enumValue.GetCustomAttribute<FrameworkMonikerAttribute>() is FrameworkMonikerAttribute frameworkMonikerAttribute)
        {
            return frameworkMonikerAttribute.Moniker;
        }
        return null;
    }
}
