using ModuleCore.Mvvm;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using OpencvsharpModule.Models;
using OpencvsharpModule.Views;
using Prism.Commands;
using Prism.Ioc;
using Prism.Regions;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace OpencvsharpModule.ViewModels
{
    //CV_MOP_OPEN - 开运算
    //CV_MOP_CLOSE - 闭运算
    //CV_MOP_GRADIENT - 形态梯度
    //CV_MOP_TOPHAT - 顶帽
    //CV_MOP_BLACKHAT - 黑帽
    public class MorphologyViewModel : RegionViewModelBase
    {
        public ImagePool Pool { get; set; }
        private string _ViewName;
        public string ViewName
        {
            get { return _ViewName; }
            set { SetProperty(ref _ViewName, value); }
        }
        public MorphologyViewModel(IContainerExtension container, IRegionManager regionManager) : base(regionManager)
        {
            ViewName = this.GetType().Name;
            regionManager.RegisterViewWithRegion(ViewName, typeof(CommonView));
            Pool = container.Resolve<ImagePool>();
            MorphShapeList.Add("Cross", MorphShapes.Cross);
            MorphShapeList.Add("Ellipse", MorphShapes.Ellipse);
            MorphShapeList.Add("Rect", MorphShapes.Rect);

            MorphTypeList.Add("开运算 Open", MorphTypes.Open);
            MorphTypeList.Add("闭运算 Close", MorphTypes.Close);
            MorphTypeList.Add("腐蚀 Erode", MorphTypes.Erode);
            MorphTypeList.Add("膨胀 Dilate", MorphTypes.Dilate);
            MorphTypeList.Add("黑帽 BlackHat", MorphTypes.BlackHat);
            MorphTypeList.Add("顶帽 TopHat", MorphTypes.TopHat);
            MorphTypeList.Add("梯度 Gradient", MorphTypes.Gradient);
            MorphTypeList.Add("击中击不中 HitMiss", MorphTypes.HitMiss);
            MorphTypeThis = MorphTypes.Open;
        }

        private MorphShapes _morphShapeThis;

        public MorphShapes MorphShapeThis
        {
            get { return _morphShapeThis; }
            set { SetProperty(ref _morphShapeThis, value); }
        }

        //运算的类型
        private ObservableDictionary<string, MorphTypes> _morphTypeList = new ObservableDictionary<string, MorphTypes>();

        public ObservableDictionary<string, MorphTypes> MorphTypeList
        {
            get { return _morphTypeList; }
            set { SetProperty(ref _morphTypeList, value); }
        }

        private MorphTypes _morphTypeThis;

        public MorphTypes MorphTypeThis
        {
            get { return _morphTypeThis; }
            set { SetProperty(ref _morphTypeThis, value); }
        }

        //卷积核
        private ObservableDictionary<string, MorphShapes> _morphShapeList = new();

        public ObservableDictionary<string, MorphShapes> MorphShapeList
        {
            get { return _morphShapeList; }
            set { SetProperty(ref _morphShapeList, value); }
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

        private int _KernelWidth = 3;

        public int KernelWidth
        {
            get { return _KernelWidth; }
            set { SetProperty(ref _KernelWidth, value); GoMorphology(); }
        }

        private int _KernelHeight = 3;

        public int KernelHeight
        {
            get { return _KernelHeight; }
            set { SetProperty(ref _KernelHeight, value); GoMorphology(); }
        }

        #region Command

        private DelegateCommand _addMat;

        public DelegateCommand AddMat =>
                _addMat ??= new DelegateCommand(ExecuteAddMat);

        private int add;

        private void ExecuteAddMat()
        {
            if (Dst == null) return;
            MatName ??= "Morphology" + add;
            while (Pool.Images.ContainsKey(MatName))
            {
                MatName = "Morphology" + add++;
            }
            Pool.Images[MatName] = Dst.Clone();
        }

        private void GoMorphology()
        {
            if (!Pool.SelectImage.HasValue) return;
            Pool.SelectImage.Value.Value.CopyTo(Src);

            if (MorphTypeThis == MorphTypes.HitMiss && Src.Channels() == 3)
                Cv2.CvtColor(Src, Src, ColorConversionCodes.BGR2GRAY);

            if (KernelWidth < 1) KernelWidth = 1;
            if (KernelHeight < 1) KernelHeight = 1;

            InputArray kernel
                = Cv2.GetStructuringElement(
                    MorphShapeThis,
                    new Size(KernelWidth, KernelHeight));

            Cv2.MorphologyEx(src: Src, dst: Dst, op: MorphTypeThis, element: kernel, anchor: new Point(-1, -1));
            ImgDst = WriteableBitmapConverter.ToWriteableBitmap(Dst);
            CommandText = @$"InputArray kernel = Cv2.GetStructuringElement(MorphShapes.{MorphShapeThis},new Size({KernelWidth}, {KernelHeight}));
Cv2.MorphologyEx(src: Src, dst: Dst, op: MorphTypes.{MorphTypeThis}, element: kernel);";
        }

        #endregion Command


        #region Blur

        private bool _IsBlur;

        public bool IsBlur
        {
            get { return _IsBlur; }
            set { SetProperty(ref _IsBlur, value); }
        }

        private bool _IsMedianBlur;

        public bool IsMedianBlur
        {
            get { return _IsMedianBlur; }
            set { SetProperty(ref _IsMedianBlur, value); }
        }

        private bool _IsGaussianBlur = true;

        public bool IsGaussianBlur
        {
            get { return _IsGaussianBlur; }
            set { SetProperty(ref _IsGaussianBlur, value); }
        }

        private int _BlurWidth;

        public int BlurWidth
        {
            get { return _BlurWidth; }
            set { SetProperty(ref _BlurWidth, value); GoBlur(); }
        }

        private int _BlurHeight;

        public int BlurHeight
        {
            get { return _BlurHeight; }
            set { SetProperty(ref _BlurHeight, value); GoBlur(); }
        }

        private void GoBlur()
        {
            if (!GetSrc()) return;
            if (BlurWidth == 0) _BlurWidth = 1;
            if (BlurWidth % 2 == 0) _BlurWidth += 1;
            if (BlurHeight == 0) _BlurHeight = 1;
            if (BlurHeight % 2 == 0) _BlurHeight += 1;

            Cv2.Blur(Src, Dst, new Size(BlurWidth, BlurHeight));

            ImgDst = WriteableBitmapConverter.ToWriteableBitmap(Dst);

            CommandText = $"Cv2.Blur(Src, Dst, new Size({BlurWidth}, {BlurHeight}));";
        }

        private long _CT;

        public long CT
        {
            get { return _CT; }
            set { SetProperty(ref _CT, value); }
        }

        private int _BlurKernelWidth = 3;

        public int BlurKernelWidth
        {
            get { return _BlurKernelWidth; }
            set { SetProperty(ref _BlurKernelWidth, value); GoGaussianBlur(); }
        }

        private int _BlurKernelHeight = 3;

        public int BlurKernelHeight
        {
            get { return _BlurKernelHeight; }
            set { SetProperty(ref _BlurKernelHeight, value); GoGaussianBlur(); }
        }

        private void GoGaussianBlur()
        {
            if (!GetSrc()) return;
            if (BlurKernelWidth < 3) _BlurKernelWidth = 3;
            if (BlurKernelWidth % 2 == 0) _BlurKernelWidth += 1;
            if (BlurKernelHeight < 3) _BlurKernelHeight = 3;
            if (BlurKernelHeight % 2 == 0) _BlurKernelHeight += 1;

 
            Cv2.GaussianBlur(Src, Dst, new Size(BlurKernelWidth, BlurKernelHeight), 0);
 
            ImgDst = WriteableBitmapConverter.ToWriteableBitmap(Dst);
            CommandText = $"Cv2.GaussianBlur(Src, Dst, new Size({BlurKernelWidth}, {BlurKernelHeight}),0);";
        }

        private int _KernelSize;

        public int KernelSize
        {
            get { return _KernelSize; }
            set { SetProperty(ref _KernelSize, value); GoMedianBlur(); }
        }

        private bool Running;
        private bool NewValue;

        //执行最后一次的触发
        //新的触发到来时，改变委托的内容。
        //当有任务在运行时，循环等待任务结束，执行新的任务

        async private void GoMedianBlur()
        {
            if (KernelSize < 3) _KernelSize = 3;
            if (KernelSize % 2 == 0) _KernelSize += 1;
            NewValue = true;
            if (Running) return;

            try
            {
                Running = true;
                while (NewValue)
                {
                    NewValue = false;
                    if (!GetSrc()) return;
                    //耗时操作之后，检查有无新值进入
  
                    await Task.Run(() => Cv2.MedianBlur(Src, Dst, KernelSize));
      
                    ImgDst = WriteableBitmapConverter.ToWriteableBitmap(Dst);
                    CommandText = $"Cv2.MedianBlur(Src, Dst,{KernelSize});";
                }
            }
            finally
            {
                Running = false;
            }
        }

        private bool GetSrc()
        {
            if (!Pool.SelectImage.HasValue) return false;
            Src = Pool.SelectImage.Value.Value;
            if (Src is null || Src.Empty()) return false;

            return true;
        }

        #endregion Blur
    }
}