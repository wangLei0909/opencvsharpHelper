using ModuleCore.Mvvm;
//using opencvcli;
using OpenCvSharp;
using OpenCvSharp.Features2D;
using OpenCvSharp.WpfExtensions;
using OpenCvSharp.XFeatures2D;
using OpencvsharpModule.Common;
using OpencvsharpModule.Models;
using Prism.Commands;
using Prism.Ioc;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Imaging;

namespace OpencvsharpModule.ViewModels
{
    public partial class FeatureMatchingViewModel : RegionViewModelBase
    {
        public ImagePool Pool { get; set; }

       // private readonly GOCW gocw;
        public DataPool PoolData { get; set; }

        public FeatureMatchingViewModel(IContainerExtension container, IRegionManager regionManager) : base(regionManager)
        {
            PoolData = container.Resolve<DataPool>();
            Pool = container.Resolve<ImagePool>();
           // gocw = container.Resolve<GOCW>();
            //相关性系数
            //这类方法将模版对其均值的相对值与图像对其均值的相关值进行匹配,
            //1表示完美匹配,-1表示糟糕的匹配,0表示没有任何相关性(随机序列).
            TemplateMatchModeList.Add("CCoeff", TemplateMatchModes.CCoeff);
            TemplateMatchModeList.Add("CCoeffNormed", TemplateMatchModes.CCoeffNormed); //Normed 归一化

            //相关
            //这类方法采用模板和图像间的乘法操作,所以较大的数表示匹配程度较高,0表示最坏的匹配效果.
            TemplateMatchModeList.Add("CCorr", TemplateMatchModes.CCorr);
            TemplateMatchModeList.Add("CCorrNormed", TemplateMatchModes.CCorrNormed);

            //平方差
            //最好匹配为0.匹配越差,匹配值越大.
            TemplateMatchModeList.Add("SqDiff", TemplateMatchModes.SqDiff);
            TemplateMatchModeList.Add("SqDiffNormed", TemplateMatchModes.SqDiffNormed);

            TemplateMatchModeThis = TemplateMatchModes.SqDiffNormed;

            //ECC
            MotionTypeList.Add("Translation", MotionTypes.Translation); //平移
            MotionTypeList.Add("Euclidean", MotionTypes.Euclidean);//刚体  平移(Translation)、缩放(Scale)、旋转(Rotation)
            MotionTypeList.Add("Affine", MotionTypes.Affine);  //仿射  平移(Translation)、缩放(Scale)、翻转(Flip)、旋转(Rotation)和剪切(Shear)
            MotionTypeList.Add("Homography", MotionTypes.Homography);//单应

            //
            ShapeMatchModeList.Add("I1", ShapeMatchModes.I1);
            ShapeMatchModeList.Add("I2", ShapeMatchModes.I2);
            ShapeMatchModeList.Add("I3", ShapeMatchModes.I3);
            ShapeMatchModesThis = _ShapeMatchModeList.FirstOrDefault();
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
        public Mat Src2 { get; set; } = new Mat();
        public Mat Dst { get; set; } = new Mat();

        private DelegateCommand<string> _GoMatche;

        public DelegateCommand<string> GoMatche =>
             _GoMatche ??= new DelegateCommand<string>(ExecuteGoMatche);

        private void ExecuteGoMatche(string feature)
        {
            Mat dst1 = null; Mat dst2 = null;
            if (!Pool.SelectImage.HasValue || Pool.SelectImage.Value.Value.Empty()) return;
            dst1 = Pool.SelectImage.Value.Value.Clone();
            if (!Pool.SelectImage2.HasValue || Pool.SelectImage2.Value.Value.Empty()) return;
            dst2 = Pool.SelectImage2.Value.Value.Clone();
            System.Diagnostics.Stopwatch sw = System.Diagnostics.Stopwatch.StartNew();

            Mat transform = null;
            Mat descriptors1 = new();
            Mat descriptors2 = new();
            KeyPoint[] kps1 = null;
            KeyPoint[] kps2 = null;
            switch (feature)
            {
                case "SIFT":
                    SIFT siftSam = SIFT.Create(contrastThreshold: SiftContrast);
                    siftSam.DetectAndCompute(dst1, null, out kps1, descriptors1);
                    siftSam.DetectAndCompute(dst2, null, out kps2, descriptors2);
                    break;

                case "SURF":
                    SURF surfSam = SURF.Create(SurfThreshold);
                    surfSam.DetectAndCompute(dst1, null, out kps1, descriptors1);
                    surfSam.DetectAndCompute(dst2, null, out kps2, descriptors2);

                    break;

                case "AKAZE":
                    AKAZE AKAZESam = AKAZE.Create();
                    AKAZESam.DetectAndCompute(dst1, null, out kps1, descriptors1);
                    AKAZESam.DetectAndCompute(dst2, null, out kps2, descriptors2);
                    break;

                case "BRISK":
                    BRISK briskSam = BRISK.Create(FREAKThresold);
                    briskSam.DetectAndCompute(dst1, null, out kps1, descriptors1);
                    briskSam.DetectAndCompute(dst2, null, out kps2, descriptors2);
                    break;

                case "FREAK":
                    FREAK FREAKSam = FREAK.Create();

                    FastFeatureDetector fast = FastFeatureDetector.Create(FREAKThresold);
                    kps1 = fast.Detect(dst1);
                    kps2 = fast.Detect(dst2);
                    break;

                case "ORB":
                    ORB orgSam = ORB.Create();
                    orgSam.DetectAndCompute(dst1, null, out kps1, descriptors1);
                    orgSam.DetectAndCompute(dst2, null, out kps2, descriptors2);
                    break;

                default:
                    break;
            }
            DMatch[] matches = null;
            if (FlannMatcher)
            {
                BFMatcher bfmatcher = new BFMatcher();

                if (IsEnableKnnMatch)
                {
                    DMatch[][] matchesKnn = bfmatcher.KnnMatch(descriptors1, descriptors2, 2);

                    matches = matchesKnn.Where(mt => mt[0].Distance < 0.7 * mt[1].Distance).Select(mt => mt[0]).ToArray();
                }
                else
                {
                    matches = bfmatcher.Match(descriptors1, descriptors2);
                }
            }
            else
            {
                FlannBasedMatcher flannBasedMatcher = new FlannBasedMatcher();

                if (descriptors1.Type() != MatType.CV_32F && descriptors2.Type() != MatType.CV_32F)
                {
                    descriptors1.ConvertTo(descriptors1, MatType.CV_32F);
                    descriptors2.ConvertTo(descriptors2, MatType.CV_32F);
                }
                if (IsEnableKnnMatch)
                {
                    DMatch[][] matchesKnn2 = flannBasedMatcher.KnnMatch(descriptors1, descriptors2, 2);
                    matches = matchesKnn2.Select(mt => mt[0]).ToArray();
                }
                else
                {
                    matches = flannBasedMatcher.Match(descriptors1, descriptors2);
                }
            }

            if (matches.Length < 4)
            {
                CommandText = "匹配失败";
                return;
            }
            DMatch[] dMatchesFilter = null;
            if (IsEnableMinDis)
            {
                dMatchesFilter = Match_min(matches, DistanceMax).ToArray();
            }
            else
            {
                dMatchesFilter = matches;
            }
            Mat reMat = new();
            if (dMatchesFilter.Length >= 4)
            {
                (List<DMatch>, List<Point2d>, List<Point2d>) goodRansac = Ransac(dMatchesFilter, kps1, kps2);
                Cv2.DrawMatches(dst1, kps1, dst2, kps2, goodRansac.Item1, reMat);
                IEnumerable<Point2f> ptf1 = goodRansac.Item2.Select(p => new Point2f((float)p.X, (float)p.Y));
                IEnumerable<Point2f> ptf2 = goodRansac.Item3.Select(p => new Point2f((float)p.X, (float)p.Y));
                transform = Cv2.GetAffineTransform(ptf2, ptf1);
            }

            Dst = reMat;

            if (Dst.Empty()) return;
            if (transform is not null && !transform.Empty())
            {
                CommandText = Cv2.Format(transform);

                if (MatExtension.RotationMatrixToEulerAngles(transform, out Vec3d euler))
                {
                    CommandText += "\n旋转角度：" + euler.Item2 * 180 / Cv2.PI;
                }

                CommandText += "\n" + euler.ToString();
            }

            sw.Stop();
            CommandText += "\n" + feature + "匹配耗时：" + sw.ElapsedMilliseconds + " ms";
            ImgDst = WriteableBitmapConverter.ToWriteableBitmap(Dst);
        }
    }

