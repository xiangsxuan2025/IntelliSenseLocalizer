namespace System.Collections.Generic;

/// <summary>
/// HashSet 扩展方法
/// 提供 HashSet 集合的实用扩展功能
/// </summary>
public static class HashSetExtensions
{
    /// <summary>
    /// 将集合中的所有元素添加到 HashSet 中
    /// </summary>
    /// <typeparam name="T">元素类型</typeparam>
    /// <param name="hashSet">目标 HashSet</param>
    /// <param name="collection">要添加的集合</param>
    /// <returns>添加元素后的 HashSet（便于链式调用）</returns>
    public static HashSet<T> AddRange<T>(this HashSet<T> hashSet, IEnumerable<T> collection)
    {
        foreach (var item in collection)
        {
            hashSet.Add(item);
        }
        return hashSet;
    }
}
