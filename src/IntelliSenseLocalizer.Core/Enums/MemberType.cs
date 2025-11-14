namespace IntelliSenseLocalizer;

/// <summary>
/// 成员类型枚举
/// 定义 XML 文档注释中不同成员类型的标识符
/// 每种类型对应 XML 文档中成员名称的前缀字符
/// </summary>
public enum MemberType
{
    /// <summary>
    /// 方法 (Method) - 对应前缀 "M:"
    /// 表示类的方法成员，包括构造函数、静态方法等
    /// </summary>
    Method,
    /// <summary>
    /// 类型 (Type) - 对应前缀 "T:"
    /// 表示类、结构体、接口、枚举、委托等类型定义
    /// </summary>
    Type,
    /// <summary>
    /// 属性 (Property) - 对应前缀 "P:"
    /// 表示类的属性成员
    /// </summary>
    Property,
    /// <summary>
    /// 事件 (Event) - 对应前缀 "E:"
    /// 表示类的事件成员
    /// </summary>
    Event,
    /// <summary>
    /// 字段 (Field) - 对应前缀 "F:"
    /// 表示类的字段成员
    /// </summary>
    Field,
}