    partial class FeatureMatchingViewModel
    {
        private int _SurfThreshold = 500;

        public int SurfThreshold
        {
            get { return _SurfThreshold; }
            set { SetProperty(ref _SurfThreshold, value); }
        }

        private bool _IsEnableKnnMatch;

        public bool IsEnableKnnMatch
        {
            get { return _IsEnableKnnMatch; }
            set { SetProperty(ref _IsEnableKnnMatch, value); }
        }

        private bool _IsEnableMinDis;

        public bool IsEnableMinDis
        {
            get { return _IsEnableMinDis; }
            set { SetProperty(ref _IsEnableMinDis, value); }
        }

        private bool _FlannMatcher;

        public bool FlannMatcher
        {
            get { return _FlannMatcher; }
            set { SetProperty(ref _FlannMatcher, value); }
        }

        private double _SiftContrast = 0.04;

        public double SiftContrast
        {
            get { return _SiftContrast; }
            set { SetProperty(ref _SiftContrast, value); }
        }

        private int _FREAKThresold = 70;

        public int FREAKThresold
        {
            get { return _FREAKThresold; }
            set { SetProperty(ref _FREAKThresold, value); }
        }

        //private Mat MatchBEBLID(Mat matFrom, Mat matTo, out Mat transform)
        //{
        //    transform = new(3, 3, MatType.CV_64FC1);
        //    return gocw.BEBLIDMatcher(matFrom, matTo, transform);
        //}

