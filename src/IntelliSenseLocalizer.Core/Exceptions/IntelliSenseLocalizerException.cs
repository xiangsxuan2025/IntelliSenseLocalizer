namespace IntelliSenseLocalizer;

/// <summary>
/// IntelliSense 本地化异常基类
/// 作为所有 IntelliSense 本地化相关异常的父类
/// 提供统一的异常处理基础
/// </summary>
[Serializable]
public class IntelliSenseLocalizerException : Exception
{
    protected IntelliSenseLocalizerException(
      System.Runtime.Serialization.SerializationInfo info,
      System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

    public IntelliSenseLocalizerException()
    { }

    public IntelliSenseLocalizerException(string message) : base(message)
    {
    }

    public IntelliSenseLocalizerException(string message, Exception inner) : base(message, inner)
    {
    }
}
