using System.Xml;

using IntelliSenseLocalizer.Models;

namespace IntelliSenseLocalizer;

/// <summary>
/// 默认 IntelliSense 项提供者
/// 负责从 XML 文档中提取 IntelliSense 项描述符
/// </summary>
public class DefaultIntelliSenseItemProvider : IIntelliSenseItemProvider
{
    /// <summary>
    /// 从 XML 文档中获取所有 IntelliSense 项描述符
    /// </summary>
    /// <param name="xmlDocument">XML 文档对象</param>
    /// <param name="intelliSenseFileDescriptor">IntelliSense 文件描述符</param>
    /// <returns>IntelliSense 项描述符的枚举</returns>
    public IEnumerable<IntelliSenseItemDescriptor> GetItems(XmlDocument xmlDocument, IntelliSenseFileDescriptor intelliSenseFileDescriptor)
    {
        // 获取所有 member 节点
        var memberNodeList = xmlDocument.GetElementsByTagName("member");

        // 为每个 member 节点创建描述符
        foreach (XmlElement member in memberNodeList)
        {
            yield return IntelliSenseItemDescriptor.Create(member, intelliSenseFileDescriptor);
        }
    }
}