        private float _DistanceMax = 0.4f;

        public float DistanceMax
        {
            get { return _DistanceMax; }
            set { SetProperty(ref _DistanceMax, value); }
        }

        // https://gitee.com/lolo77/OpenCVVision

        /// <summary>
        /// 通过最小距离阈值来过滤部分匹配
        /// </summary>
        /// <param name="matches"></param>
        /// <returns></returns>
        private static List<DMatch> Match_min(DMatch[] matches, float distanceMax)
        {
            var minDist = matches.Min(x => x.Distance); ;
            return matches.Where(x => x.Distance <= Math.Max(2 * minDist, distanceMax)).ToList();
        }

        /// <summary>
        /// 随机抽取4个点计算单应性矩阵并重投影，比较坐标，记录正确点数量；多次重复，将正确点数量最多的当做正确匹配
        /// </summary>
        /// <param name="dMatches"></param>
        /// <param name="queryKeyPoints"></param>
        /// <param name="trainKeyPoint"></param>
        /// <returns></returns>
        private static (List<DMatch>, List<Point2d>, List<Point2d>) Ransac(DMatch[] dMatches, KeyPoint[] queryKeyPoints, KeyPoint[] trainKeyPoint)
        {
            List<DMatch> reList = new();
            List<Point2d> src1Pts = new();
            List<Point2d> dst1Pts = new();
            List<Point2d> srcPoints = new();
            List<Point2d> dstPoints = new();

            for (int i = 0; i < dMatches.Length; i++)
            {
                srcPoints.Add(new Point2d(queryKeyPoints[dMatches[i].QueryIdx].Pt.X, queryKeyPoints[dMatches[i].QueryIdx].Pt.Y));
                dstPoints.Add(new Point2d(trainKeyPoint[dMatches[i].TrainIdx].Pt.X, trainKeyPoint[dMatches[i].TrainIdx].Pt.Y));
            }
            Mat inliersMask = new Mat();
            _ = Cv2.FindHomography(srcPoints, dstPoints, HomographyMethods.Ransac, 5, inliersMask);
            _ = inliersMask.GetArray(out byte[] inliersArray);
            for (int i = 0; i < inliersArray.Length; i++)
            {
                if (inliersArray[i] != 0)
                {
                    reList.Add(dMatches[i]);
                    src1Pts.Add(srcPoints[i]);
                    dst1Pts.Add(dstPoints[i]);
                }
            }
            return (reList, src1Pts, dst1Pts);
        }

