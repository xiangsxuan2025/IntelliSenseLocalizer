namespace HtmlAgilityPack;

/// <summary>
/// HtmlNode 扩展方法
/// 提供 HTML 节点操作的实用扩展功能
/// </summary>
internal static class HtmlNodeExtensions
{
    /// <summary>
    /// 获取指定标签名的下一个兄弟节点
    /// </summary>
    /// <param name="htmlNode">当前 HTML 节点</param>
    /// <param name="tagName">要查找的标签名</param>
    /// <returns>找到的兄弟节点，如果不存在返回 null</returns>
    public static HtmlNode? GetNextTagNode(this HtmlNode htmlNode, string tagName)
    {
        while (htmlNode.NextSibling is HtmlNode nextNode)
        {
            if (nextNode.Name.EqualsOrdinalIgnoreCase(tagName))
            {
                return nextNode;
            }

            htmlNode = nextNode;
        }

        return null;
    }

    /// <summary>
    /// 获取指定标签名的前一个兄弟节点
    /// </summary>
    /// <param name="htmlNode">当前 HTML 节点</param>
    /// <param name="tagName">要查找的标签名</param>
    /// <returns>找到的兄弟节点，如果不存在返回 null</returns>
    public static HtmlNode? GetPreTagNode(this HtmlNode htmlNode, string tagName)
    {
        while (htmlNode.PreviousSibling is HtmlNode preNode)
        {
            if (preNode.Name.EqualsOrdinalIgnoreCase(tagName))
            {
                return preNode;
            }

            htmlNode = preNode;
        }

        return null;
    }
}
