using ModuleCore.Mvvm;
using OpencvsharpModule.Models;
using OpencvsharpModule.Views;
using Prism.Ioc;
using Prism.Modularity;

namespace OpencvsharpModule
{
    public class ModuleOpenCVSharp : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
       
            var Navigate = containerProvider.Resolve<NavigateModel>();
            Navigate.NavigateList.Add(new NavigateItem() { ViewName = "CameraView", IconKind = "CameraOutline", DisplayName = "拍照", UserLevel = 0, Display = true });
            Navigate.NavigateList.Add(new NavigateItem() { ViewName = "ThresholdView", IconKind = "CircleHalfFull", DisplayName = "阈值", UserLevel = 0, Display = true });
            Navigate.NavigateList.Add(new NavigateItem() { ViewName = "MorphologyView", IconKind = "YinYang", DisplayName = "形态学与滤波", UserLevel = 0, Display = true });
            Navigate.NavigateList.Add(new NavigateItem() { ViewName = "CannyView", IconKind = "CircleSlice8", DisplayName = "边缘与轮廓", UserLevel = 0, Display = true });
            Navigate.NavigateList.Add(new NavigateItem() { ViewName = "HoughLinesView", IconKind = "CircleOffOutline", DisplayName = "线与圆检测", UserLevel = 0, Display = true });
            Navigate.NavigateList.Add(new NavigateItem() { ViewName = "FeatureMatchingView", IconKind = "Fingerprint", DisplayName = "匹配", UserLevel = 0, Display = true });
            Navigate.NavigateList.Add(new NavigateItem() { ViewName = "ConnectedView", IconKind = "Grain", DisplayName = "连通域", UserLevel = 0, Display = true });
            Navigate.NavigateList.Add(new NavigateItem() { ViewName = "MaskCopyView", IconKind = "ContentCopy", DisplayName = "两图运算", UserLevel = 0, Display = true });
            //Navigate.NavigateList.Add(new NavigateItem() { ViewName = "HogSvmView", IconKind = "BookOpenPageVariantOutline", DisplayName = "机器学习", UserLevel = 0, Display = true });
            Navigate.NavigateList.Add(new NavigateItem() { ViewName = "CornersView", IconKind = "LessThan", DisplayName = "角点检测", UserLevel = 0, Display = true });
            Navigate.NavigateList.Add(new NavigateItem() { ViewName = "RoslynView", IconKind = "ScriptTextPlayOutline", DisplayName = "脚本", UserLevel = 0, Display = true });
            Navigate.DefaultView = "CameraView";
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<CalibrateCommon>();
            containerRegistry.RegisterSingleton<CameraModel>();
            containerRegistry.RegisterSingleton<ImagePool>();
            containerRegistry.RegisterSingleton<DataPool>();
            //containerRegistry.RegisterSingleton<opencvcli.GOCW>();
            containerRegistry.RegisterSingleton<RoslynEditorModel>();

            containerRegistry.RegisterDialog<CalibrateView, ViewModels.CalibrateViewModel>();


            containerRegistry.RegisterForNavigation<CameraView>();
            containerRegistry.RegisterForNavigation<ThresholdView>();
            containerRegistry.RegisterForNavigation<MorphologyView>();
            containerRegistry.RegisterForNavigation<HoughLinesView>();
            containerRegistry.RegisterForNavigation<RoslynView>();
            containerRegistry.RegisterForNavigation<FeatureMatchingView>();
            containerRegistry.RegisterForNavigation<ConnectedView>();
            containerRegistry.RegisterForNavigation<MaskCopyView>();
            containerRegistry.RegisterForNavigation<CannyView>();
            containerRegistry.RegisterForNavigation<HogSvmView>();
            containerRegistry.RegisterForNavigation<CornersView>();
        }
    }
}