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
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace OpencvsharpModule.ViewModels
{
    public class CannyViewModel : RegionViewModelBase
    {
        public ImagePool Pool { get; set; }
        private string _ViewName;

        public string ViewName
        {
            get { return _ViewName; }
            set { SetProperty(ref _ViewName, value); }
        }

        public CannyViewModel(IContainerExtension container, IRegionManager regionManager) : base(regionManager)
        {
            PoolData = container.Resolve<DataPool>();
            Pool = container.Resolve<ImagePool>();
            ViewName = this.GetType().Name;
            _ = regionManager.RegisterViewWithRegion(ViewName, typeof(CommonView));

            contourApproximationModeList.Add("ApproxNone", ContourApproximationModes.ApproxNone);
            contourApproximationModeList.Add("ApproxSimple", ContourApproximationModes.ApproxSimple);
            contourApproximationModeList.Add("ApproxTC89KCOS", ContourApproximationModes.ApproxTC89KCOS);
            contourApproximationModeList.Add("ApproxTC89L1", ContourApproximationModes.ApproxTC89L1);
            ContourApproximationModeThis = ContourApproximationModes.ApproxSimple;
            RetrievalModeList.Add("CComp", RetrievalModes.CComp);
            RetrievalModeList.Add("External", RetrievalModes.External);
            //  RetrievalModeList.Add("FloodFill", RetrievalModes.FloodFill);
            RetrievalModeList.Add("List", RetrievalModes.List);
            RetrievalModeList.Add("Tree", RetrievalModes.Tree);
            RetrievalModeThis = RetrievalModes.External;
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
            MatName ??= "Canny" + add;
            while (Pool.Images.ContainsKey(MatName))
            {
                MatName = "Canny" + add++;
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

        private Mat Src = new();
        private Mat Dst;
        private Mat Gray;
        private long _CT;
        private System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

        public long CT
        {
            get { return _CT; }
            set { SetProperty(ref _CT, value); }
        }

        #region Cannny

        private double _Threshold1 = 50;

        public double Threshold1
        {
            get { return _Threshold1; }
            set { SetProperty(ref _Threshold1, value); ExecuteGoCanny(); }
        }

        private double _Threshold2 = 150;

        public double Threshold2
        {
            get { return _Threshold2; }
            set { SetProperty(ref _Threshold2, value); ExecuteGoCanny(); }
        }

        private bool Running;
        private bool NewValue;

        private DelegateCommand _GoCanny;

        public DelegateCommand GoCanny =>
             _GoCanny ??= new DelegateCommand(ExecuteGoCanny);

        async private void ExecuteGoCanny()
        {
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
                    sw.Restart();

                    // 大于maxVal的都被检测为边缘，
                    // 而低于minval的都被检测为非边缘。
                    // 对于中间的像素点，如果与确定为边缘的像素点邻接，则判定为边缘；否则为非边缘。
                    Dst = new();
                    await Task.Run(() => Cv2.Canny(Gray, Dst, Threshold1, Threshold2));
                    sw.Stop();
                    CT = sw.ElapsedMilliseconds;
                    ImgDst = WriteableBitmapConverter.ToWriteableBitmap(Dst);
                    CommandText = $"Cv2.Canny(Src, Dst,{Threshold1:F0},{Threshold2:F0});";
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

            if (Pool.SelectImage.Value.Value is null || Pool.SelectImage.Value.Value.Empty()) return false;

            if (Src != Pool.SelectImage.Value.Value)
            {
                Src = Pool.SelectImage.Value.Value;
                Pool.SelectImage.Value.Value.GetGray(out Gray);
            }
            return true;
        }

        #endregion Cannny

        #region FindContours

        private bool NewRange;
        private ObservableDictionary<string, ContourApproximationModes> contourApproximationModeList = new ObservableDictionary<string, ContourApproximationModes>();

        public ObservableDictionary<string, ContourApproximationModes> ContourApproximationModeList
        {
            get { return contourApproximationModeList; }
            set { SetProperty(ref contourApproximationModeList, value); }
        }

        private ContourApproximationModes contourApproximationModeThis;

        public ContourApproximationModes ContourApproximationModeThis
        {
            get { return contourApproximationModeThis; }
            set { SetProperty(ref contourApproximationModeThis, value); NewValue = true; }
        }

        private ObservableDictionary<string, RetrievalModes> retrievalModeList = new ObservableDictionary<string, RetrievalModes>();

        public ObservableDictionary<string, RetrievalModes> RetrievalModeList
        {
            get { return retrievalModeList; }
            set { SetProperty(ref retrievalModeList, value); }
        }

        private RetrievalModes retrievalModeThis;

        public RetrievalModes RetrievalModeThis
        {
            get { return retrievalModeThis; }
            set { SetProperty(ref retrievalModeThis, value); NewValue = true; }
        }

        private int coutourCount;

        public int CoutourCount
        {
            get { return coutourCount; }
            set { SetProperty(ref coutourCount, value); }
        }

        private int _LenghtLow = 100;

        public int LenghtLow
        {
            get { return _LenghtLow; }
            set { SetProperty(ref _LenghtLow, value); Filtr = "Length"; NewRange = true; ExecuteGoFindContours(); }
        }

        private int _LenghtHigh = 1000;

        public int LenghtHigh
        {
            get { return _LenghtHigh; }
            set { SetProperty(ref _LenghtHigh, value); Filtr = "Length"; NewRange = true; ExecuteGoFindContours(); }
        }

        private int _LenghtLargest = 1000;

        public int LenghtLargest
        {
            get { return _LenghtLargest; }
            set { SetProperty(ref _LenghtLargest, value); }
        }

        private int _AreaLow = 100;

        public int AreaLow
        {
            get { return _AreaLow; }
            set { SetProperty(ref _AreaLow, value); Filtr = "Area"; NewRange = true; ExecuteGoFindContours(); }
        }

        private int _AreaHigh = 1000;

        public int AreaHigh
        {
            get { return _AreaHigh; }
            set { SetProperty(ref _AreaHigh, value); Filtr = "Area"; NewRange = true; ExecuteGoFindContours(); }
        }

        private int _AreaLargest = 1000;

        public int AreaLargest
        {
            get { return _AreaLargest; }
            set { SetProperty(ref _AreaLargest, value); }
        }

        private Point[][] contours;
        private DelegateCommand _goFindContours;

        public DelegateCommand GoFindContours =>
               _goFindContours ??= new DelegateCommand(ExecuteGoFindContours);

        private bool IsRunning;

        async private void ExecuteGoFindContours()
        {
            if (IsRunning) return;
            IsRunning = true;
            try
            {
                if (!Pool.SelectImage.HasValue) return;
                if (!Pool.SelectImage.Value.Value.GetGrayAndBgr(out Gray, out Dst)) return;
                if (Src is null || Src != Pool.SelectImage.Value.Value || NewValue)
                {
                    NewRange = true;
                    NewValue = false;
                    Src = Pool.SelectImage.Value.Value;
                    Mat binary = new();
                    Cv2.Threshold(Gray, binary, 100, 255, ThresholdTypes.Binary | ThresholdTypes.Otsu);
                    //获得轮廓
                    await Task.Run(() => Cv2.FindContours(binary, out contours, out _, RetrievalModeThis, ContourApproximationModeThis));
                    LenghtLargest = contours.Max(c => c.Length) * 2;
                    AreaLargest = (int)(contours.Max(c => Cv2.ContourArea(c)) * 1.2);
                    CommandText = $" Cv2.FindContours(src_gray, out Point[][] contours, out _, RetrievalModes.{RetrievalModeThis},ContourApproximationMode.{ContourApproximationModeThis})";
                }
                if (contours is null) return;
                ContourList.Clear();

                while (NewRange)
                {
                    NewRange = false;
                    int i = 0;
                    switch (Filtr)
                    {
                        case "Length":
                            foreach (var contour in contours)
                            {
                                if (contour.Length > LenghtLow && contour.Length < LenghtHigh)
                                {
                                    ContourList[$"contour{i}"] = contour;
                                    i++;
                                }
                            }
                            break;

                        case "Area":
                            foreach (var contour in contours)
                            {
                                if (Cv2.ContourArea(contour) > AreaLow && Cv2.ContourArea(contour) < AreaHigh)
                                {
                                    ContourList[$"contour{i}"] = contour;
                                    i++;
                                }
                            }
                            break;

                        default:
                            break;
                    }

                    CoutourCount = ContourList.Count;
                    await Task.Run(() =>
                    {
                        for (int i = 0; i < ContourList.Count; i++)
                        {
                            Cv2.DrawContours(Dst, ContourList.Values, i, Scalar.RandomColor(), 2);
                        }
                    });
                    ImgDst = WriteableBitmapConverter.ToWriteableBitmap(Dst);
                }
            }
            finally
            {
                IsRunning = false;
            }
        }

        private string Filtr = "Length";
        private ObservableDictionary<string, Point[]> _ContourList = new();

        public ObservableDictionary<string, Point[]> ContourList
        {
            get { return _ContourList; }
            set { SetProperty(ref _ContourList, value); }
        }

        private KeyValuePair<string, Point[]>? _selectContour;

        public KeyValuePair<string, Point[]>? SelectContour
        {
            get { return _selectContour; }
            set
            {
                SetProperty(ref _selectContour, value);
                if (value is not null && value.HasValue && value.Value.Value.Length > 0)
                {
                    Mat mat = Src.Clone();
                    List<Point[]> pointsList = new();
                    pointsList.Add(value.Value.Value);
                    Cv2.DrawContours(mat, pointsList, 0, Scalar.RandomColor(), 2);
                    ImgDst = WriteableBitmapConverter.ToWriteableBitmap(mat);
                }
            }
        }

        private DelegateCommand _addContour;

        public DelegateCommand AddContour =>
                _addContour ??= new DelegateCommand(ExecuteAddContour);

        private void ExecuteAddContour()
        {
            if (!SelectContour.HasValue || SelectContour.Value.Value == null) return;
            ContourName ??= "Contour" + add;
            while (PoolData.Contours.ContainsKey(ContourName))
            {
                ContourName = "Contour" + add++;
            }
            PoolData.Contours[ContourName] = SelectContour.Value.Value;
        }

        public DataPool PoolData { get; set; }
        private string _ContourName;

        public string ContourName
        {
            get { return _ContourName; }
            set { SetProperty(ref _ContourName, value); }
        }

        private string _ContoursName;

        public string ContoursName
        {
            get { return _ContoursName; }
            set { SetProperty(ref _ContoursName, value); }
        }

        private DelegateCommand _AddContours;

        public DelegateCommand AddContours =>
            _AddContours ?? (_AddContours = new DelegateCommand(ExecuteAddContours));

        private void ExecuteAddContours()
        {
            if (ContourList.Count < 1) return;
            ContoursName ??= "Contours" + add;
            while (PoolData.ContoursSet.ContainsKey(ContoursName))
            {
                ContoursName = "Contours" + add++;
            }
            PoolData.ContoursSet[ContoursName] = new() { Width = Dst.Width, Height = Dst.Height, Contours = ContourList.Values.ToArray() };
        }

        //AddContours

        #endregion FindContours
    }
}