        #region MatchTemplate

        private ObservableDictionary<string, MotionTypes> _MotionTypeList = new();

        public ObservableDictionary<string, MotionTypes> MotionTypeList
        {
            get { return _MotionTypeList; }
            set { SetProperty(ref _MotionTypeList, value); }
        }

        private ObservableDictionary<string, TemplateMatchModes> _templateMatchModeList = new ObservableDictionary<string, TemplateMatchModes>();

        public ObservableDictionary<string, TemplateMatchModes> TemplateMatchModeList
        {
            get { return _templateMatchModeList; }
            set { SetProperty(ref _templateMatchModeList, value); }
        }

        private TemplateMatchModes _templateMatchModeThis;

        public TemplateMatchModes TemplateMatchModeThis
        {
            get { return _templateMatchModeThis; }
            set { SetProperty(ref _templateMatchModeThis, value); }
        }

        public Mat Template;
        public Mat Target;
        private DelegateCommand<string> _GoTemplateMatche;

        public DelegateCommand<string> GoTemplateMatche =>
             _GoTemplateMatche ??= new DelegateCommand<string>(ExecuteGoTemplateMatche);

        private void ExecuteGoTemplateMatche(string feature)
        {
            if (!Pool.SelectImage.HasValue || Pool.SelectImage.Value.Value.Empty()) return;
            Target = Pool.SelectImage.Value.Value;
            if (!Pool.SelectImage2.HasValue || Pool.SelectImage2.Value.Value.Empty()) return;
            Template = Pool.SelectImage2.Value.Value;
            Dst = Target.Clone();
            if (Template.Width > Target.Width || Template.Height > Target.Height)
            {
                CommandText = "模板大于目标，无法匹配！";
                return;
            }
            if (Template.Channels() != Target.Channels())
            {
                CommandText = "两图象通道数不同，无法匹配！";
                return;
            }

            Mat totals = new();
            Cv2.MatchTemplate(Target, Template, totals, TemplateMatchModeThis);
            Cv2.MinMaxLoc(totals, out double minVal, out double maxVal, out Point minLocation, out Point maxLocation);
            //画出匹配的矩，
            if (TemplateMatchModeThis == TemplateMatchModes.SqDiff || TemplateMatchModeThis == TemplateMatchModes.SqDiffNormed)
            {
                Cv2.Rectangle(Dst, minLocation, new Point(minLocation.X + Template.Cols, minLocation.Y + Template.Rows), Scalar.RandomColor(), 2);

                MatchingTotal = minVal;
            }
            else
            {
                Cv2.Rectangle(Dst, maxLocation, new Point(maxLocation.X + Template.Cols, maxLocation.Y + Template.Rows), Scalar.RandomColor(), 2);
                MatchingTotal = maxVal;
            }

            ImgDst = WriteableBitmapConverter.ToWriteableBitmap(Dst);
            CommandText = $"Cv2.MatchTemplate(Target, Template, totals, TemplateMatchModes.{TemplateMatchModeThis})";
        }

        private double matchingTotal;

