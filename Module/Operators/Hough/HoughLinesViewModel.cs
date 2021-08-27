using ModuleCore.Mvvm;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using OpenCvSharp.XImgProc;
using OpencvsharpModule.Common;
using OpencvsharpModule.Models;
using OpencvsharpModule.Views;
using Prism.Commands;
using Prism.Ioc;
using Prism.Regions;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

//当使用了 OTSU和 TRIANGLE两个标志时，输入图像必须为单通道。

namespace OpencvsharpModule.ViewModels
{
    public class HoughLinesViewModel : RegionViewModelBase
    {
        public ImagePool Pool { get; set; }

        public HoughLinesViewModel(IContainerExtension container, IRegionManager regionManager) : base(regionManager)
        {
            Pool = container.Resolve<ImagePool>();
            ViewName = this.GetType().Name;
            regionManager.RegisterViewWithRegion(ViewName, typeof(CommonView));
        }

        private string _ViewName;

        public string ViewName
        {
            get { return _ViewName; }
            set { SetProperty(ref _ViewName, value); }
        }

        private long _CT;

        public long CT
        {
            get { return _CT; }
            set { SetProperty(ref _CT, value); }
        }

        private readonly System.Diagnostics.Stopwatch sw = new();

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

        private Mat Src;
        private Mat Dst;
        private Mat Gray;

        private DelegateCommand _addMat;

        public DelegateCommand AddMat =>
                _addMat ??= new DelegateCommand(ExecuteAddMat);

        private int add;

