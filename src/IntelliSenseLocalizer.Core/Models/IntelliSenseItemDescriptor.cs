using System.Xml;

namespace IntelliSenseLocalizer.Models;

/// <summary>
/// IntelliSense 项描述符
/// 记录类，表示 XML 文档中的一个成员项
/// </summary>
/// <param name="IntelliSenseFileDescriptor">所属的 IntelliSense 文件描述符</param>
/// <param name="OriginName">原始名称（不包含成员类型前缀）</param>
/// <param name="UniqueKey">唯一键（用于标识和查找）</param>
/// <param name="MemberType">成员类型</param>
/// <param name="Element">对应的 XML 元素</param>
public record class IntelliSenseItemDescriptor(IntelliSenseFileDescriptor IntelliSenseFileDescriptor, string OriginName, string UniqueKey, MemberType MemberType, XmlElement Element)
{
    /// <summary>
    /// 从 XML 元素创建 IntelliSense 项描述符
    /// </summary>
    /// <param name="Element">XML 元素</param>
    /// <param name="intelliSenseFileDescriptor">IntelliSense 文件描述符</param>
    /// <returns>创建的 IntelliSense 项描述符</returns>
    /// <exception cref="Exception">当元素名称无效时抛出</exception>
    public static IntelliSenseItemDescriptor Create(XmlElement Element, IntelliSenseFileDescriptor intelliSenseFileDescriptor)
    {
        var name = Element.GetAttribute("name");

        if (string.IsNullOrEmpty(name))
        {
            throw new Exception();
        }

        // 根据名称前缀确定成员类型
        MemberType memberType = name[0] switch
        {
            'M' => MemberType.Method,      // 方法
            'T' => MemberType.Type,        // 类型
            'P' => MemberType.Property,    // 属性
            'E' => MemberType.Event,       // 事件
            'F' => MemberType.Field,       // 字段
            _ => throw new Exception(name) // 未知类型
        };

        // 移除成员类型前缀（前两个字符，如 "M:"）
        name = name.Substring(2);

        // 生成唯一键
        var uniqueKey = IntelliSenseNameUtil.NormalizeOriginNameToUniqueKey(name);

        return new(intelliSenseFileDescriptor, name, uniqueKey, memberType, Element);
    }
}