        public double MatchingTotal
        {
            get { return matchingTotal; }
            set { SetProperty(ref matchingTotal, value); }
        }

        #endregion MatchTemplate

        #region MatcheShapes

        private DelegateCommand _GoShapesMatche;

        public DelegateCommand GoShapesMatche =>
             _GoShapesMatche ??= new DelegateCommand(ExecuteGoShapesMatche);

        private void ExecuteGoShapesMatche()
        {
            if (!PoolData.SelectContour1.HasValue) return;
            if (!PoolData.SelectContours.HasValue) return;

            var coutour = PoolData.SelectContour1.Value.Value;

            var w = PoolData.SelectContours.Value.Value.Width;
            var h = PoolData.SelectContours.Value.Value.Height;

            Mat mat = new(h, w, MatType.CV_8UC1, Scalar.Black);

            MaxScore = 0;
            commandText = "";
            Dst = mat.CvtColor(ColorConversionCodes.GRAY2BGR);
            Cv2.DrawContours(Dst, PoolData.SelectContours.Value.Value.Contours, -1, Scalar.White);
            var scaleHigh = coutour.Length * LenghtHigh / 100;
            var scaleLow = coutour.Length * LenghtLow / 100;

            for (int i = 0; i < PoolData.SelectContours.Value.Value.Contours.Length; i++)
            {
                var score = Cv2.MatchShapes(PoolData.SelectContour1.Value.Value, PoolData.SelectContours.Value.Value.Contours[i], ShapeMatchModesThis.Value);

                MaxScore = score > MaxScore ? score : MaxScore;

                if (score >= MinScore
                    && PoolData.SelectContours.Value.Value.Contours[i].Length > scaleLow
                    && PoolData.SelectContours.Value.Value.Contours[i].Length < scaleHigh)
                {
                    Cv2.DrawContours(Dst, PoolData.SelectContours.Value.Value.Contours, i, Scalar.Red);

                    Cv2.PutText(Dst, score.ToString("F3"), PoolData.SelectContours.Value.Value.Contours[i][0], HersheyFonts.HersheyDuplex, 0.5, Scalar.Yellow);
                    Moments M = Cv2.Moments(PoolData.SelectContours.Value.Value.Contours[i], true);
                    double cX = M.M10 / M.M00;
                    double cY = M.M01 / M.M00;  //目标物的质心
                    float a = (float)(M.M20 / M.M00 - cX * cX);
                    float b = (float)(2 * (M.M11 / M.M00 - cX * cY));
                    float c = (float)(M.M02 / M.M00 - cY * cY);
                    double tanAngle = Cv2.FastAtan2(2 * b, a - c) / 2;

                    if (GetRotatedRect) { 
                        RotatedRect rrect = Cv2.MinAreaRect(PoolData.SelectContours.Value.Value.Contours[i]);
                    for (int j = 0; j < 4; j++)
                    {
                        Cv2.Line(Dst, (int)rrect.Points()[j].X, (int)rrect.Points()[j].Y, (int)rrect.Points()[(j + 1) % 4].X, (int)rrect.Points()[(j + 1) % 4].Y, Scalar.Lime);
                    }
                    }
                    var r = Math.Sqrt(Cv2.ContourArea(PoolData.SelectContours.Value.Value.Contours[i]) / Cv2.PI);

                    var x = r * Math.Cos(tanAngle * Cv2.PI / 180);
                    var y = r * Math.Sin(tanAngle * Cv2.PI / 180);
                    var endpoint = new Point(cX + x, cY + y);
                    Cv2.ArrowedLine(Dst, new Point(cX, cY), endpoint, Scalar.Green);
                }
            }

            ImgDst = WriteableBitmapConverter.ToWriteableBitmap(Dst);
        }

        private bool _GetRotatedRect;
        public bool GetRotatedRect
        {
            get { return _GetRotatedRect; }
            set { SetProperty(ref _GetRotatedRect, value); }
        }
        private double _MinScore = 0.8d;

