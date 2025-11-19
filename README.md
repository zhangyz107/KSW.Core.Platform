# KSW.Core.Platform
坤混顺维自建平台

## 框架组成
- KSW.Core  框架核心
- KSW.Data.Core 数据层核心
- KSW.Data.Abstractions 数据层抽象
- KSW.Data.EntityFrameworkCore  数据实体核心
- KSW.Data.EntityFrameworkCore.Sqlite   数据实体用于Sqlit
- KSW.Domain    域核心
- KSW.Migrations    SQL语句生成及更新组件
- KSW.ObjectMapping.AutoMapper  实体与模型自动映射组件
- KSW.UI.WPF    集成部分自定义WPF控件（消息窗、等待窗、Toast、多选下拉框等）
- KSW.Validators    实体创建、更新数据验证中间件
- KSW.Aop.AspectCore    Aop框架核心，项目代理（支持异常捕获、日志等）
- KSW.Application   域业务核心  

## 框架介绍
本框架依托于.NET 主流依赖注入框架Prism,结合Util框架实现；  
基于Entity Framework ORM框架可以支持多种数据库类型（SQL Server、PostgreSQL、MySQL、Sqlite等）；  
支持Entity Framework原生SQL语句生成脚本；  
支持Aop面向切片编程思想，可以支持自定义中间件实现消息拦截、日志记录等功能；


## 使用方法
- 在自建工程的App.xaml中进行如下修改：  
  - `<Application></Application>`替换为`<prism:PrismApplication></prism:PrismApplication>`
  - 加入代码`xmlns:prism="http://prismlibrary.com/"`    
- 在自建工程的App.xaml.cs中进行如下修改：
  - 修改继承类`Application`为`PrismApplication`
  - 重载方法CreateShell，其中ShellView为主界面视图  
    ```
    protected override Window CreateShell()  
    {  
        return Container.Resolve<ShellView>();  
    }
    ```
  - 重载方法`RegisterTypes`，设置加载项
    ```
    protected override void RegisterTypes(IContainerRegistry containerRegistry)
    {
        var container = containerRegistry.GetContainer();

        // 初始化日志配置
        InitLogConfig();

        // 初始化多语言配置
        InitLanguageConfig(containerRegistry);

        // 将 Serilog 注入容器
        containerRegistry.RegisterInstance(Log.Logger);

        containerRegistry.Register<Dispatcher>(() => Current.Dispatcher);

        // 启动引导程序
        var bootstrapper = new Bootstrapper(containerRegistry);
        bootstrapper.Start();

        // 注册视图
        RegisterView(containerRegistry);
    }
    ```  
    其中初始化日志代码  
    ```
    private void InitLogConfig()
    {
        // 日志路径配置
        var logOutputTemplate = ConfigurationManager.AppSettings["OutputTemplate"];

        // 日志配置（可依据自身需求配置，参照Serilog）
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .Enrich.FromLogContext()
            .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day, outputTemplate: logOutputTemplate)
            .CreateLogger();
    }
    ```
    初始化多语言配置，框架支持多语言设置，在程序初始化时注入容器
    ```
    private void InitLanguageConfig(IContainerRegistry containerRegistry)
    {
        var languageManager = LanguageHelper.Manager;
        containerRegistry.RegisterInstance(languageManager);
    }
    ```
    ```
    /// <summary>
    /// 多语言管理
    /// </summary>
    public class LanguageHelper
    {
        /// <summary>
        /// 多语言资源命名空间
        /// </summary>
        public string ResourceName
        {
            get => "KSW.ATE01.Start.Properties.Resources";
        }

        private readonly ILanguageManager _manager;

        private static readonly Lazy<LanguageHelper> _lazy = new Lazy<LanguageHelper>(() => new LanguageHelper());

        public static ILanguageManager Manager { get { return _lazy?.Value?._manager; } }

        public LanguageHelper()
        {
            _manager = LanguageManagerFactory.CreateManager(ResourceName, GetType().Assembly);
        }
    }
    ```
  - 重载方法`CreateContainerExtension`，设置中间件框架  
    ```
    protected override IContainerExtension CreateContainerExtension()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddLibrary();

        var container = new DryIocContainerExtension(new Container(CreateContainerRules()).WithDependencyInjectionAdapter(serviceCollection));
        Ioc.SetServiceProviderAction(() => container);
        return container;
    }
    ```
  - 重载方法`ConfigureModuleCatalog`，配置依赖注入的模块，主要用于数据库或其他独立模块
    ```
    protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
    {
        //添加Sqlite模块
        moduleCatalog.AddModule<SqliteModule>();
    }
    ```
   - 重载方法`OnExit`，程序退出时将日志全部记录到文件中  
     ```
     protected override void OnExit(ExitEventArgs e)
     {
            Log.CloseAndFlush();
            base.OnExit(e);
     }
     ```

## 实体域
- 创建域文件夹（xx-Domain）
- 创建实体域解决方案
  - xxx.xxx.Domain.AAA
  - xxx.xxx.Domain.AAA.Core
- 实体类创建
  - 创建Entities文件夹
  - 创建实体类A.cs（实体方法）、实体类A.Base.cs（实体属性）
  - 实体类A.Base.cs继承聚合根、IDelete、IVersion、IAudited等接口
- 实体仓储创建
  - 创建Repositories文件夹
  - 创建IARepository继承IRepository\<T>其中T表示实体类

