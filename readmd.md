Done:
删除了广告按钮
优化了按钮描述，如最小化，重启后端
启动即会自动最小化

正在做的： 
Done：cron任务自动检查当前是否和服务器保持链接，没链接了自动执行reload（）函数来重启后端。
目前计划：重试时间逐步翻倍。超过五次不行报错。



TODO:

重要： 可能稍微难以解决一点，复杂一点。需要divide and conquer：   
1. TUN模式在一些电脑上只有terminal里面有网络。 web 浏览器没网络。
可能是v2rayn的问题，也可能是core的问题
done: 2. 改一下主文件夹名称， me_optimized_v2rayn

从文件结构来看，这个项目实际上包含了两个不同的 GUI 实现：
WPF (Windows Presentation Foundation) 实现:
在 v2rayN/Views/ 目录下的 .xaml 和 .xaml.cs 文件
如 MainWindow.xaml, AddServer2Window.xaml 等
Avalonia UI 实现:
在 v2rayN.Desktop/Views/ 目录下的 .axaml 和 .axaml.cs 文件
如 MainWindow.axaml, OptionSettingWindow.axaml 等
这看起来像是一个同时维护着 WPF 和 Avalonia 两个版本的项目：
WPF 版本可能是原始的 Windows 专用版本
Avalonia 版本可能是后来添加的跨平台版本
两个版本共用了一些业务逻辑代码，如 ServiceLib 目录下的 ViewModels。这是一种常见的跨平台策略 - 将核心业务逻辑放在共享库中，然后为不同的 UI 框架实现各自的视图层。

