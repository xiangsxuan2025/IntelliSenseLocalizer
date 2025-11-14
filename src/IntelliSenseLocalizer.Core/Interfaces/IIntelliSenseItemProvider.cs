using System.Xml;

using IntelliSenseLocalizer.Models;

namespace IntelliSenseLocalizer;

/// <summary>
/// IntelliSense 项提供者接口
/// 定义从 XML 文档提取 IntelliSense 项的标准方法
/// </summary>
public interface IIntelliSenseItemProvider
{
    /// <summary>
    /// 从 XML 文档中获取所有 IntelliSense 项描述符
    /// </summary>
    /// <param name="xmlDocument">XML 文档对象</param>
    /// <param name="intelliSenseFileDescriptor">IntelliSense 文件描述符</param>
    /// <returns>IntelliSense 项描述符的枚举</returns>
    IEnumerable<IntelliSenseItemDescriptor> GetItems(XmlDocument xmlDocument, IntelliSenseFileDescriptor intelliSenseFileDescriptor);
}
