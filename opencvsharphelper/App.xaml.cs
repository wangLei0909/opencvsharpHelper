using opencvsharphelper.Views;
using Prism.Ioc;
using Prism.Modularity;
using System.Windows;

namespace opencvsharphelper
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override Window CreateShell()
        {         
            //设置程序的运行时环境
            System.Environment.SetEnvironmentVariable("Path", @"D:/OpenCV/Runtimes/opencv452/;D:/OpenCV/Runtimes/zbarX64Runtime/;D:/OpenCV/Runtimes/BaslerRuntimeX64/;D:/OpenCV/Runtimes/MVSRuntimeX64/;");

            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
        }

        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            _ = moduleCatalog.AddModule<ModuleCore.CoreModule>();

            _ = moduleCatalog.AddModule<OpencvsharpModule.ModuleOpenCVSharp>();
        }
    }
}