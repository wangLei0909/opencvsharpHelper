using ModuleCore.Mvvm;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using OpencvsharpModule.Common;
using OpencvsharpModule.Models;
using Prism.Commands;
using Prism.Ioc;
using Prism.Regions;
using System;
using System.Diagnostics;
using System.Windows.Media.Imaging;

namespace OpencvsharpModule.ViewModels
{
    public partial class MatchTemplateViewModel : RegionViewModelBase
    {
        public ImagePool Pool { get; set; }

        public MatchTemplateViewModel(IContainerExtension container, IRegionManager regionManager) : base(regionManager)
        {
            Pool = container.Resolve<ImagePool>();
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

        private long _CT;

        public long CT
        {
            get { return _CT; }
            set { SetProperty(ref _CT, value); }
        }

        public Mat Template;
        public Mat Target;
        public Mat Dst;

        private DelegateCommand<string> _GoMatche;

        public DelegateCommand<string> GoMatche =>
             _GoMatche ??= new DelegateCommand<string>(ExecuteGoMatche);

        private void ExecuteGoMatche(string feature)
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

            sw.Restart();
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
            sw.Stop();
            CT = sw.ElapsedMilliseconds;
            ImgDst = WriteableBitmapConverter.ToWriteableBitmap(Dst);
            CommandText = $"Cv2.MatchTemplate(Target, Template, totals, TemplateMatchModes.{TemplateMatchModeThis})";
        }

        private double matchingTotal;

        public double MatchingTotal
        {
            get { return matchingTotal; }
            set { SetProperty(ref matchingTotal, value); }
        }

        private System.Diagnostics.Stopwatch sw = new();

        #region ECC

        private DelegateCommand _GoECCMatche;

        public DelegateCommand GoECCMatche =>
             _GoECCMatche ??= new DelegateCommand(ExecuteGoECCMatche);

        private void ExecuteGoECCMatche()
        {
            if (!Pool.SelectImage.HasValue || Pool.SelectImage.Value.Value.Empty()) return;
            Template = Pool.SelectImage.Value.Value;
            if (!Pool.SelectImage2.HasValue || Pool.SelectImage2.Value.Value.Empty()) return;
            Target = Pool.SelectImage2.Value.Value;
            Dst = Target.Clone();

            Target.GetGray(out Mat targetGray);
            Template.GetGray(out Mat templatetGray);

            Mat warpMatrix = new();
            try
            {
                MatchingTotal = Cv2.FindTransformECC(templatetGray, targetGray, warpMatrix, MotionTypes.Affine);
                CommandText = $"Cv2.FindTransformECC( Template, Target,warpMatrix, MotionTypes.{MotionTypeSelect})";
                CommandText += "\n";
                CommandText += Cv2.Format(warpMatrix);
                var angle = Math.Asin(warpMatrix.Get<double>(0, 1)) * 180 / Math.PI;
                CommandText += "\n旋转角度：" + angle.ToString("F2");
            }
            catch
            {
                CommandText = " 匹配失败";
            }
        }

        private ObservableDictionary<string, MotionTypes> _MotionTypeList = new();

        public ObservableDictionary<string, MotionTypes> MotionTypeList
        {
            get { return _MotionTypeList; }
            set { SetProperty(ref _MotionTypeList, value); }
        }

        private MotionTypes _MotionTypeSelect;

        public MotionTypes MotionTypeSelect
        {
            get { return _MotionTypeSelect; }
            set { SetProperty(ref _MotionTypeSelect, value); }
        }

        #endregion ECC

        #region edge

        private DelegateCommand _CreatTemplate;

        public DelegateCommand CreatTemplate =>
             _CreatTemplate ??= new DelegateCommand(ExecuteCreatTemplate);

        private void ExecuteCreatTemplate()
        {
            try
            {
                if (!Pool.SelectImage2.HasValue) return;
                
                //求每一个非零点与模板中心的方向和距离
                Mat template = Pool.SelectImage2.Value.Value.Clone();

                //灰度
                template.GetGray(out Mat grayImage);

                //边缘
                var canny = new Mat();
                Cv2.Canny(grayImage, canny, 50, 150);

                //非零点
               var nonzeros =  canny.FindNonZero();
                var points = Cv2.Split(nonzeros);
                points[0] -= template.Width/2;
                points[1] -= template.Height/2;

                //距离 方向
                var magnitude = new Mat();
                var angle = new Mat();

                points[0].ConvertTo(points[0], MatType.CV_32FC1);
                points[1].ConvertTo(points[1], MatType.CV_32FC1);
                //转极坐标
                Cv2.CartToPolar(points[0], points[1], magnitude, angle,true);
                Mat polarPoints = new();
                Cv2.Merge(new[] {magnitude, angle }, polarPoints);

                edgeTemplate.PolarPoints = polarPoints;
                edgeTemplate.PointsCount = polarPoints.Cols * polarPoints.Rows;
                edgeTemplate.Width = template.Width;
                edgeTemplate.Height = template.Height;
                Mat dst = template.EmptyClone()*0;

                nonzeros.GetArray(out Point[] ps);

                foreach (var p in ps)
                {
                    Cv2.Circle(dst, p, 0, Scalar.White,-1);
                }

                ImgDst = WriteableBitmapConverter.ToWriteableBitmap(dst);

            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        EdgeTemplate edgeTemplate = new();
        private DelegateCommand _GoEdgeMatche;
        public DelegateCommand GoEdgeMatche =>
             _GoEdgeMatche ??= new DelegateCommand(ExecuteGoEdgeMatche);

        void ExecuteGoEdgeMatche()
        {
            if (edgeTemplate.PointsCount == 0) return;

            if (!Pool.SelectImage.HasValue) return;


            Pool.SelectImage.Value.Value.GetGray(out Mat grayImage);
            sw.Restart();
            var canny = new Mat();
            Cv2.Canny(grayImage, canny, 50, 150);

            var target = new Mat(canny.Height + edgeTemplate.Height, canny.Width + edgeTemplate.Width, MatType.CV_8UC1, Scalar.Black);

            target[edgeTemplate.Height / 2, edgeTemplate.Height / 2 + canny.Height, edgeTemplate.Width / 2, edgeTemplate.Width / 2 + canny.Width] = canny.Clone();

            for (int i = 0; i < canny.Width; i++)
            {
                for (int j = 0; j < canny.Height; j++)
                {
                    var targetpart = target[j, j + edgeTemplate.Height, i, i + edgeTemplate.Width];
                    var nonzeros = targetpart.FindNonZero();
                    if (nonzeros.Width * nonzeros.Height == edgeTemplate.PointsCount)
                    { 
                     
                    } 
                }
            }
            ImgDst = WriteableBitmapConverter.ToWriteableBitmap(canny);
            sw.Stop();
            CT = sw.ElapsedMilliseconds;
        }
        #endregion edge
    }

    public class EdgeTemplate
    {
        public int PointsCount { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public Mat PolarPoints { get; set; }

    }
}