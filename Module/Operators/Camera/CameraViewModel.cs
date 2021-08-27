using Prism.Ioc;
using Prism.Mvvm;

using OpencvsharpModule.Models;
using ModuleCore.Mvvm;
using Prism.Regions;
using OpencvsharpModule.Views;
using Prism.Commands;
using Prism.Services.Dialogs;

namespace OpencvsharpModule.ViewModels
{
    public class CameraViewModel : RegionViewModelBase
    {
        public CameraModel Model { get; set; }
        public CameraViewModel(IDialogService dialogService, IContainerExtension container, IRegionManager regionManager):base(regionManager)
        {

            Model = container.Resolve<CameraModel>();
            _dialogService = dialogService;

        }
        private readonly IDialogService _dialogService;
        private DelegateCommand _CalibrateCamera;

        public DelegateCommand CalibrateCamera =>
             _CalibrateCamera ??= new DelegateCommand(ExecuteCalibrate);

        private void ExecuteCalibrate()
        {
            _dialogService.Show("CalibrateView", new DialogParameters($"message={"message:QQ123211521"}"), r =>
            {
                if (r.Result == ButtonResult.Yes)
                {
                }

                if (r.Result == ButtonResult.Retry)
                {
                }
            });
        }
    }
}