        public double MinScore
        {
            get { return _MinScore; }
            set { SetProperty(ref _MinScore, value); }
        }

        private double _MaxScore;

        public double MaxScore
        {
            get { return _MaxScore; }
            set { SetProperty(ref _MaxScore, value); }
        }

        private ObservableDictionary<string, ShapeMatchModes> _ShapeMatchModeList = new();

        public ObservableDictionary<string, ShapeMatchModes> ShapeMatchModeList
        {
            get { return _ShapeMatchModeList; }
            set { SetProperty(ref _ShapeMatchModeList, value); }
        }

        private KeyValuePair<string, ShapeMatchModes> _ShapeMatchModesThis;

        public KeyValuePair<string, ShapeMatchModes> ShapeMatchModesThis
        {
            get { return _ShapeMatchModesThis; }
            set { SetProperty(ref _ShapeMatchModesThis, value); }
        }

        private double _LenghtHigh = 200;

        public double LenghtHigh
        {
            get { return _LenghtHigh; }
            set { SetProperty(ref _LenghtHigh, value); }
        }

        private double _LenghtLow = 80;

        public double LenghtLow
        {
            get { return _LenghtLow; }
            set { SetProperty(ref _LenghtLow, value); }
        }
        private DelegateCommand _GoInCircle;

        public DelegateCommand GoInCircle =>
             _GoInCircle ??= new DelegateCommand(ExecuteGoInCircle);

        private void ExecuteGoInCircle()
        {
            if (!PoolData.SelectContour1.HasValue) return;

            var coutour = PoolData.SelectContour1.Value.Value;
            var left = coutour.Min(p => p.X);
            var top = coutour.Min(P => P.Y);
            var w = coutour.Max(p => p.X);
            var h = coutour.Max(p => p.Y);

            Mat mat = new(h, w, MatType.CV_8UC1, Scalar.Black);

            List<Point[]> pointsList = new() { coutour };

            Cv2.DrawContours(mat, pointsList, 0, Scalar.White, -1);

            if (!mat.FindNonZero().GetArray(out Point[] points)) return;

            double maxdist = 0d, dist;
            Point center = new Point();
            for (int i = 0; i < points.Length; i++)
            {
                dist = Cv2.PointPolygonTest(coutour, points[i], true);
                if (dist > maxdist)
                {
                    maxdist = dist;
                    center = points[i];
                }
            }
            mat = mat.CvtColor(ColorConversionCodes.GRAY2BGR);
            Cv2.Circle(mat, center, (int)maxdist, Scalar.Red);
            Dst = mat[top, h, left, w].Clone();
            ImgDst = WriteableBitmapConverter.ToWriteableBitmap(Dst);
        }

        private DelegateCommand _GoGetCenter;

        public DelegateCommand GoGetCenter =>
             _GoGetCenter ??= new DelegateCommand(ExecuteGoGetCenter);

        private void ExecuteGoGetCenter()
        {
            if (!PoolData.SelectContour1.HasValue) return;

            var coutour = PoolData.SelectContour1.Value.Value;
            var left = coutour.Min(p => p.X);
            var top = coutour.Min(P => P.Y);
            var w = coutour.Max(p => p.X);
            var h = coutour.Max(p => p.Y);

            Mat mat = new(h, w, MatType.CV_8UC3, Scalar.Black);

            List<Point[]> pointsList = new() { coutour };

            Cv2.DrawContours(mat, pointsList, 0, Scalar.White, -1);

            /// 计算矩
            Moments mu = Cv2.Moments(coutour, true);
            ///  计算中心矩:
            Point2d mc = new Point2d(mu.M10 / mu.M00, mu.M01 / mu.M00);

            //画结果
            Cv2.Circle(mat, (Point)mc, 1, Scalar.Red);
            Dst = mat[top, h, left, w];
            ImgDst = WriteableBitmapConverter.ToWriteableBitmap(Dst);
        }
        #endregion MatcheShapes
    }
}