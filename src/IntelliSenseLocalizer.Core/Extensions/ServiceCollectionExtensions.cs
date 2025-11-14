using IntelliSenseLocalizer;
using IntelliSenseLocalizer.ThirdParty;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// 服务容器扩展方法
/// 提供依赖注入容器的扩展功能，用于注册 IntelliSense 本地化相关服务
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 向服务容器中添加 IntelliSense 本地化服务
    /// 注册所有必要的服务和实现，以便使用 IntelliSense 本地化功能
    /// </summary>
    /// <param name="services">服务容器实例</param>
    /// <returns>配置后的服务容器，支持链式调用</returns>
    public static IServiceCollection AddIntelliSenseLocalizer(this IServiceCollection services)
    {
        // 注册默认的 IntelliSense 项提供者
        services.TryAddTransient<LocalizeIntelliSenseGenerator>();

        // 注册基于微软文档的更新器工厂
        services.TryAddTransient<IIntelliSenseItemProvider, DefaultIntelliSenseItemProvider>();

        // 注册本地化 IntelliSense 生成器
        services.TryAddTransient<IIntelliSenseItemUpdaterFactory, MSDocIntelliSenseItemUpdaterFactory>();

        // 注册本地化 IntelliSense 翻译器
        services.TryAddTransient<LocalizeIntelliSenseTranslator>();

        return services;
    }
}
