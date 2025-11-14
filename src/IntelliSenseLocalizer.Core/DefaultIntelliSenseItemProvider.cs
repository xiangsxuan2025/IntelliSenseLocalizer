using System.Xml;

using IntelliSenseLocalizer.Models;

namespace IntelliSenseLocalizer;

/// <summary>
/// 默认 IntelliSense 项提供者
/// 负责从 XML 文档中提取 IntelliSense 项描述符
/// 实现 IIntelliSenseItemProvider 接口
/// </summary>
public class DefaultIntelliSenseItemProvider : IIntelliSenseItemProvider
{
    /// <summary>
    /// 从 XML 文档中获取所有 IntelliSense 项描述符
    /// 遍历 XML 文档中的所有 member 节点并创建对应的描述符
    /// </summary>
    /// <param name="xmlDocument">XML 文档对象</param>
    /// <param name="intelliSenseFileDescriptor">IntelliSense 文件描述符</param>
    /// <returns>IntelliSense 项描述符的枚举</returns>
    public IEnumerable<IntelliSenseItemDescriptor> GetItems(XmlDocument xmlDocument, IntelliSenseFileDescriptor intelliSenseFileDescriptor)
    {
        // 获取所有 member 节点，这些节点包含 API 文档信息
        var memberNodeList = xmlDocument.GetElementsByTagName("member");

        // 遍历每个 member 节点并创建描述符
        foreach (XmlElement member in memberNodeList)
        {
            // 为每个 member 节点创建描述符
            // 使用 yield return 实现延迟加载，提高内存效率
            yield return IntelliSenseItemDescriptor.Create(member, intelliSenseFileDescriptor);
        }
    }
}
