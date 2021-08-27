using ModuleCore.Mvvm;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using OpencvsharpModule.Common;
using OpencvsharpModule.Models;
using OpencvsharpModule.Views;
using Prism.Commands;
using Prism.Ioc;
using Prism.Regions;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Imaging;
using static OpenCvSharp.ConnectedComponents;

namespace OpencvsharpModule.ViewModels
{
    internal class ConnectedViewModel : RegionViewModelBase
    {
        public ImagePool Pool { get; set; }
        private string _ViewName;

        public string ViewName
        {
            get { return _ViewName; }
            set { SetProperty(ref _ViewName, value); }
        }

        public ConnectedViewModel(IContainerExtension container, IRegionManager regionManager) : base(regionManager)
        {
            Pool = container.Resolve<ImagePool>();
            ViewName = this.GetType().Name;
            regionManager.RegisterViewWithRegion(ViewName, typeof(CommonView));
        }

        private string commandText;

        public string CommandText
        {
            get { return commandText; }
            set { SetProperty(ref commandText, value); }
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

        private System.Diagnostics.Stopwatch sw = new();

        private string matName;

        public string MatName
        {
            get { return matName; }
            set { SetProperty(ref matName, value); }
        }

        private DelegateCommand _addMat;

        public DelegateCommand AddMat =>
                _addMat ??= new DelegateCommand(ExecuteAddMat);

        private int add;

        private void ExecuteAddMat()
        {
            if (Dst == null) return;
            MatName ??= "Connected" + add;
            while (Pool.Images.ContainsKey(MatName))
            {
                MatName = "Connected" + add++;
            }
            Pool.Images[MatName] = Dst.Clone();
        }

        private int blobCount;

        public int BlobCount
        {
            get { return blobCount; }
            set { SetProperty(ref blobCount, value); }
        }

        public Mat Src;
        public Mat Dst;
        public Mat Gray;

        private ConnectedComponents Cc;
        private DelegateCommand<string> _goExecute;

        public DelegateCommand<string> GoExecute =>
               _goExecute ??= new DelegateCommand<string>(ExecuteGoExecute);

        private void ExecuteGoExecute(string func)
        {
            IsNewValue = true;
            if (IsRunning) return;

            try
            {
                IsRunning = true;

                while (IsNewValue)
                {
                    IsNewValue = false;
                    if (!Pool.SelectImage.HasValue || Pool.SelectImage.Value.Value.Empty()) continue;

                    if (Src != Pool.SelectImage.Value.Value)
                    {
                        Src = Pool.SelectImage.Value.Value;

                        Src.GetGray(out Gray);

                        Mat binary = Gray.Threshold(0, 255, ThresholdTypes.Otsu | ThresholdTypes.Binary);
                        sw.Restart();
                        Cc = Cv2.ConnectedComponentsEx(binary);
                        sw.Stop();
                        CT = sw.ElapsedMilliseconds;
                    }
                    if (Cc is null || Cc.LabelCount <= 1) continue;

                    BlobDictionary.Clear();

                    //得到去除背景的blob列表
                    Blobs = Cc.Blobs.Skip(1).OrderBy(b => b.Area).ToList();

                    AreaLargest = Blobs.LastOrDefault().Area;

                    var i = 1;

                    foreach (var blob in Blobs)
                    {
                        if (blob.Area < AreaLow || blob.Area > AreaHigh) continue;

                        BlobDictionary[$"{i}"] = blob;
                        i++;
                    }

                    //画出总Mask
                    if (BlobDictionary.Count > 0)
                    {
                        Dst = new();
                        Cc.FilterByBlobs(Src, Dst, BlobDictionary.Values);
                    }
                    else
                    {
                        Dst = Src.EmptyClone();
                    }
                    ImgDst = WriteableBitmapConverter.ToWriteableBitmap(Dst);
                }
            }
            finally
            {
                IsRunning = false;
            }
        }

        private bool IsRunning, IsNewValue;

        private List<Blob> Blobs = new();

        //_blobList
        private ObservableDictionary<string, Blob> _blobDictionary = new ObservableDictionary<string, Blob>();

        public ObservableDictionary<string, Blob> BlobDictionary
        {
            get { return _blobDictionary; }
            set { SetProperty(ref _blobDictionary, value); }
        }

        private KeyValuePair<string, Blob>? _SelectBlob;

        public KeyValuePair<string, Blob>? SelectBlob
        {
            get { return _SelectBlob; }
            set
            {
                SetProperty(ref _SelectBlob, value);
                if (value is null)
                {
                    BlobInfo = "";
                    return;
                }
                Dst = new();
                BlobInfo = $" 面积：{value.Value.Value.Area}，质心：{value.Value.Value.Centroid.X:F2} : {value.Value.Value.Centroid.Y:F2} ";
                Cc.FilterByBlob(Src, Dst, value.Value.Value);
                ImgDst = WriteableBitmapConverter.ToWriteableBitmap(Dst);
            }
        }

        private int _AreaHigh = 10000;

        public int AreaHigh
        {
            get { return _AreaHigh; }
            set { SetProperty(ref _AreaHigh, value); ExecuteGoExecute("GetBlobs"); }
        }

        private int _AreaLow = 100;

        public int AreaLow
        {
            get { return _AreaLow; }
            set
            {
                SetProperty(ref _AreaLow, value);
                ExecuteGoExecute("GetBlobs");
            }
        }

        private int _AreaLargest = 10000;

        public int AreaLargest
        {
            get { return _AreaLargest; }
            set { SetProperty(ref _AreaLargest, value); }
        }

        private string _BlobInfo;

        public string BlobInfo
        {
            get { return _BlobInfo; }
            set { SetProperty(ref _BlobInfo, value); }
        }
    }
}