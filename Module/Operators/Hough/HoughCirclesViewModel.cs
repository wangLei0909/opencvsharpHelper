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

//当使用了 OTSU和 TRIANGLE两个标志时，输入图像必须为单通道。

namespace OpencvsharpModule.ViewModels
{
    public class HoughCirclesViewModel : RegionViewModelBase
    {
        public ImagePool Pool { get; set; }

        public HoughCirclesViewModel(IContainerExtension container, IRegionManager regionManager) : base(regionManager)
        {
            Pool = container.Resolve<ImagePool>();
            ViewName =  this.GetType().Name;
            regionManager.RegisterViewWithRegion(ViewName, typeof(CommonView));
        }
        private string _ViewName;
        public string ViewName
        {
            get { return _ViewName; }
            set { SetProperty(ref _ViewName, value); }
        }
        private bool _IsHSV;

        public bool IsHSV
        {
            get { return _IsHSV; }
            set { SetProperty(ref _IsHSV, value); }
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

        private WriteableBitmap _imgSource;

        public WriteableBitmap ImgSource
        {
            get { return _imgSource; }
            set { SetProperty(ref _imgSource, value); }
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
            MatName ??= "HoughCircle" + add;
            while (Pool.Images.ContainsKey(MatName))
            {
                MatName = "HoughCircle" + add++;
            }
            Pool.Images[MatName] = Dst.Clone();
        }

        private WriteableBitmap _imgDst;

        public WriteableBitmap ImgDst
        {
            get { return _imgDst; }
            set { SetProperty(ref _imgDst, value); }
        }

        private bool IsWorking;

        private Mat Gray;
        private bool NewValue;

        //原文链接：https://blog.csdn.net/weixin_41049188/article/details/92422241
      async  private void GoHoughCircles()
        {    //新的触发到来时， NewValue = true;
            NewValue = true;
            if (IsWorking) return;
            try
            {
                IsWorking = true;
                while (NewValue)
                {
                    NewValue = false;

                    if (!Pool.SelectImage.HasValue || Pool.SelectImage.Value.Value.Empty()) return;
                    if (Pool.SelectImage.Value.Value.Channels() == 3)
                    {
                        Gray = Pool.SelectImage.Value.Value.CvtColor(ColorConversionCodes.BGR2GRAY);

                        Src = Pool.SelectImage.Value.Value.Clone();
                    }
                    else
                    {
                        Gray = Pool.SelectImage.Value.Value.Clone();
                        Src = Pool.SelectImage.Value.Value.CvtColor(ColorConversionCodes.GRAY2BGR);
                    }

                    //霍夫圆检测：使用霍夫变换查找灰度图像中的圆。
                    /*
                     * 参数：
                     *      1：输入参数： 8位、单通道、灰度输入图像
                     *      2：实现方法：目前，唯一的实现方法是HoughCirclesMethod.Gradient
                     *      3: dp      :累加器分辨率与图像分辨率的反比。默认=1
                     *      4：minDist: 检测到的圆的中心之间的最小距离。(最短距离-可以分辨是两个圆的，否则认为是同心圆-                            src_gray.rows/8)
                     *      5:param1:   第一个方法特定的参数。[默认值是100] canny边缘检测阈值低
                     *      6:param2:   第二个方法特定于参数。[默认值是100] 中心点累加器阈值 – 候选圆心
                     *      7:minRadius: 最小半径
                     *      8:maxRadius: 最大半径
                     *
                     */
                  await Task.Run( ()=>
                      {
                          CircleSegment[] cs = Cv2.HoughCircles(Gray, HoughModes.Gradient, dp: 1, minDist: MinDist, Param1, Param2, MinRadius, MaxRadius);
                          Src.CopyTo(Dst);
                          Cv2.Line(Dst, new(0, 10), new( MaxRadius*2, 10), Scalar.Red, 3);
                          var startX = (MaxRadius * 2 - MinRadius * 2) /2;
                          Cv2.Line(Dst, new(startX, 10), new(startX+ MinRadius * 2, 10), Scalar.Blue, 3);
                          for (int i = 0; i < cs.Length; i++)
                          {
                              //画圆
                              Cv2.Circle(Dst, (int)cs[i].Center.X, (int)cs[i].Center.Y, (int)cs[i].Radius, new Scalar(0, 0, 255), 2, LineTypes.AntiAlias);
                   
                          }

                          CircleCount = cs.Length;
                      }
                    );

                    ImgDst = WriteableBitmapConverter.ToWriteableBitmap(Dst);
                    CommandText = $"Cv2.HoughCircles(Src, HoughModes.Gradient,1, minDist:{MinDist:F0},  param1:  {Param1:F2}, param2:{Param2:F2} ,minRadius:{MinRadius},maxRadius:{MaxRadius});";
                }
            }
            finally
            {
                IsWorking = false;
            }
        }

        private int _CircleCount;

        public int CircleCount
        {
            get { return _CircleCount; }
            set { SetProperty(ref _CircleCount, value); }
        }

        private int _MinRadius = 10;

        public int MinRadius
        {
            get { return _MinRadius; }
            set { SetProperty(ref _MinRadius, value); GoHoughCircles(); }
        }

        private int _MaxRadius = 100;

        public int MaxRadius
        {
            get { return _MaxRadius; }
            set { SetProperty(ref _MaxRadius, value); GoHoughCircles(); }
        }

        private double _Param1 = 100;

        public double Param1
        {
            get { return _Param1; }
            set { SetProperty(ref _Param1, value); GoHoughCircles(); }
        }

        private double _Param2 = 100;

        public double Param2
        {
            get { return _Param2; }
            set { SetProperty(ref _Param2, value); GoHoughCircles(); }
        }

        private double _MinDist = 50;

        public double MinDist
        {
            get { return _MinDist; }
            set { SetProperty(ref _MinDist, value); GoHoughCircles(); }
        }

        private DelegateCommand _GetMask;

        public DelegateCommand GetMask =>
             _GetMask ??= new DelegateCommand(ExecuteGetMask);

        private void ExecuteGetMask()
        {
            if (!Pool.SelectImage.HasValue || Pool.SelectImage.Value.Value.Empty()) return;
            if (Pool.SelectImage.Value.Value.Channels() == 3)
            {
                Gray = Pool.SelectImage.Value.Value.CvtColor(ColorConversionCodes.BGR2GRAY);

                Src = Pool.SelectImage.Value.Value.Clone();
            }
            else
            {
                Gray = Pool.SelectImage.Value.Value.Clone();
                Src = Pool.SelectImage.Value.Value.CvtColor(ColorConversionCodes.GRAY2BGR);
            }

            CircleSegment[] cs = Cv2.HoughCircles(Gray, HoughModes.Gradient, dp: 1, minDist: MinDist, Param1, Param2, MinRadius, MaxRadius);
            Dst = new(Src.Size(), MatType.CV_8UC1, Scalar.Black);
            for (int i = 0; i < cs.Length; i++)
            {
                //画圆
                Cv2.Circle(Dst, (int)cs[i].Center.X, (int)cs[i].Center.Y, (int)cs[i].Radius, Scalar.White, -1, LineTypes.AntiAlias);
            }

            ImgDst = WriteableBitmapConverter.ToWriteableBitmap(Dst);
            CommandText = $"Cv2.HoughCircles(Src, HoughModes.Gradient,1, minDist:{MinDist:F0},  param1:  {Param1:F2}, param2:{Param2:F2} ,minRadius:{MinRadius},maxRadius:{MaxRadius});";
        }

        private DelegateCommand _GetMaskROI;

        public DelegateCommand GetMaskROI =>
             _GetMaskROI ??= new DelegateCommand(ExecuteGetMaskROI);

        private void ExecuteGetMaskROI()
        {
            if (!Pool.SelectImage.HasValue || Pool.SelectImage.Value.Value.Empty()) return;
            if (Pool.SelectImage.Value.Value.Channels() == 3)
            {
                Gray = Pool.SelectImage.Value.Value.CvtColor(ColorConversionCodes.BGR2GRAY);

                Src = Pool.SelectImage.Value.Value.Clone();
            }
            else
            {
                Gray = Pool.SelectImage.Value.Value.Clone();
                Src = Pool.SelectImage.Value.Value.CvtColor(ColorConversionCodes.GRAY2BGR);
            }

            CircleSegment[] cs = Cv2.HoughCircles(Gray, HoughModes.Gradient, dp: 1, minDist: MinDist, Param1, Param2, MinRadius, MaxRadius);
            Dst = new(Src.Size(), MatType.CV_8UC1, Scalar.Black);
            for (int i = 0; i < cs.Length; i++)
            {
                //画圆
                Cv2.Circle(Dst, (int)cs[i].Center.X, (int)cs[i].Center.Y, (int)cs[i].Radius, Scalar.White, -1, LineTypes.AntiAlias);
            }
            Cv2.CopyTo(Src, Dst, Dst);
            ImgDst = WriteableBitmapConverter.ToWriteableBitmap(Dst);
        }

        private DelegateCommand _CutMaskROI;

        public DelegateCommand CutMaskROI =>
             _CutMaskROI ??= new DelegateCommand(ExecuteCutMaskROI);

        private void ExecuteCutMaskROI()
        {
            if (!Pool.SelectImage.HasValue || Pool.SelectImage.Value.Value.Empty()) return;
            if (Pool.SelectImage.Value.Value.Channels() == 3)
            {
                Gray = Pool.SelectImage.Value.Value.CvtColor(ColorConversionCodes.BGR2GRAY);

                Src = Pool.SelectImage.Value.Value.Clone();
            }
            else
            {
                Gray = Pool.SelectImage.Value.Value.Clone();
                Src = Pool.SelectImage.Value.Value.CvtColor(ColorConversionCodes.GRAY2BGR);
            }

            CircleSegment[] cs = Cv2.HoughCircles(Gray, HoughModes.Gradient, dp: 1, minDist: MinDist, Param1, Param2, MinRadius, MaxRadius);
            Dst = new(Src.Size(), MatType.CV_8UC1, Scalar.Black);
 
            for (int i = 0; i < cs.Length; i++)
            {
                //画圆
                Cv2.Circle(Dst, (int)cs[i].Center.X, (int)cs[i].Center.Y, (int)cs[i].Radius, Scalar.White, -1, LineTypes.AntiAlias);
            }
            Cv2.FindContours(Dst, out Point[][] contours, out _, RetrievalModes.External, ContourApproximationModes.ApproxSimple);
            if (contours.Length < 1) return;
            Rect rect = Cv2.BoundingRect(contours[0]);
            Cv2.CopyTo(Src, Dst, Dst);
            Dst = Dst[rect];

            ImgDst = WriteableBitmapConverter.ToWriteableBitmap(Dst);
        }


        #endregion Command
    }
}