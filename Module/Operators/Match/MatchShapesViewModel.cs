using ModuleCore.Mvvm;
//using opencvcli;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using OpencvsharpModule.Models;
using Prism.Commands;
using Prism.Ioc;
using Prism.Regions;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Imaging;

namespace OpencvsharpModule.ViewModels
{
    public partial class MatchShapesViewModel : RegionViewModelBase
    {
        public ImagePool PoolImage { get; set; }
        public DataPool PoolData { get; set; }

      //  private readonly GOCW gocw;

        public MatchShapesViewModel(IContainerExtension container, IRegionManager regionManager) : base(regionManager)
        {
            PoolImage = container.Resolve<ImagePool>();
            PoolData = container.Resolve<DataPool>();
            ShapeMatchModeList.Add("I1", ShapeMatchModes.I1);
            ShapeMatchModeList.Add("I2", ShapeMatchModes.I2);
            ShapeMatchModeList.Add("I3", ShapeMatchModes.I3);
            ShapeMatchModesThis = _ShapeMatchModeList.FirstOrDefault();
           // gocw = container.Resolve<opencvcli.GOCW>();
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

        private long _CT;

        public long CT
        {
            get { return _CT; }
            set { SetProperty(ref _CT, value); }
        }

        private WriteableBitmap _imgDst;

        public WriteableBitmap ImgDst
        {
            get { return _imgDst; }
            set { SetProperty(ref _imgDst, value); }
        }

        public Mat Src { get; set; }
        public Mat Mask { get; set; }
        public Mat Dst;

        private DelegateCommand _GoMatchShapes;

        public DelegateCommand GoMatchShapes =>
             _GoMatchShapes ??= new DelegateCommand(ExecuteGoMatchShapes);

        private void ExecuteGoMatchShapes()
        {
            if (!PoolData.SelectContour1.HasValue) return;


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

        private double _Score;

        public double Score
        {
            get { return _Score; }
            set { SetProperty(ref _Score, value); }
        }

        private System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

        private DelegateCommand _GoMatchShapeContext;

        public DelegateCommand GoMatchShapeContext =>
             _GoMatchShapeContext ??= new DelegateCommand(ExecuteGoMatchShapeContext);

        private void ExecuteGoMatchShapeContext()
        {

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
 

        private DelegateCommand _GoMinAreaRect;

        public DelegateCommand GoMinAreaRect =>
             _GoMinAreaRect ??= new DelegateCommand(ExecuteGoMinAreaRect);

        private void ExecuteGoMinAreaRect()
        {
 
 
        }

        private DelegateCommand _GoMinAreaTriangle;

        public DelegateCommand GoMinAreaTriangle =>
             _GoMinAreaTriangle ??= new DelegateCommand(ExecuteGoMinAreaTriangle);

        private void ExecuteGoMinAreaTriangle()
        {
        }

        private DelegateCommand _GoMinAreaCircle;

        public DelegateCommand GoMinAreaCircle =>
             _GoMinAreaCircle ??= new DelegateCommand(ExecuteGoMinAreaCircle);

        private void ExecuteGoMinAreaCircle()
        {
        }
    }
}