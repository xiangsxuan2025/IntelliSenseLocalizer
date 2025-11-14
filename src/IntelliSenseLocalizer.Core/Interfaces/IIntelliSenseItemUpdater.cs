using IntelliSenseLocalizer.Models;

namespace IntelliSenseLocalizer;

/// <summary>
/// IntelliSense 项更新器接口
/// 定义更新 IntelliSense 项的标准方法
/// </summary>
public interface IIntelliSenseItemUpdater : IDisposable
{
    /// <summary>
    /// 异步更新 IntelliSense 项组中的所有项
    /// </summary>
    /// <param name="intelliSenseItemGroup">要更新的 IntelliSense 项组</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>表示异步更新操作的任务</returns>
    Task UpdateAsync(IGrouping<string, IntelliSenseItemDescriptor> intelliSenseItemGroup, CancellationToken cancellationToken);
}
