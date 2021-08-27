using ModuleCore.Mvvm;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using OpencvsharpModule.Common;
using OpencvsharpModule.Models;
using OpencvsharpModule.Views;
using Prism.Commands;
using Prism.Ioc;
using Prism.Regions;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace OpencvsharpModule.ViewModels
{
    public class CornersViewModel : RegionViewModelBase
    {
        public ImagePool Pool { get; set; }
        private string _ViewName;

        public string ViewName
        {
            get { return _ViewName; }
            set { SetProperty(ref _ViewName, value); }
        }

        public CornersViewModel(IContainerExtension container, IRegionManager regionManager) : base(regionManager)
        {
            Pool = container.Resolve<ImagePool>();
            ViewName = this.GetType().Name;
            regionManager.RegisterViewWithRegion(ViewName, typeof(CommonView));
        }

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

        private System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

        private Mat Src;
        private Mat Dst;
        private Mat Gray;
        private Mat Mask;

        private DelegateCommand _GoClearMask;

        public DelegateCommand GoClearMask =>
             _GoClearMask ??= new DelegateCommand(ExecuteGoClearMask);

        private void ExecuteGoClearMask()
        {
            Pool.SelectMask = null;
            Pool.MaskSrc = null;
        }

        private bool NewValue, Runing;
        private DelegateCommand _GoGoodFeatures;

        public DelegateCommand GoGoodFeatures =>
             _GoGoodFeatures ??= new DelegateCommand(ExecuteGoGoodFeatures);

        async private void ExecuteGoGoodFeatures()
        {
            NewValue = true;
          
            if (Runing) return;
            Runing = true;
            try
            {
                if (!Pool.SelectImage.HasValue) return;
                if (!Pool.SelectImage.Value.Value.GetGrayAndBgr(out Gray, out Dst)) return;
                Point2f[] corners = System.Array.Empty<Point2f>();
                while (NewValue)
                {
                    NewValue = false;

                    if (Pool.SelectMask.HasValue) Mask = Pool.SelectMask.Value.Value;
                    else
                        Mask = null;
                    if (Mask == Pool.SelectImage.Value.Value) return;
                    sw.Restart();
                    //角点检测
                    await Task.Run(() => corners = Cv2.GoodFeaturesToTrack(Gray,
                                                 MaxCorners,
                                                 QualityLevel,
                                                 MinDistance,
                                                 Mask,
                                                 BlockSize,
                                                 UseHarris,
                                                 K
                                        ));
                    sw.Stop();
                    CT = sw.ElapsedMilliseconds;
                }
                Dst *= 0;
                //将检测到的角点绘制到原图上
                for (int i = 0; i < corners.Length; i++)
                {
                    Cv2.Circle(Dst, (Point)corners[i], 1, Scalar.RandomColor());
                }

                ImgDst = WriteableBitmapConverter.ToWriteableBitmap(Dst);
            }
            finally
            {
                Runing = false;
            }
        }

        private double _MinDistance = 3.0;

        public double MinDistance
        {
            get { return _MinDistance; }
            set { SetProperty(ref _MinDistance, value); ExecuteGoGoodFeatures(); }
        }

        private double _QualityLevel = 0.01;

        public double QualityLevel
        {
            get { return _QualityLevel; }
            set { SetProperty(ref _QualityLevel, value); ExecuteGoGoodFeatures(); }
        }

        private int _MaxCorners = 200;

        public int MaxCorners
        {
            get { return _MaxCorners; }
            set { SetProperty(ref _MaxCorners, value); ExecuteGoGoodFeatures(); }
        }

        private bool _UseHarris = false;

        public bool UseHarris
        {
            get { return _UseHarris; }
            set { SetProperty(ref _UseHarris, value); ExecuteGoGoodFeatures(); }
        }

        private int _BlockSize = 3;

        public int BlockSize
        {
            get { return _BlockSize; }
            set { SetProperty(ref _BlockSize, value); ExecuteGoGoodFeatures(); }
        }

        private double _K = 0.04;

        public double K
        {
            get { return _K; }
            set { SetProperty(ref _K, value); ExecuteGoGoodFeatures(); }
        }
    }
}