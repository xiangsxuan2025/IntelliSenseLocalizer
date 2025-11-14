using System.Buffers;
using System.IO.Hashing;
using System.Text;
using System.Xml;
using Microsoft.Extensions.Logging;

namespace IntelliSenseLocalizer.ThirdParty;

/// <summary>
/// 本地化 IntelliSense 翻译器
/// 负责将 XML 文档内容翻译为目标语言
/// </summary>
public class LocalizeIntelliSenseTranslator
{
    #region Private 字段

    /// <summary>
    /// Base62 编码器，用于生成内容版本标识
    /// </summary>
    private static readonly IBaseAnyEncoder<char> s_base62Encoder = BaseAnyEncoding.CreateEncoder("0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz".AsSpan());

    /// <summary>
    /// 日志记录器
    /// </summary>
    private readonly ILogger _logger;

    #endregion Private 字段

    #region Public 构造函数

    /// <summary>
    /// 初始化本地化 IntelliSense 翻译器
    /// </summary>
    /// <param name="logger">日志记录器</param>
    public LocalizeIntelliSenseTranslator(ILogger<LocalizeIntelliSenseTranslator> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    #endregion Public 构造函数

    #region Public 方法

    /// <summary>
    /// 异步翻译 XML 文档
    /// </summary>
    /// <param name="context">翻译上下文</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>表示异步翻译操作的任务</returns>
    public virtual async Task TranslateAsync(TranslateContext context, CancellationToken cancellationToken)
    {
        var sourceXmlDocument = new XmlDocument();
        sourceXmlDocument.Load(context.FilePath);

        XmlDocument outputXmlDocument;

        if (context.IsPatch)
        {
            outputXmlDocument = await PatchTranslateAsync(context, sourceXmlDocument, cancellationToken);
        }
        else
        {
            outputXmlDocument = await TranslateAsync(context, sourceXmlDocument, cancellationToken);
        }

        // 排序内容
        var membersNode = GetMembersNode(outputXmlDocument);
        OrderChildNodesByNameAttribute(membersNode);

        var outDir = Path.GetDirectoryName(context.OutputPath);
        DirectoryUtil.CheckDirectory(outDir);

        _logger.LogDebug("[{File}] processing completed. Save the file into {OutputPath}.", context.FilePath, context.OutputPath);

        outputXmlDocument.Save(context.OutputPath);
    }

    #endregion Public 方法

    #region Protected 方法

    /// <summary>
    /// 补丁模式翻译 - 只翻译新增或修改的内容
    /// </summary>
    /// <param name="context">翻译上下文</param>
    /// <param name="sourceXmlDocument">源 XML 文档</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>翻译后的 XML 文档</returns>
    protected virtual async Task<XmlDocument> PatchTranslateAsync(TranslateContext context, XmlDocument sourceXmlDocument, CancellationToken cancellationToken)
    {
        var patchXmlDocument = new XmlDocument();
        patchXmlDocument.Load(context.OutputPath);

        var outputXmlDocument = (XmlDocument)patchXmlDocument.Clone();

        var contentTranslator = context.ContentTranslator;

        var parallelOptions = new ParallelOptions()
        {
            MaxDegreeOfParallelism = context.ParallelCount > 0 ? context.ParallelCount : 1,
            CancellationToken = cancellationToken,
        };

        var shouldTranslateNodes = new List<XmlNode>();

        var membersNode = GetMembersNode(outputXmlDocument);

        var sourceMembers = SelectMembersMap(sourceXmlDocument);
        var outputMembers = SelectMembersMap(outputXmlDocument);

        foreach (var (name, outputNode) in outputMembers)
        {
            // 移除已不存在的节点
            if (!sourceMembers.TryGetValue(name, out var sourceNode))
            {
                outputNode.ParentNode!.RemoveChild(outputNode);
            }
            else    // 处理还存在节点的子节点
            {
                var sourceChildNodesMap = GetProcessableChildNodesMap(sourceNode);
                var outputChildNodesMap = GetProcessableChildNodesMap(outputNode);

                foreach (var (outputChildName, outputChild) in outputChildNodesMap)
                {
                    // 移除已不存在的子节点
                    if (!sourceChildNodesMap.TryGetValue(outputChildName, out var sourceChild)
                        || sourceChild.Name != outputChild.Name)
                    {
                        outputChild.ParentNode!.RemoveChild(outputChild);
                    }
                    else
                    {
                        var isIgnore = outputChild.Attributes?.GetNamedItem("i")?.Value == "1";

                        if (isIgnore)
                        {
                            continue;
                        }

                        var existedVersion = outputChild.Attributes?.GetNamedItem("v")?.Value;
                        var version = GetContentVersion(sourceChild.InnerXml);
                        if (string.Equals(version, existedVersion))
                        {
                            continue;
                        }

                        outputChild.InnerXml = sourceChild.InnerXml;

                        shouldTranslateNodes.Add(outputChild);
                    }
                }

                // 添加子节点
                foreach (var (sourceChildName, sourceChild) in sourceChildNodesMap)
                {
                    if (!outputChildNodesMap.TryGetValue(sourceChildName, out var outputChild))
                    {
                        var importedNode = outputXmlDocument.ImportNode(sourceChild, true);
                        outputNode.AppendChild(importedNode);

                        shouldTranslateNodes.Add(importedNode);
                    }
                }

                OrderChildNodesByNameAttribute(outputNode);
            }
        }

        foreach (var (name, node) in sourceMembers)
        {
            // 添加缺少节点
            if (!outputMembers.TryGetValue(name, out var outputNode))
            {
                var importedNode = outputXmlDocument.ImportNode(node, true);
                membersNode.AppendChild(importedNode);

                shouldTranslateNodes.AddRange(SelectProcessableChildNodes(importedNode));
            }
        }

        await TranslateNodesAsync(context, shouldTranslateNodes, cancellationToken);

        return outputXmlDocument;
    }

    /// <summary>
    /// 完整模式翻译 - 翻译所有内容
    /// </summary>
    /// <param name="context">翻译上下文</param>
    /// <param name="sourceXmlDocument">源 XML 文档</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>翻译后的 XML 文档</returns>
    protected virtual async Task<XmlDocument> TranslateAsync(TranslateContext context, XmlDocument sourceXmlDocument, CancellationToken cancellationToken)
    {
        var outputXmlDocument = (XmlDocument)sourceXmlDocument.Clone();

        var nodes = SelectNodes(outputXmlDocument, "//summary", "//typeparam", "//param", "//returns");

        await TranslateNodesAsync(context, nodes, cancellationToken);

        // 排序
        foreach (var item in SelectMembersMap(outputXmlDocument).SelectMany(m => SelectProcessableChildNodes(m.Value)))
        {
            OrderChildNodesByNameAttribute(item);
        }

        return outputXmlDocument;
    }

    #endregion Protected 方法

    #region Private 方法

    /// <summary>
    /// 获取内容的版本标识（基于 CRC32 哈希的 Base62 编码）
    /// </summary>
    /// <param name="content">内容字符串</param>
    /// <returns>版本标识字符串</returns>
    private static string GetContentVersion(string content)
    {
        return s_base62Encoder.EncodeToString(Crc32.Hash(Encoding.UTF8.GetBytes(content.Trim())));
    }

    /// <summary>
    /// 获取成员节点
    /// </summary>
    /// <param name="outputXmlDocument">输出 XML 文档</param>
    /// <returns>成员节点</returns>
    private static XmlNode GetMembersNode(XmlDocument outputXmlDocument)
    {
        return outputXmlDocument.SelectSingleNode("/doc/members")!;
    }

    /// <summary>
    /// 按指定键对子节点进行排序
    /// </summary>
    /// <typeparam name="TOrderKey">排序键类型</typeparam>
    /// <param name="node">要排序的节点</param>
    /// <param name="keySelector">键选择器</param>
    private static void OrderChildNodes<TOrderKey>(XmlNode node, Func<XmlNode, TOrderKey> keySelector)
    {
        List<XmlAttribute>? attributes = null;
        if (node.Attributes is not null)
        {
            attributes = [];
            foreach (XmlAttribute item in node.Attributes)
            {
                attributes.Add(item);
            }
        }

        var childNodes = node.ChildNodes.ToList();
        node.RemoveAll();

        foreach (var childNode in childNodes.OrderBy(keySelector))
        {
            node.AppendChild(childNode);
        }

        if (attributes is not null)
        {
            foreach (XmlAttribute item in attributes)
            {
                node.Attributes!.Append(item);
            }
        }
    }

    /// <summary>
    /// 按名称属性对子节点进行排序
    /// </summary>
    /// <param name="xmlNode">要排序的 XML 节点</param>
    private static void OrderChildNodesByNameAttribute(XmlNode xmlNode)
    {
        OrderChildNodes(xmlNode, m => m.Attributes?.GetNamedItem("name")?.Value ?? string.Empty);
    }

    /// <summary>
    /// 获取可处理子节点的字典
    /// </summary>
    /// <param name="xmlNode">XML 节点</param>
    /// <returns>子节点字典</returns>
    private Dictionary<string, XmlNode> GetProcessableChildNodesMap(XmlNode xmlNode)
    {
        return SelectProcessableChildNodes(xmlNode).ToDictionary(m => $"{m.Name}:{m.Attributes!["name"]?.Value}", m => m, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// 选择成员映射
    /// </summary>
    /// <param name="xmlDocument">XML 文档</param>
    /// <returns>成员节点字典</returns>
    private Dictionary<string, XmlNode> SelectMembersMap(XmlDocument xmlDocument)
    {
        var members = SelectNodes(xmlDocument, "/doc/members/member");

        return members.ToDictionary(m => $"{m.Name}:{m.Attributes!["name"]?.Value}", m => m, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// 选择多个 XPath 的节点
    /// </summary>
    /// <param name="xmlDocument">XML 文档</param>
    /// <param name="xpaths">XPath 表达式数组</param>
    /// <returns>节点列表</returns>
    private List<XmlNode> SelectNodes(XmlDocument xmlDocument, params string[] xpaths)
    {
        List<XmlNode> result = [];
        foreach (var xpath in xpaths)
        {
            if (xmlDocument.SelectNodes(xpath) is { } nodeList)
            {
                result.AddRange(nodeList.ToList());
            }
        }
        return result;
    }

    /// <summary>
    /// 选择可处理的子节点
    /// </summary>
    /// <param name="xmlNode">XML 节点</param>
    /// <returns>可处理子节点列表</returns>
    private List<XmlNode> SelectProcessableChildNodes(XmlNode xmlNode)
    {
        if (xmlNode is not XmlElement xmlElement)
        {
            return [];
        }
        List<XmlNode> result = [];

        Append(xmlElement.GetSummaryNodes());
        Append(xmlElement.GetTypeParamNodes());
        Append(xmlElement.GetParamNodes());
        Append(xmlElement.GetReturnsNodes());

        return result;

        void Append(XmlNodeList? xmlNodeList)
        {
            if (xmlNodeList is not null)
            {
                result.AddRange(xmlNodeList.ToList());
            }
        }
    }

    /// <summary>
    /// 异步翻译节点列表
    /// </summary>
    /// <param name="context">翻译上下文</param>
    /// <param name="nodes">要翻译的节点列表</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>表示异步翻译操作的任务</returns>
    private async Task TranslateNodesAsync(TranslateContext context, List<XmlNode> nodes, CancellationToken cancellationToken)
    {
        var contentTranslator = context.ContentTranslator;

        var parallelOptions = new ParallelOptions()
        {
            MaxDegreeOfParallelism = context.ParallelCount > 0 ? context.ParallelCount : 1,
            CancellationToken = cancellationToken,
        };

        await Parallel.ForEachAsync(nodes, parallelOptions, async (node, token) =>
        {
            var rawInnerXml = node.InnerXml;
            try
            {
                var translated = await contentTranslator.TranslateAsync(rawInnerXml, context.SourceCultureInfo, context.TargetCultureInfo, token);
                node.InnerXml = translated;
                var versionAttribute = node.OwnerDocument!.CreateAttribute("v");
                var verison = GetContentVersion(rawInnerXml);
                versionAttribute.Value = verison;
                node.Attributes!.SetNamedItem(versionAttribute);

                var ignoreAttribute = node.OwnerDocument!.CreateAttribute("i");
                ignoreAttribute.Value = "0";
                node.Attributes!.SetNamedItem(ignoreAttribute);
                _logger.LogInformation("Translate IntelliSenseFile Content [{Verison}] \"{Content}\" success \"{Translated}\".", verison, rawInnerXml, translated);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Translate IntelliSenseFile Content \"{Content}\" fail.", rawInnerXml);
            }
        });
    }

    #endregion Private 方法
}
