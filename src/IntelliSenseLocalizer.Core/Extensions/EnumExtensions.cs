using System.Reflection;

using IntelliSenseLocalizer;

namespace IntelliSenseLocalizer;

/// <summary>
/// 枚举扩展方法
/// 提供枚举类型的实用扩展功能
/// </summary>
public static class EnumExtensions
{
    /// <summary>
    /// 获取枚举值的自定义特性
    /// </summary>
    /// <typeparam name="T">特性类型</typeparam>
    /// <param name="enumValue">枚举值</param>
    /// <returns>自定义特性实例，如果不存在返回默认值</returns>
    public static T? GetCustomAttribute<T>(this Enum enumValue) where T : Attribute
    {
        var type = enumValue.GetType();
        var enumName = Enum.GetName(type, enumValue);  // 获取对应的枚举名
        if (enumName is null)
        {
            return default;
        }
        var fieldInfo = type.GetField(enumName);
        if (fieldInfo is null)
        {
            return default;
        }
        var attribute = fieldInfo.GetCustomAttribute(typeof(T), false);
        if (attribute is null)
        {
            return default;
        }
        return (T)attribute;
    }
}
