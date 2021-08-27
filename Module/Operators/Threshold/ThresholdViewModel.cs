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
    partial class ThresholdViewModel : RegionViewModelBase
    {
        public ImagePool Pool { get; set; }
        private string _ViewName;
        public string ViewName
        {
            get { return _ViewName; }
            set { SetProperty(ref _ViewName, value); }
        }
        public ThresholdViewModel(IContainerExtension container, IRegionManager regionManager) : base(regionManager)
        {
            ViewName = this.GetType().Name;
            regionManager.RegisterViewWithRegion(ViewName, typeof(CommonView));
            Pool = container.Resolve<ImagePool>();
            ThresholdTypeList.Add("Binary", ThresholdTypes.Binary);
            ThresholdTypeList.Add("BinaryInv", ThresholdTypes.BinaryInv);
            ThresholdTypeList.Add("Tozero", ThresholdTypes.Tozero);
            ThresholdTypeList.Add("TozeroInv", ThresholdTypes.TozeroInv);
            ThresholdTypeList.Add("Trunc", ThresholdTypes.Trunc);
            AdaptiveThresholdTypeList.Add("GaussianC", AdaptiveThresholdTypes.GaussianC);
            AdaptiveThresholdTypeList.Add("MeanC", AdaptiveThresholdTypes.MeanC);
        }

        private ObservableDictionary<string, ThresholdTypes> _thresholdTypeList = new();

        public ObservableDictionary<string, ThresholdTypes> ThresholdTypeList
        {
            get { return _thresholdTypeList; }
            set { SetProperty(ref _thresholdTypeList, value); }
        }

        private ThresholdTypes _thresholdTypeThis;

        public ThresholdTypes ThresholdTypeThis
        {
            get { return _thresholdTypeThis; }
            set { SetProperty(ref _thresholdTypeThis, value); }
        }

        private bool _isOtsu;

        public bool IsOtsu
        {
            get { return _isOtsu; }
            set
            {
                if (value)
                {
                    IsTriangle = false;
                }
                SetProperty(ref _isOtsu, value);
                Threshold();
            }
        }

        private bool _isTriangle;

        public bool IsTriangle
        {
            get { return _isTriangle; }
            set
            {
                if (value)
                {
                    IsOtsu = false;
                }
                SetProperty(ref _isTriangle, value);
                Threshold();
            }
        }

        private string matName;

        public string MatName
        {
            get { return matName; }
            set { SetProperty(ref matName, value); }
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

        public Mat Src { get; set; } = new Mat();
        public Mat Dst { get; set; } = new Mat();

        #region Command

        private DelegateCommand _addMat;

        public DelegateCommand AddMat =>
                _addMat ??= new DelegateCommand(ExecuteAddMat);

        private int add;

        private void ExecuteAddMat()
        {
            if (Dst == null) return;
            MatName ??= "Threshold" + add;
            while (Pool.Images.ContainsKey(MatName))
            {
                MatName = "Threshold" + add++;
            }
            Pool.Images[MatName] = Dst.Clone();
        }

        private DelegateCommand _GoThreshold;

        public DelegateCommand GoThreshold =>
             _GoThreshold ??= new DelegateCommand(ExecuteGoThreshold);

        private void ExecuteGoThreshold()
        {
            Threshold();
        }

        private int _BarThresh = 100;

        public int BarThresh
        {
            get { return _BarThresh; }
            set
            {
                if (IsTriangle || IsOtsu)
                {
                    _BarThresh = 0;
                    return;
                }
                SetProperty(ref _BarThresh, value); Threshold();
            }
        }

        private int _BarMax = 255;

        public int BarMax
        {
            get { return _BarMax; }
            set { SetProperty(ref _BarMax, value); Threshold(); }
        }

        private void Threshold()
        {
            if (!Pool.SelectImage.HasValue) return;

            if (Pool.SelectImage.Value.Value.Channels() == 3)
                Src = Pool.SelectImage.Value.Value.CvtColor(ColorConversionCodes.BGR2GRAY);
            else Pool.SelectImage.Value.Value.CopyTo(Src);

            ThresholdTypes type = ThresholdTypeThis;
            var typeText = $"ThresholdTypes.{ThresholdTypeThis}";
            if (IsOtsu)
            {
                type = ThresholdTypeThis | ThresholdTypes.Otsu;
                typeText += "| ThresholdTypes.Otsu";
            }
            if (IsTriangle)
            {
                type = ThresholdTypeThis | ThresholdTypes.Triangle;
                typeText += "| ThresholdTypes.Triangle";
            }
            Cv2.Threshold(Src, Dst, BarThresh, BarMax, type);

            CommandText = $"Cv2.Threshold(Src, Dst, {BarThresh}, {BarMax}, {typeText});";

            ImgDst = WriteableBitmapConverter.ToWriteableBitmap(Dst);
        }

        #endregion Command
    }

    #region AdaptiveThreshold

    partial class ThresholdViewModel
    {
        private ObservableDictionary<string, AdaptiveThresholdTypes> _adaptivethresholdTypeList = new();

        public ObservableDictionary<string, AdaptiveThresholdTypes> AdaptiveThresholdTypeList
        {
            get { return _adaptivethresholdTypeList; }
            set { SetProperty(ref _adaptivethresholdTypeList, value); }
        }

        private AdaptiveThresholdTypes _adaptivethresholdTypeThis;

        public AdaptiveThresholdTypes AdaptiveThresholdTypeThis
        {
            get { return _adaptivethresholdTypeThis; }
            set { SetProperty(ref _adaptivethresholdTypeThis, value); }
        }

        private DelegateCommand _goAdaptiveThreshold;

        public DelegateCommand GoAdaptiveThreshold =>
               _goAdaptiveThreshold ??= new DelegateCommand(ExecuteGoAdaptiveThreshold);

        private void ExecuteGoAdaptiveThreshold()
        {
            if (!Pool.SelectImage.HasValue) return;

            if (Pool.SelectImage.Value.Value.Channels() == 3)
                Src = Pool.SelectImage.Value.Value.CvtColor(ColorConversionCodes.BGR2GRAY);
            else Pool.SelectImage.Value.Value.CopyTo(Src);

            if (ThresholdTypeThis == ThresholdTypes.Binary || ThresholdTypeThis == ThresholdTypes.BinaryInv)
                Cv2.AdaptiveThreshold(Src, Dst, AdaptiveThreshBar, AdaptiveThresholdTypeThis, ThresholdTypeThis, BlockSize, c);
            else
                ThresholdTypeThis = ThresholdTypes.Binary;
            ImgDst = WriteableBitmapConverter.ToWriteableBitmap(Dst);
            CommandText = $"Cv2.AdaptiveThreshold(Src, Dst, {AdaptiveThreshBar}, {AdaptiveThresholdTypeThis}, {ThresholdTypeThis},{BlockSize},{c});";
        }

        private int _AdaptiveThreshBar = 255;

        public int AdaptiveThreshBar
        {
            get { return _AdaptiveThreshBar; }
            set { SetProperty(ref _AdaptiveThreshBar, value); ExecuteGoAdaptiveThreshold(); }
        }

        private int blockSize = 3;

        public int BlockSize
        {
            get { return blockSize; }
            set
            {
                if (value < 3) return;
                if (value % 2 != 1) value++;
                SetProperty(ref blockSize, value);
                ExecuteGoAdaptiveThreshold();
            }
        }

        private int c = 2;

        public int C
        {
            get { return c + 50; }
            set
            {
                SetProperty(ref c, value - 50);
                ExecuteGoAdaptiveThreshold();
            }
        }


    #endregion AdaptiveThreshold


    #region inrange
    private void GoInRange()
    {
        if (!Pool.SelectImage.HasValue || Pool.SelectImage.Value.Value.Empty()) return;
        Src = Pool.SelectImage.Value.Value;
        if (Src.Type() != MatType.CV_8UC3) return;

        Src = Src.CvtColor(ColorConversionCodes.BGR2HSV);
        Scalar low = new Scalar(HLow, SLow, VLow);
        Scalar high = new Scalar(HHigh, SHigh, VHigh);
        Cv2.InRange(Src, low, high, Dst);
        ImgDst = WriteableBitmapConverter.ToWriteableBitmap(Dst);
        CommandText = $"Cv2.InRange(Src,  new Scalar({HLow}, {SLow}, {VLow}), new Scalar({HHigh}, {SHigh},{ VHigh}), Dst);";
    }

    private int _HLow = 1;

    public int HLow
    {
        get { return _HLow; }
        set
        {
            SetProperty(ref _HLow, value);
            GoInRange();
        }
    }

    private int _HHigh = 180;

    public int HHigh
    {
        get { return _HHigh; }
        set
        {
            SetProperty(ref _HHigh, value);
            GoInRange();
        }
    }

    private int _SLow = 1;

    public int SLow
    {
        get { return _SLow; }
        set
        {
            SetProperty(ref _SLow, value);
            GoInRange();
        }
    }

    private int _SHigh = 255;

    public int SHigh
    {
        get { return _SHigh; }
        set
        {
            SetProperty(ref _SHigh, value);
            GoInRange();
        }
    }

    private int _VLow = 1;

    public int VLow
    {
        get { return _VLow; }
        set
        {
            SetProperty(ref _VLow, value);
            GoInRange();
        }
    }

    private int _VHigh = 255;

    public int VHigh
    {
        get { return _VHigh; }
        set
        {
            SetProperty(ref _VHigh, value);
            GoInRange();
        }
    }
        #endregion
    }
}