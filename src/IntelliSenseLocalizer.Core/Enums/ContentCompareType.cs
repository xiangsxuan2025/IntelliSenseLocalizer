namespace IntelliSenseLocalizer;

/// <summary>
/// 内容对照类型枚举
/// 定义原始内容和本地化内容的显示顺序和方式
/// </summary>
public enum ContentCompareType
{
    /// <summary>
    /// 默认值，通常等同于 OriginFirst
    /// </summary>
    Default = 0,

    /// <summary>
    /// 原始内容靠前，本地化内容在后
    /// 格式：[原始内容] [分隔线] [本地化内容]
    /// 适用于希望先看到英文原文的用户
    /// </summary>
    OriginFirst = 1,

    /// <summary>
    /// 本地化内容靠前，原始内容在后
    /// 格式：[本地化内容] [分隔线] [原始内容]
    /// 适用于希望先看到翻译内容的用户
    /// </summary>
    LocaleFirst = 2,

    /// <summary>
    /// 无原始内容对照，只显示本地化内容
    /// 格式：[本地化内容]
    /// 适用于希望界面最简洁的用户
    /// </summary>
    None = 3,
}
