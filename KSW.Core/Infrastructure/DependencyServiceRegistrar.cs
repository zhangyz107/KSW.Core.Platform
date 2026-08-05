using Castle.DynamicProxy;
using Example;
using KSW.Dependency;
using KSW.Exceptions;
using KSW.Interception;
using KSW.Reflections;
using Microsoft.Extensions.DependencyInjection;
using Prism.Ioc;

namespace KSW.Infrastructure;

/// <summary>
/// 依赖服务注册器 - 用于扫描注册ISingletonDependency,IScopeDependency,ITransientDependency
/// </summary>
public class DependencyServiceRegistrar : IServiceRegistrar
{
    /// <summary>
    /// 获取服务名
    /// </summary>
    public static string ServiceName => "KSW.Infrastructure.DependencyServiceRegistrar";

    /// <summary>
    /// 排序号
    /// </summary>
    public int OrderId => 100;

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool Enabled => ServiceRegistrarConfig.IsEnabled(ServiceName);

    /// <summary>
    /// 注册服务
    /// </summary>
    /// <param name="serviceContext">服务上下文</param>
    public Action Register(ServiceContext serviceContext)
    {
        // 配置代理生成器
        var containerRegistry = serviceContext.ContainerRegistry;
        containerRegistry.RegisterSingleton<LoggingInterceptor>();
        containerRegistry.RegisterSingleton<ProxyGenerator>();
        return () =>
        {
            RegisterDependency<ISingletonDependency>(serviceContext.ContainerRegistry, serviceContext.TypeFinder, ServiceLifetime.Singleton);
            RegisterDependency<IScopeDependency>(serviceContext.ContainerRegistry, serviceContext.TypeFinder, ServiceLifetime.Scoped);
            RegisterDependency<ITransientDependency>(serviceContext.ContainerRegistry, serviceContext.TypeFinder, ServiceLifetime.Transient);
        };
    }

    /// <summary>
    /// 注册依赖
    /// </summary>
    private void RegisterDependency<TDependencyInterface>(IContainerRegistry containerRegistry, ITypeFinder finder, ServiceLifetime lifetime)
    {
        var types = GetTypes<TDependencyInterface>(finder);
        var result = FilterTypes(types);

        foreach (var item in result)
            RegisterType(containerRegistry, item.Item1, item.Item2, lifetime);
    }

    /// <summary>
    /// 获取接口类型和实现类型列表
    /// </summary>
    private List<(Type, Type)> GetTypes<TDependencyInterface>(ITypeFinder finder)
    {
        var result = new List<(Type, Type)>();
        var classTypes = finder.Find<TDependencyInterface>();
        foreach (var classType in classTypes)
        {
            var interfaceTypes = Helpers.Reflection.GetInterfaceTypes(classType, typeof(TDependencyInterface));
            interfaceTypes.ForEach(interfaceType => result.Add((interfaceType, classType)));
        }
        return result;
    }

    /// <summary>
    /// 过滤类型
    /// </summary>
    private List<(Type, Type)> FilterTypes(List<(Type, Type)> types)
    {
        var result = new List<(Type, Type)>();
        foreach (var group in types.GroupBy(t => t.Item1))
        {
            if (group.Count() == 1)
            {
                result.Add(group.First());
                continue;
            }
            else
            {
                var isMultiple = GetMultiple(group.Key);
                if (isMultiple)
                {
                    var list = group.Select(x => (x.Item1, x.Item2));
                    result.AddRange(list);
                }
                else
                    result.Add(GetTypesByPriority(group));
            }

        }
        return result;
    }

    /// <summary>
    /// 获取优先级类型
    /// </summary>
    private (Type, Type) GetTypesByPriority(IGrouping<Type, (Type, Type)> group)
    {
        int? currentPriority = null;
        Type classType = null;
        foreach (var item in group)
        {
            var priority = GetPriority(item.Item2);
            if (currentPriority == null || priority > currentPriority)
            {
                currentPriority = priority;
                classType = item.Item2;
            }
        }
        return (group.Key, classType);
    }

    /// <summary>
    /// 获取优先级
    /// </summary>
    private int GetPriority(Type type)
    {
        var attribute = type.GetCustomAttribute<IocAttribute>();
        if (attribute == null)
            return 0;
        return attribute.Priority;
    }

    /// <summary>
    /// 是否支持多个实现类
    /// </summary>
    /// <param name="group"></param>
    /// <returns></returns>
    private bool GetMultiple(Type type)
    {
        var attribute = type.GetCustomAttribute<InterfaceMultipleAttribute>();
        if (attribute == null)
            return false;
        return attribute.IsMultiple;
    }

    /// <summary>
    /// 注册类型
    /// </summary>
    private void RegisterType(IContainerRegistry containerRegistry, Type interfaceType, Type classType, ServiceLifetime lifetime)
    {
        var className = classType.Name;
        switch (lifetime)
        {
            case ServiceLifetime.Singleton:
                if (!containerRegistry.IsRegistered(classType, className))
                    containerRegistry.Register(classType, className);

                if (!containerRegistry.IsRegistered(interfaceType, className))
                    containerRegistry.RegisterSingleton(interfaceType, c =>
                    {
                        var generator = c.Resolve<ProxyGenerator>();
                        var interceptor = c.Resolve<LoggingInterceptor>();
                        var target = c.Resolve(classType, className);
                        return generator.CreateInterfaceProxyWithTarget(interfaceType, target, interceptor);
                    });
                break;
            case ServiceLifetime.Scoped:
                if (!containerRegistry.IsRegistered(classType, className))
                    containerRegistry.Register(classType, className);

                if (!containerRegistry.IsRegistered(interfaceType, className))
                    containerRegistry.RegisterScoped(interfaceType, c =>
                    {
                        var generator = c.Resolve<ProxyGenerator>();
                        var interceptor = c.Resolve<LoggingInterceptor>();
                        var target = c.Resolve(classType, className);
                        return generator.CreateInterfaceProxyWithTarget(interfaceType, target, interceptor);
                    });
                break;
            case ServiceLifetime.Transient:
                if (!containerRegistry.IsRegistered(classType, className))
                    containerRegistry.Register(classType, className);

                if (!containerRegistry.IsRegistered(interfaceType, className))
                    containerRegistry.Register(interfaceType, c =>
                    {
                        var generator = c.Resolve<ProxyGenerator>();
                        var interceptor = c.Resolve<LoggingInterceptor>();
                        var target = c.Resolve(classType, className);
                        return generator.CreateInterfaceProxyWithTarget(interfaceType, target, interceptor);
                    });
                break;
            default:
                break;
        }
    }
}