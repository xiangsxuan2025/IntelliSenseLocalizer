namespace IntelliSenseLocalizer;

/// <summary>
/// IntelliSense 项更新器工厂接口
/// 定义创建 IntelliSense 项更新器的标准方法
/// </summary>
public interface IIntelliSenseItemUpdaterFactory
{
    /// <summary>
    /// 获取适用于指定生成上下文的更新器
    /// </summary>
    /// <param name="generateContext">生成上下文</param>
    /// <returns>IntelliSense 项更新器实例</returns>
    IIntelliSenseItemUpdater GetUpdater(GenerateContext generateContext);
}
