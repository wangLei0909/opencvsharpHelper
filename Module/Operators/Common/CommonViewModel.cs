using ModuleCore.Mvvm;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using OpencvsharpModule.Models;
using Prism.Commands;
using Prism.Ioc;
using Prism.Regions;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace OpencvsharpModule.ViewModels
{
    public class CommonViewModel : RegionViewModelBase
    {
        public ImagePool Pool { get; set; }

        public CommonViewModel(IContainerExtension container, IRegionManager regionManager) : base(regionManager)
        {
            Pool = container.Resolve<ImagePool>();
        }

  
    }
}