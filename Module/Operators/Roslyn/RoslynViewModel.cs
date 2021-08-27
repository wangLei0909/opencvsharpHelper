using ModuleCore.Mvvm;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using OpencvsharpModule.Models;
using OpencvsharpModule.Views;
using Prism.Commands;
using Prism.Ioc;
using Prism.Regions;
using System.Windows.Media.Imaging;

namespace OpencvsharpModule.ViewModels
{
 
    public class RoslynViewModel : RegionViewModelBase
    {
        public ImagePool Pool { get; set; }
        private string _ViewName;
        public string ViewName
        {
            get { return _ViewName; }
            set { SetProperty(ref _ViewName, value); }
        }
        public RoslynViewModel(IContainerExtension container, IRegionManager regionManager) : base(regionManager)
        {
            Pool = container.Resolve<ImagePool>();
            Model = container.Resolve<RoslynEditorModel>();
            ViewName = this.GetType().Name;
            regionManager.RegisterViewWithRegion(ViewName, typeof(CommonView));
        }
       public RoslynEditorModel Model { get; }

    }
}