        private void ExecuteAddMat()
        {
            if (Dst == null || Dst.Empty()) return;
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

        private bool NewValue;

        #region HoughLines

        private int _ThresholdP = 100;

        public int ThresholdP
        {
            get { return _ThresholdP; }
            set { SetProperty(ref _ThresholdP, value); GoHoughLinesP(); }
        }

        private double _MinLinLenght = 100;

        public double MinLinLenght
        {
            get { return _MinLinLenght; }
            set { SetProperty(ref _MinLinLenght, value); GoHoughLinesP(); }
        }

        private double _MaxLineGap = 10;

        public double MaxLineGap
        {
            get { return _MaxLineGap; }
            set { SetProperty(ref _MaxLineGap, value); GoHoughLinesP(); }
        }

        private void GoHoughLinesP()
        {
            NewValue = true;
            if (IsWorking) return;
            try
            {
                IsWorking = true;

                while (NewValue)
                {
                    NewValue = false;
                    if (!Pool.SelectImage.HasValue) return;
                    if (!Pool.SelectImage.Value.Value.GetGrayAndBgr(out Gray, out Dst)) return;

                    // 1:边缘检测
                    Mat canny = new Mat(Gray.Size(), Gray.Type());
                    Cv2.Canny(Gray, canny, 100, 200, 3, false);

                    Cv2.Line(Dst, new(10, 10), new(10 + MinLinLenght, 10), Scalar.Red, 3);
                    /*
                    *  HoughLinesP:使用概率霍夫变换查找二进制图像中的线段。
                    *  参数：
                    *      1； image: 输入图像 （只能输入单通道图像）
                    *      2； rho:   累加器的距离分辨率(以像素为单位) 生成极坐标时候的像素扫描步长
                    *      3； theta: 累加器的角度分辨率(以弧度为单位)生成极坐标时候的角度步长，一般取值CV_PI/180 ==1度
                    *      4； threshold: 累加器阈值参数。只有那些足够的行才会返回 投票(>阈值)；设置认为几个像素连载一起才能被看做是直线。
                    *      5； minLineLength: 最小线长度，设置最小线段是有几个像素组成。
                    *      6； maxLineGap: 同一条线上的点之间连接它们的最大允许间隙。(默认情况下是0）：设置你认为像素之间间隔多少个间隙也能认为是直线
                    *      返回结果:
                    *      输出线。每条线由一个4元向量(x1, y1, x2，y2)
                    */
                    sw.Restart();
                    LineSegmentPoint[] linePionts = Cv2.HoughLinesP(canny, 1, Cv2.PI / 180, ThresholdP, MinLinLenght, MaxLineGap);//只能输入单通道图像
                    sw.Stop();
                    CT = sw.ElapsedMilliseconds;
                    for (int i = 0; i < linePionts.Length; i++)
                    {
                        Cv2.Line(Dst, linePionts[i].P1, linePionts[i].P2, Scalar.RandomColor(), 2);
                    }

                    //画交点 、角度
                    if (linePionts.Length > 1)
                    {
                        Line2D line1 = MatExtension.LineSegmentPoint2Line2D(linePionts[0]);
                        Line2D line2 = MatExtension.LineSegmentPoint2Line2D(linePionts[1]);

                        MatExtension.IntersectionPoint(line1, line2, out Point2d intersectionPoint);
                        Cv2.Circle(Dst, (int)intersectionPoint.X, (int)intersectionPoint.Y, 3, Scalar.Red, -1, LineTypes.AntiAlias);

                        var p1 = intersectionPoint.DistanceTo(linePionts[0].P1) > intersectionPoint.DistanceTo(linePionts[0].P2) ? linePionts[0].P1 : linePionts[0].P2;

                        var p2 = intersectionPoint.DistanceTo(linePionts[1].P1) > intersectionPoint.DistanceTo(linePionts[1].P2) ? linePionts[1].P1 : linePionts[1].P2;

                        Dst.DrawAngle(intersectionPoint, p1, p2, 20);
                    }

                    CommandText = $" Cv2.HoughLinesP(canny, 1, 0.01,{ThresholdP}, {MinLinLenght:F2}, {MaxLineGap:F2});";

                    ImgDst = WriteableBitmapConverter.ToWriteableBitmap(Dst);
                }
            }
            finally
            {
                IsWorking = false;
            }
        }

        #endregion HoughLines

        #region 快速直线检测

        private int _FastMinLenght = 30;

        public int FastMinLenght
        {
            get { return _FastMinLenght; }
            set
            {
                SetProperty(ref _FastMinLenght, value);
                ExecuteGoFastLineDetectors();
            }
        }

        private float _FastMaxDistance = 1.14f;

        public float FastMaxDistance
        {
            get { return _FastMaxDistance; }
            set
            {
                SetProperty(ref _FastMaxDistance, value);
                ExecuteGoFastLineDetectors();
            }
        }

        private DelegateCommand _GoFastLineDetectors;

        public DelegateCommand GoFastLineDetectors =>
             _GoFastLineDetectors ??= new DelegateCommand(ExecuteGoFastLineDetectors);

        private void ExecuteGoFastLineDetectors()
        {
            if (!Pool.SelectImage.HasValue) return;

            if (!Pool.SelectImage.Value.Value.GetGrayAndBgr(out Gray, out Dst)) return;
            sw.Restart();

            FastLineDetector fastLineDetector = FastLineDetector.Create(lengthThreshold: FastMinLenght, distanceThreshold: FastMaxDistance);
            var lines = fastLineDetector.Detect(Gray);

            sw.Stop();
            CT = sw.ElapsedMilliseconds;
            fastLineDetector.DrawSegments(Dst, lines);

            if (lines.Length > 1)
            {
                Line2D line1 = new(lines[0].Item0, lines[0].Item1, lines[0].Item2, lines[0].Item3);
                Line2D line2 = new(lines[1].Item0, lines[1].Item1, lines[1].Item2, lines[1].Item3);

                MatExtension.IntersectionPoint(line1, line2, out Point2d intersectionPoint);
                Cv2.Circle(Dst, (int)intersectionPoint.X, (int)intersectionPoint.Y, 3, Scalar.Red, -1, LineTypes.AntiAlias);

                Point p1 = intersectionPoint.DistanceTo(new(line1.Vx, line1.Vy)) > intersectionPoint.DistanceTo(new(line1.X1, line1.Y1)) ? new(line1.Vx, line1.Vy) : new(line1.X1, line1.Y1);
                Point p2 = intersectionPoint.DistanceTo(new(line2.Vx, line2.Vy)) > intersectionPoint.DistanceTo(new(line2.X1, line2.Y1)) ? new(line2.Vx, line2.Vy) : new(line2.X1, line2.Y1);

                Dst.DrawAngle(intersectionPoint, p1, p2, 20);
            }
            ImgDst = WriteableBitmapConverter.ToWriteableBitmap(Dst);
        }

        #endregion 快速直线检测

        #region HoughLines

        private int _Threshold = 100;

        public int Threshold
        {
            get { return _Threshold; }
            set { SetProperty(ref _Threshold, value); GoHoughLines(); }
        }

        private void GoHoughLines()
        {
            NewValue = true;
            if (IsWorking) return;
            try
            {
                IsWorking = true;

                while (NewValue)
                {
                    NewValue = false;
                    if (!Pool.SelectImage.HasValue) return;
                    if (!Pool.SelectImage.Value.Value.GetGrayAndBgr(out Gray, out Dst)) return;

                    Cv2.Threshold(Gray, Gray, 0, 255, ThresholdTypes.Binary | ThresholdTypes.Otsu);

                    sw.Restart();
                    var lines = Cv2.HoughLines(Gray, 1, Cv2.PI / 180, ThresholdP);//只能输入单通道图像
                    sw.Stop();
                    CT = sw.ElapsedMilliseconds;

                    var drawlines = lines.Take(10).ToArray();
                    for (int l = 0; l < drawlines.Length; l++)
                    {
                        float rho = drawlines[l].Rho, theta = drawlines[l].Theta;
                        Point pt1, pt2;
                        double a = Math.Cos(theta), b = Math.Sin(theta);
                        double x0 = a * rho, y0 = b * rho;
                        pt1.X = (int)Math.Round(x0 + 1000 * (-b));
                        pt1.Y = (int)Math.Round(y0 + 1000 * (a));
                        pt2.X = (int)Math.Round(x0 - 1000 * (-b));
                        pt2.Y = (int)Math.Round(y0 - 1000 * (a));
                        Cv2.Line(Dst, pt1.X, pt1.Y, pt2.X, pt2.Y, Scalar.Blue);
                    }

                    CommandText = $" Cv2.HoughLines(canny, 1, 0.01,{Threshold});";

                    ImgDst = WriteableBitmapConverter.ToWriteableBitmap(Dst);
                }
            }
            finally
            {
                IsWorking = false;
            }
        }

        #endregion HoughLines

        #region HoughCircles

        //原文链接：https://blog.csdn.net/weixin_41049188/article/details/92422241
        async private void GoHoughCircles()
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
                    await Task.Run(() =>
                    {
                        Dst = Src.Clone();
                        CircleSegment[] cs = Cv2.HoughCircles(Gray, HoughModes.Gradient, dp: 1, minDist: MinDist, Param1, Param2, MinRadius, MaxRadius);

                        Cv2.Line(Dst, new(0, 10), new(MaxRadius * 2, 10), Scalar.Red, 3);
                        var startX = (MaxRadius * 2 - MinRadius * 2) / 2;
                        Cv2.Line(Dst, new(startX, 10), new(startX + MinRadius * 2, 10), Scalar.Blue, 3);
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

        #endregion HoughCircles
    }
}