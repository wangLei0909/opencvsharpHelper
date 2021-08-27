using ModuleCore.Mvvm;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using Prism.Ioc;
using Prism.Mvvm;
using System.Collections.Generic;
using System.Linq;

namespace OpencvsharpModule.Models
{
    public class DataPool : BindableBase
    {
        public DataPool(IContainerExtension container)
        {
            Pool = container.Resolve<ImagePool>();
        }

        public ImagePool Pool { get; set; }

        private ObservableDictionary<string, Point[]> _Contours = new();

        public ObservableDictionary<string, Point[]> Contours
        {
            get { return _Contours; }
            set { SetProperty(ref _Contours, value); }
        }

        // 选择的轮廓
        private KeyValuePair<string, Point[]>? _selectContour1;

        public KeyValuePair<string, Point[]>? SelectContour1
        {
            get { return _selectContour1; }
            set
            {
                SetProperty(ref _selectContour1, value);
                if (value is not null && value.HasValue && value.Value.Value.Length > 0)
                {
                    var left = value.Value.Value.Min(p => p.X);
                    var top = value.Value.Value.Min(P => P.Y);
                    var w = value.Value.Value.Max(p => p.X);
                    var h = value.Value.Value.Max(p => p.Y);

                    Mat mat = new(h, w, MatType.CV_8UC1, Scalar.Black);
                    List<Point[]> pointsList = new();
                    pointsList.Add(value.Value.Value);

                    Cv2.DrawContours(mat, pointsList, 0, Scalar.White);

                    Mat dst = mat[top, h, left, w];
                    Pool.ImgSrc = WriteableBitmapConverter.ToWriteableBitmap(dst);
                }
            }
        }

        private ObservableDictionary<string, ContoursMat> _ContoursSet = new();

        public ObservableDictionary<string, ContoursMat> ContoursSet
        {
            get { return _ContoursSet; }
            set { SetProperty(ref _ContoursSet, value); }
        }

        // 选择的轮廓
        private KeyValuePair<string, ContoursMat>? _selectContours;

        public KeyValuePair<string, ContoursMat>? SelectContours
        {
            get { return _selectContours; }
            set
            {
                SetProperty(ref _selectContours, value);
                if (value is not null && value.HasValue && value.Value.Value.Contours.Length > 0)
                {
                    Mat mat = new(value.Value.Value.Height, value.Value.Value.Width, MatType.CV_8UC1, Scalar.Black);

                    Cv2.DrawContours(mat, value.Value.Value.Contours, -1, Scalar.White);

                    Pool.ImgSrc2 = WriteableBitmapConverter.ToWriteableBitmap(mat);
                }
            }
        }
    }

    public class ContoursMat
    {
        public int Width { get; set; }
        public int Height { get; set; }

        public Point[][] Contours { get; set; }
    }
}