using Microsoft.Win32;
using ModuleCore.Mvvm;
using ModuleCore.Services;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using OpencvsharpModule.Common;
using OpencvsharpModule.Devices;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace OpencvsharpModule.Models
{
    public partial class CameraModel : BindableBase
    {
        public CameraModel(IEventAggregator ea, IContainerExtension container)
        {

            HikrobotCameras = new MVSCameras();
            BaslerCameras = new BaslerCameras();

            _ea = ea;
            HikrobotCameras.ErrorMessage += ShowError;
            HikrobotCameras.CameraListChanged += UpdateCameraList;
            HikrobotCameras.InitCameras();

            BaslerCameras.ErrorMessage += ShowError;
            BaslerCameras.CameraListChanged += UpdateCameraList;
            BaslerCameras.InitCameras();

            Pool = container.Resolve<ImagePool>();
            var vcas = Enum.GetValues<VideoCaptureAPIs>();
            foreach (var vca in vcas)
                VideoCaptureAPIsList.Add(vca.ToString(), vca);
            VideoCaptureAPI = VideoCaptureAPIsList.FirstOrDefault();
            LoadAutoRun();
        }

        private void UpdateCameraList(string cameraInfo)
        {
            var info = cameraInfo.Split(";");
            CameraList.Add(info[0], info[1]); // 厂家 ，IP
        }

        private void ShowError(string obj)
        {
            _ea.GetEvent<MessageEvent>().Publish(new()
            {
                Target = "errLog",
                Content = obj
            });
        }

        public ICameras HikrobotCameras { get; set; }
        public ICameras BaslerCameras { get; set; }
        private readonly IEventAggregator _ea;
        public ImagePool Pool { get; set; }

        private DelegateCommand _addMat;

        public DelegateCommand AddMat =>
                _addMat ??= new DelegateCommand(ExecuteAddMat);

        private int add;

        private void ExecuteAddMat()
        {
            if (!Pool.SelectImage.HasValue) return;
            MatName ??= "Src" + add;
            while (Pool.Images.ContainsKey(MatName))
            {
                MatName = "Src" + add++;
            }
            Pool.Images[MatName] = new Mat();
            Pool.SelectImage.Value.Value.CopyTo(Pool.Images[MatName]);
        }

        private DelegateCommand _AddDstMat;

        public DelegateCommand AddDstMat =>
             _AddDstMat ??= new DelegateCommand(ExecuteAddDstMat);

        private int addDst;

        private void ExecuteAddDstMat()
        {
            if (Dst == null) return;
            DstMatName ??= "Dst" + addDst;
            while (Pool.Images.ContainsKey(DstMatName))
            {
                DstMatName = "Dst" + addDst++;
            }
            Pool.Images[DstMatName] = Dst.Clone();
        }

        private WriteableBitmap _imgDst;

        public WriteableBitmap ImgDst
        {
            get { return _imgDst; }
            set { SetProperty(ref _imgDst, value); }
        }

        private long _CT;

        public long CT
        {
            get { return _CT; }
            set { SetProperty(ref _CT, value); }
        }

        private string _MatName;

        public string MatName
        {
            get { return _MatName; }
            set { SetProperty(ref _MatName, value); }
        }

        private string _DstMatName;

        public string DstMatName
        {
            get { return _DstMatName; }
            set { SetProperty(ref _DstMatName, value); }
        }

        private ObservableDictionary<string, string> _CameraList = new();

        public ObservableDictionary<string, string> CameraList
        {
            get { return _CameraList; }
            set { SetProperty(ref _CameraList, value); }
        }

        private readonly Stopwatch AutoRunsw = new Stopwatch();

        private void UpdateSrc()
        {
            Pool.Images["Src"] = Src;
            Pool.SelectImage = null;
            Pool.SelectImage = Pool.Images.Where(i => i.Key == "Src").FirstOrDefault();
            Task.Run(() =>
            {
                AutoRunsw.Restart();
                AutoRun.Value.Invoke(Src);
                AutoRunsw.Stop();
                CT = AutoRunsw.ElapsedMilliseconds;
                if (Dst is null) return;
                System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
                {
                    ImgDst = WriteableBitmapConverter.ToWriteableBitmap(Dst);
                }));
            });
        }

        private ObservableDictionary<string, Action<Mat>> _AutoRunList = new();

        public Mat Dst;

        public ObservableDictionary<string, Action<Mat>> AutoRunList
        {
            get { return _AutoRunList; }
            set { SetProperty(ref _AutoRunList, value); }
        }

        private KeyValuePair<string, Action<Mat>> _AutoRun;

        public KeyValuePair<string, Action<Mat>> AutoRun
        {
            get { return _AutoRun; }
            set
            {
                SetProperty(ref _AutoRun, value);
            }
        }

        private DelegateCommand _GoAutoRun;

        public DelegateCommand GoAutoRun =>
             _GoAutoRun ??= new DelegateCommand(ExecuteGoAutoRun);

        private void ExecuteGoAutoRun()
        {
            if (!Pool.SelectImage.HasValue) return;
            Src = Pool.SelectImage.Value.Value;
            if (Src is null || Src.Empty()) return;
            GC.Collect();
            AutoRunsw.Restart();
            AutoRun.Value.Invoke(Src);
            AutoRunsw.Stop();
            CT = AutoRunsw.ElapsedMilliseconds;

            ImgDst = WriteableBitmapConverter.ToWriteableBitmap(Dst);
        }

        private KeyValuePair<string, string> _SelectedCamera;

        public KeyValuePair<string, string> SelectedCamera
        {
            get { return _SelectedCamera; }
            set
            {
                SetProperty(ref _SelectedCamera, value);
            }
        }

        private DelegateCommand _GetImageROI;

        public DelegateCommand GetImageROI =>
             _GetImageROI ??= new DelegateCommand(ExecuteGetImageROI);

        private void ExecuteGetImageROI()
        {
            if (!Pool.SelectImage.HasValue || !Pool.SelectImage.Value.Value.GetBgr(out Src)) return;
            //选区超出图像
            if (Pool.ROILeft + Pool.ROIWidth > Src.Width || Pool.ROITop + Pool.ROIHeight > Src.Height)
            {
                Pool.ROILeft = 0;
                Pool.ROIWidth = Src.Width;
                Pool.ROITop = 0;
                Pool.ROIHeight = Src.Height;
            }
            Rect rect = new(Pool.ROILeft, Pool.ROITop, Pool.ROIWidth, Pool.ROIHeight);

            Dst = Src[rect].Clone();

            ImgDst = WriteableBitmapConverter.ToWriteableBitmap(Dst);
        }

        private DelegateCommand _GetImageROIMask;

        public DelegateCommand GetImageROIMask =>
             _GetImageROIMask ??= new DelegateCommand(ExecuteGetImageROIMask);

        private void ExecuteGetImageROIMask()
        {
            if (!Pool.SelectImage.HasValue || !Pool.SelectImage.Value.Value.GetGray(out Src)) return;
            //选区超出图像
            if (Pool.ROILeft + Pool.ROIWidth > Src.Width || Pool.ROITop + Pool.ROIHeight > Src.Height)
            {
                Pool.ROILeft = 0;
                Pool.ROIWidth = Src.Width;
                Pool.ROITop = 0;
                Pool.ROIHeight = Src.Height;
            }
            Rect rect = new(Pool.ROILeft, Pool.ROITop, Pool.ROIWidth, Pool.ROIHeight);

            Dst = Src * 0;
            var mask = new Mat(rect.Height, rect.Width, MatType.CV_8UC1, Scalar.White);

            Dst[rect] = mask;
            ImgDst = WriteableBitmapConverter.ToWriteableBitmap(Dst);
        }

        private DelegateCommand _GetOneImageROI;

        public DelegateCommand GetOneImageROI =>
             _GetOneImageROI ??= new DelegateCommand(ExecuteGetOneImageROI);

        private async void ExecuteGetOneImageROI()
        {
            if (SelectedCamera.Value is null)
            {
                threadEnable = true;
                return;
            }
            sw.Restart();
            Rect rect = new(Pool.ROILeft, Pool.ROITop, Pool.ROIWidth, Pool.ROIHeight);
            // 拍照
            switch (SelectedCamera.Key)
            {
                case "Hikrobot":
                    Src = await HikrobotCameras.GetOneImage(SelectedCamera.Value, ExposureTime, rect);
                    break;
                case "Basler":
                    Src = await BaslerCameras.GetOneImage(SelectedCamera.Value, ExposureTime, rect);
                    break;
                default:
                    break;
            }

            sw.Stop();
            GrabTime = sw.Elapsed.TotalMilliseconds;

            UpdateSrc();
        }

        private DelegateCommand _getOneImage;

        public DelegateCommand GetOneImage =>
                    _getOneImage ??= new DelegateCommand(ExecuteGetOneImage, () => threadEnable);

        private readonly Stopwatch sw = new();

        private bool threadEnable = true;

        private async void ExecuteGetOneImage()
        {
            if (SelectedCamera.Value is null)
            {
                threadEnable = true;
                return;
            }
            sw.Restart();

            switch (SelectedCamera.Key)
            {
                case "Hikrobot":
                    Src = await HikrobotCameras.GetOneImage(SelectedCamera.Value, ExposureTime, new(0, 0, 0, 0));
                    break;
                case "Basler":
                    Src = await BaslerCameras.GetOneImage(SelectedCamera.Value, ExposureTime, new(0, 0, 0, 0));
                    break;
                default:
                    break;
            }
            // 拍照

            sw.Stop();
            GrabTime = sw.Elapsed.TotalMilliseconds;
            Pool.IsCamera = true;
            UpdateSrc();
        }

        private int exposureTime = 10000;

        public int ExposureTime
        {
            get { return exposureTime; }
            set { SetProperty(ref exposureTime, value); }
        }

        private double _showTime;

        public double ShowTime
        {
            get { return _showTime; }
            set { SetProperty(ref _showTime, value); }
        }

        private double _grabTime;

        public double GrabTime
        {
            get { return _grabTime; }
            set { SetProperty(ref _grabTime, value); }
        }

        private Mat Src;

        #region Usb Camera

        private int _CameraIndex;

        public int CameraIndex
        {
            get { return _CameraIndex; }
            set { SetProperty(ref _CameraIndex, value); }
        }

        private VideoCapture capture;
        private DelegateCommand _GetUsbImage;

        public DelegateCommand GetUsbImage =>
             _GetUsbImage ??= new DelegateCommand(ExecuteGetUsbImage);

        private bool Busy;

        async private void ExecuteGetUsbImage()
        {
            if (Busy) return;
            try
            {
                Busy = true;
                if (CameraIndex != UsbID || capture is null)
                {
                    capture = new VideoCapture(CameraIndex, VideoCaptureAPI.Value);

                    if (!capture.IsOpened()) return;

                    capture.Set(VideoCaptureProperties.FrameWidth, FrameWidth);
                    capture.Set(VideoCaptureProperties.FrameHeight, FrameHeight);
                    UsbID = CameraIndex;
                    await Task.Delay(500);
                }

                if (!capture.IsOpened()) return;

                sw.Restart();

                // 拍照
                Src ??= new();
                var getdone = capture.Read(Src);

                sw.Stop();

                GrabTime = sw.Elapsed.TotalMilliseconds;

                if (!getdone) return;

                UpdateSrc();

                await Task.Delay(10);
            }
            finally
            {
                Busy = false;
            }
        }

        private ObservableDictionary<string, VideoCaptureAPIs> _VideoCaptureAPIsList = new();

        public ObservableDictionary<string, VideoCaptureAPIs> VideoCaptureAPIsList
        {
            get { return _VideoCaptureAPIsList; }
            set { SetProperty(ref _VideoCaptureAPIsList, value); }
        }

        private KeyValuePair<string, VideoCaptureAPIs> _VideoCaptureAPI;

        public KeyValuePair<string, VideoCaptureAPIs> VideoCaptureAPI
        {
            get { return _VideoCaptureAPI; }
            set
            {
                SetProperty(ref _VideoCaptureAPI, value);
                capture?.Dispose();
                capture = null;
            }
        }

        private BindingList<string> _CaptureModeList = new()
        {
            "640 X 480",
            "800 X 600",
            "1280 X 720",
            "1600 X 1200",
            "1920 X 1080",
            "2048 X 1536"
        };

        public BindingList<string> CaptureModeList
        {
            get { return _CaptureModeList; }
            set
            {
                SetProperty(ref _CaptureModeList, value);
            }
        }

        private string _CaptureMode = "2048 X 1536";

        public string CaptureMode
        {
            get { return _CaptureMode; }
            set
            {
                SetProperty(ref _CaptureMode, value);
                var width_height = _CaptureMode.Replace(" ", "").Split("X");
                if (width_height.Length < 2
                    || !int.TryParse(width_height[0], out int width)
                    || !int.TryParse(width_height[1], out int height))
                    return;

                FrameWidth = width;
                FrameHeight = height;
                capture?.Dispose();
                capture = null;
            }
        }

        private int _FrameWidth = 2048;

        public int FrameWidth
        {
            get { return _FrameWidth; }
            set { SetProperty(ref _FrameWidth, value); }
        }

        private int _FrameHeight = 1536;

        public int FrameHeight
        {
            get { return _FrameHeight; }
            set { SetProperty(ref _FrameHeight, value); }
        }

        private int UsbID = 10;

        #endregion Usb Camera

        #region 载入 保存

        private DelegateCommand _LoadFiles;

        public DelegateCommand LoadFiles =>
             _LoadFiles ??= new DelegateCommand(ExecuteLoadFiles);

        private void ExecuteLoadFiles()
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new()
            {
                Title = "选择文件夹",
                Filter = "文件夹|*.directory",
                FileName = "选择此文件夹",

                ValidateNames = false,
                CheckFileExists = false,
                CheckPathExists = true,
                Multiselect = true,//允许同时选择多个文件
                //InitialDirectory = FilePath//指定启动路径
            };

            if (openFileDialog.ShowDialog() == true)

            {
                var path = openFileDialog.FileName.Replace("选择此文件夹.directory", "");

                if (!System.IO.Directory.Exists(path))
                {
                    System.Windows.MessageBox.Show(path + "文件夹不存在", "选择文件提示");
                    return;
                }
                // Pool.Images.Clear();
                var files = System.IO.Directory.GetFiles(path); // 获取多个图片
                foreach (var file in files)
                {
                    var imageInput = new Mat(file, ImreadModes.AnyColor);
                    if (imageInput is null || imageInput.Width == 0)
                        continue;
                    FileInfo finfo = new(file);

                    Pool.Images.Add(finfo.Name, imageInput);
                }
            }
        }

        private DelegateCommand _goStorageWrite;

        public DelegateCommand GoStorageWrite =>
             _goStorageWrite ??= new DelegateCommand(ExecuteGoStorageWrite);

        private void ExecuteGoStorageWrite()
        {
            if (!Pool.SelectImage.HasValue) return;

            Src = Pool.SelectImage.Value.Value;
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "xml文件|*.xml",
                Title = "Save File"
            };
            saveFileDialog.ShowDialog();

            if (string.IsNullOrEmpty(saveFileDialog.FileName)) return;
            try
            {
                FileStorage storagewrite = new(saveFileDialog.FileName, FileStorage.Modes.Write);
                storagewrite.Write(saveFileDialog.SafeFileName.Split(".")[0], Src);
                storagewrite.Release();
            }
            catch (Exception)
            {
                ShowError("序列化失败，文件名称需要符合变量命名规则。 ");
            }
        }

        private DelegateCommand _goStorageRead;

        public DelegateCommand GoStorageRead =>
             _goStorageRead ??= new DelegateCommand(ExecuteGoStorageRead);

        private void ExecuteGoStorageRead()
        {
            OpenFileDialog ofd = new()
            {
                DefaultExt = ".*",
                Filter = "xml文件|*.xml"
            };
            if (ofd.ShowDialog() == true)
            {
                try
                {
                    FileStorage storageread = new(ofd.FileName, FileStorage.Modes.Read);
                    Src = storageread[ofd.SafeFileName.Split(".")[0]].ToMat();
                    if (!Src.Empty())
                        UpdateSrc();
                    else

                        ShowError("反序列化失败，检查文件。");
                }
                catch
                {
                    // NLogService.Error(ex.Message);
                    ShowError("反序列化失败，检查文件。");
                }
            }
        }

        private DelegateCommand _GoSaveImage;

        public DelegateCommand GoSaveImage =>
             _GoSaveImage ??= new DelegateCommand(ExecuteGoSaveImage);

        private void ExecuteGoSaveImage()
        {
            if (!Pool.SelectImage.HasValue || Pool.SelectImage.Value.Value.Empty()) return;

            Src = Pool.SelectImage.Value.Value;
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "图片(*.jpg;*.png;*.jpeg;*.bmp)|*.jpg;*.png;*.jpeg;*.bmp",
                Title = "Save File"
            };
            if (saveFileDialog.ShowDialog() == false) return;
            if (string.IsNullOrEmpty(saveFileDialog.FileName)) return;

            Cv2.ImWrite(saveFileDialog.FileName, Src);
        }

        private DelegateCommand _loadImageFile;

        public DelegateCommand LoadImageFile =>
                _loadImageFile ??= new DelegateCommand(ExecuteLoadImageFile);

        private void ExecuteLoadImageFile()
        {
            OpenFileDialog ofd = new()
            {
                DefaultExt = ".*",
                Filter = "图像文件(*.jpg;*.png;*.bmp)|*.jpg;*.png;*.bmp"
            };
            if (ofd.ShowDialog() == true)
            {
                try
                {
                    Src = Cv2.ImRead(ofd.FileName);
                    if(!Src.Empty())
                        UpdateSrc();
                    else
                        ShowError("不支持的图像格式");
                }
                catch (Exception ex)
                {
                    NLogService.Error(ex.Message);
                   
                }
            }
        }

        #endregion 载入 保存

        private string _Text = "测试文字";

        public string Text
        {
            get { return _Text; }
            set { SetProperty(ref _Text, value); }
        }

        private DelegateCommand _DrawText;

        public DelegateCommand DrawText =>
             _DrawText ??= new DelegateCommand(ExecuteDrawText);

        private void ExecuteDrawText()
        {
            if (!Pool.SelectImage.HasValue) return;
            Src = Pool.SelectImage.Value.Value;
            if (Src is null || Src.Empty()) return;
            Dst = Src.Clone();

            AutoRunsw.Restart();
            Dst.PutTextZh(Text, new Point(Pool.ROILeft, Pool.ROITop), FontSize);
            AutoRunsw.Stop();
            CT = AutoRunsw.ElapsedMilliseconds;
            ImgDst = WriteableBitmapConverter.ToWriteableBitmap(Dst);
            Dst.Dispose();
        }

        private float _FontSize = 24;
        public float FontSize
        {
            get { return _FontSize; }
            set { SetProperty(ref _FontSize, value); }
        }
    }
}