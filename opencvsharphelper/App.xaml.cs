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
            System.Environment.SetEnvironmentVariable("Path", @"D:\OpenCV\Runtimes\MVSRuntimeX64;C:\Program Files (x86)\Common Files\MVS\Runtime\Win64_x64;D:/OpenCV/Runtimes/opencv452/;D:/OpenCV/Runtimes/BaslerRuntimeX64/;C:\Program Files\Basler\pylon 6\Runtime\x64;");

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