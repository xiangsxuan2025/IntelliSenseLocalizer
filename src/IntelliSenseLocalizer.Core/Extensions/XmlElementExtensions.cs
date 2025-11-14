using IntelliSenseLocalizer;

namespace System.Xml;

/// <summary>
/// XmlElement 扩展方法
/// 提供 XML 元素操作的实用扩展功能
/// </summary>
internal static class XmlElementExtensions
{
    /// <summary>
    /// 创建 para 节点
    /// </summary>
    /// <param name="element">父元素</param>
    /// <param name="value">节点值（可选）</param>
    /// <returns>创建的 para 节点</returns>
    public static XmlElement CreateParaNode(this XmlElement element, string? value = null)
    {
        var result = element.OwnerDocument.CreateElement("para");
        if (!string.IsNullOrEmpty(value))
        {
            result.Value = value;
        }
        return result;
    }

    /// <summary>
    /// 创建引用字典，包含所有 see、paramref、typeparamref、c 标签的引用
    /// </summary>
    /// <param name="element">要分析的 XML 元素</param>
    /// <returns>引用名称到 XML 节点的字典</returns>
    public static Dictionary<string, XmlNode> CreateRefDictionary(this XmlElement element)
    {
        var result = new Dictionary<string, XmlNode>();

        AppendRefs(element, result, "see");
        AppendRefs(element, result, "paramref");
        AppendRefs(element, result, "typeparamref");
        AppendRefs(element, result, "c");

        return result;

        /// <summary>
        /// 将指定标签名的所有节点添加到引用字典中
        /// </summary>
        static void AppendRefs(XmlElement rootElement, Dictionary<string, XmlNode> result, string tagName)
        {
            if (rootElement.GetElementsByTagName(tagName) is XmlNodeList xmlNodeList)
            {
                for (int i = 0; i < xmlNodeList.Count; i++)
                {
                    var item = (XmlElement)xmlNodeList[i]!;

                    var key = IntelliSenseNameUtil.TrimMemberPrefix(item.GetAttribute("cref"));
                    if (string.IsNullOrWhiteSpace(key))
                    {
                        key = item.GetAttribute("langword");
                    }
                    if (string.IsNullOrWhiteSpace(key))
                    {
                        key = item.GetAttribute("name");
                    }
                    if (string.IsNullOrWhiteSpace(key))
                    {
                        key = item.InnerText.Trim();
                    }
                    key = IntelliSenseNameUtil.NormalizeOriginNameToUniqueKey(key);
                    result.TryAdd(key, item);
                }
            }
        }
    }

    /// <summary>
    /// 获取 param 节点列表
    /// </summary>
    /// <param name="element">父元素</param>
    /// <returns>param 节点列表</returns>
    public static XmlNodeList GetParamNodes(this XmlElement element)
    {
        return element.GetElementsByTagName("param");
    }

    /// <summary>
    /// 获取 returns 节点列表
    /// </summary>
    /// <param name="element">父元素</param>
    /// <returns>returns 节点列表</returns>
    public static XmlNodeList GetReturnsNodes(this XmlElement element)
    {
        return element.GetElementsByTagName("returns");
    }

    /// <summary>
    /// 获取 summary 节点列表
    /// </summary>
    /// <param name="element">父元素</param>
    /// <returns>summary 节点列表</returns>
    public static XmlNodeList GetSummaryNodes(this XmlElement element)
    {
        return element.GetElementsByTagName("summary");
    }

    /// <summary>
    /// 获取 typeparam 节点列表
    /// </summary>
    /// <param name="element">父元素</param>
    /// <returns>typeparam 节点列表</returns>
    public static XmlNodeList GetTypeParamNodes(this XmlElement element)
    {
        return element.GetElementsByTagName("typeparam");
    }

    /// <summary>
    /// 导入并追加子节点
    /// </summary>
    /// <param name="element">目标元素</param>
    /// <param name="node">要导入和追加的节点</param>
    /// <returns>目标元素（便于链式调用）</returns>
    public static XmlElement ImportAppendChild(this XmlElement element, XmlNode? node)
    {
        if (node is null)
        {
            return element;
        }
        var newNode = element.OwnerDocument.ImportNode(node, true);
        element.AppendChild(newNode);
        return element;
    }

    /// <summary>
    /// 将 XmlNodeList 转换为 List
    /// </summary>
    /// <param name="xmlNodeList">XML 节点列表</param>
    /// <returns>包含所有节点的列表</returns>
    public static List<XmlNode> ToList(this XmlNodeList xmlNodeList)
    {
        var result = new List<XmlNode>();
        foreach (XmlNode item in xmlNodeList)
        {
            result.Add(item);
        }
        return result;
    }
}
