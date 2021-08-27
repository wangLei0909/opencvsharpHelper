using ModuleCore.Mvvm;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using OpencvsharpModule.Models;
using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;
using System;
using System.Linq;

namespace OpencvsharpModule.ViewModels
{
    public class CalibrateViewModel : BindableBase, IDialogAware
    {

        public event Action<IDialogResult> RequestClose;
        public string Title { get { return "标定助手"; } }
        public bool CanCloseDialog()
        {
            return true;
        }

        public void Close()
        {
            RequestClose?.Invoke(new DialogResult(ButtonResult.Yes));
        }

        public void OnDialogClosed()
        {
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
        }

        public CalibrateViewModel(IContainerExtension container, IRegionManager regionManager)
        {
            Common = container.Resolve<CalibrateCommon>();
            FishEye = container.Resolve<FishEyeModel>();
            CalibrateCamera = container.Resolve<CalibrateCameraModel>();
        }

        public CalibrateCommon Common { get; set; }
        public FishEyeModel FishEye { get; set; }
        public CalibrateCameraModel CalibrateCamera { get; set; }

        private string filePath;

        public string FilePath
        {
            get { return filePath; }
            set { SetProperty(ref filePath, value); }
        }

        private DelegateCommand _loadFiles;

        public DelegateCommand LoadFiles =>
             _loadFiles ??= new DelegateCommand(ExecuteLoadFiles);


        private void ExecuteLoadFiles()
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "选择文件夹",
                Filter = "文件夹|*.directory",
                FileName = "选择此文件夹",

                ValidateNames = false,
                CheckFileExists = false,
                CheckPathExists = true,
                Multiselect = true,//允许同时选择多个文件
                InitialDirectory = FilePath//指定启动路径
            };

            if (openFileDialog.ShowDialog() == true)

            {
                var path = openFileDialog.FileName.Replace("选择此文件夹.directory", "");

                if (!System.IO.Directory.Exists(path))
                {
                    System.Windows.MessageBox.Show(path + "文件夹不存在", "选择文件提示");
                    return;
                }
                Common.Images.Clear();
                var files = System.IO.Directory.GetFiles(path); // 获取多个图片
                foreach (var file in files)
                {
                    Mat imageInput = new Mat(file, ImreadModes.AnyColor);
                    if (imageInput is null || imageInput.Width == 0)
                        continue;
                    Common.Images.Add(file, imageInput);
                }
                if (Common.Images.Count > 0)
                {
                    Common.ImgSrc = WriteableBitmapConverter.ToWriteableBitmap(Common.Images.First().Value);
                    Common.ImgDst = Common.ImgSrc;
                }
            }
        }
    }
}