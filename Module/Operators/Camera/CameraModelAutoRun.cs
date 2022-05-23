using ModuleCore.UserControls;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using OpenCvSharp.WpfExtensions;
using OpencvsharpModule.Common;
using Prism.Commands;

using Sdcb.PaddleOCR.KnownModels;
using Sdcb.PaddleOCR;

using System;
using System.Linq;
using System.Net.Http;
using System.Text;
using ZXing;

namespace OpencvsharpModule.Models
{
    public partial class CameraModel
    {

        private void LoadAutoRun()
        {
            AutoRunList.Add("无处理", mat => Dst = mat);


            AutoRunList.Add("旋转较正", mat =>
            {
                if (AutoRunning) return;
                try
                {
                    AutoRunning = true;

                    var angle = mat.GetDFTAngle();

                    Dst = mat.Rotate((float)angle.angle);
                }
                finally
                {
                    AutoRunning = false;
                }
            });
            AutoRunList.Add("傅里叶变换", mat =>
            {
                if (AutoRunning) return;
                try
                {
                    AutoRunning = true;

                    var angle = mat.GetDFTAngle();

                    Dst = angle.DFT;
                }
                finally
                {
                    AutoRunning = false;
                }
            });
            AutoRunList.Add("取反", mat =>
            {
                if (AutoRunning) return;
                try
                {
                    AutoRunning = true;
                    Dst = new();
                    Cv2.BitwiseNot(mat, Dst);
                }
                finally
                {
                    AutoRunning = false;
                }
            });

            AutoRunList.Add("转置", mat =>
            {
                if (AutoRunning) return;
                try
                {
                    AutoRunning = true;
                    Dst = new();
                    Cv2.Transpose(mat, Dst);
                }
                finally
                {
                    AutoRunning = false;
                }
            });
            AutoRunList.Add("左右翻转", mat =>
            {
                if (AutoRunning) return;
                try
                {
                    AutoRunning = true;
                    Dst = new();
                    Cv2.Flip(mat, Dst, FlipMode.Y);
                }
                finally
                {
                    AutoRunning = false;
                }
            });
            AutoRunList.Add("上下翻转", mat =>
            {
                if (AutoRunning) return;
                try
                {
                    AutoRunning = true;
                    Dst = new();
                    Cv2.Flip(mat, Dst, FlipMode.X);
                }
                finally
                {
                    AutoRunning = false;
                }
            });
            AutoRunList.Add("旋转180度", mat =>
            {
                if (AutoRunning) return;
                try
                {
                    AutoRunning = true;
                    Dst = new();
                    Cv2.Flip(mat, Dst, FlipMode.XY);
                }
                finally
                {
                    AutoRunning = false;
                }
            });
            AutoRunList.Add("顺时针旋转90度", mat =>
            {
                if (AutoRunning) return;
                try
                {
                    AutoRunning = true;
                    Dst = new();
                    Cv2.Transpose(mat, Dst);
                    Cv2.Flip(Dst, Dst, FlipMode.Y);
                }
                finally
                {
                    AutoRunning = false;
                }
            });
            AutoRunList.Add("逆时针旋转90度", mat =>
            {
                if (AutoRunning) return;
                try
                {
                    AutoRunning = true;
                    Dst = new();
                    Cv2.Transpose(mat, Dst);
                    Cv2.Flip(Dst, Dst, FlipMode.X);
                }
                finally
                {
                    AutoRunning = false;
                }
            });

            AutoRunList.Add("自定义角度旋转", mat =>
            {
                if (AutoRunning) return;
                try
                {
                    AutoRunning = true;
                    Dst = mat.Rotate(RotateAngle);
                }
                finally
                {
                    AutoRunning = false;
                }
            });
            AutoRunList.Add("各通道最小值", mat =>
            {
                if (AutoRunning) return;
                try
                {
                    Dst = new();
                    if (mat.Channels() == 3)
                    {
                        Cv2.Split(mat, out Mat[] bgr);

                        Cv2.Min(bgr[0], bgr[1], Dst);
                        Cv2.Min(Dst, bgr[2], Dst);
                    }
                    else
                    {
                        Dst = mat.Clone();
                    }
                }
                finally
                {
                    AutoRunning = false;
                }
            });

            AutoRunList.Add("各通道最大值", mat =>
            {
                if (AutoRunning) return;
                try
                {
                    Dst = new();
                    if (mat.Channels() == 3)
                    {
                        Cv2.Split(mat, out Mat[] bgr);

                        Cv2.Max(bgr[0], bgr[1], Dst);
                        Cv2.Max(Dst, bgr[2], Dst);
                    }
                    else
                    {
                        Dst = mat.Clone();
                    }
                }
                finally
                {
                    AutoRunning = false;
                }
            });
            AutoRunList.Add("灰度", mat =>
            {
                if (AutoRunning) return;
                try
                {
                    AutoRunning = true;
                    Dst = new();
                    if (mat.Type() == MatType.CV_8UC1)
                        Dst = mat;
                    if (mat.Type() == MatType.CV_8UC3)
                        Cv2.CvtColor(mat, Dst, ColorConversionCodes.BGR2GRAY);
                }
                finally
                {
                    AutoRunning = false;
                }
            });
            AutoRunList.Add("灰度直方图", mat =>
            {
                if (AutoRunning) return;
                try
                {
                    AutoRunning = true;

                    Mat gray = new();
                    if (mat.Type() == MatType.CV_8UC1)
                        gray = mat;
                    if (mat.Type() == MatType.CV_8UC3)
                        Cv2.CvtColor(mat, gray, ColorConversionCodes.BGR2GRAY);

                    Mat hist = new Mat();
                    //计算0-255的像素各有多少个，
                    Cv2.CalcHist(
                        new Mat[] { gray },
                        channels: new int[] { 0 },
                        mask: null,
                        hist: hist,
                        dims: 1,
                        histSize: new int[] { 256 }, //histSize:使用多少个bin(柱子)，一般为256
                        ranges: new Rangef[] { new Rangef(0, 256) }); //ranges:像素值的范围，一般为[0,256]表示0~255
                    //归一化
                    Cv2.Normalize(hist, hist);
                    var histstr = Cv2.Format(hist);
                    Dst = new Mat(new Size(256, 512), MatType.CV_8UC1, new Scalar(0));

                    for (int i = 0; i < 256; i++)
                    {
                        int len = (int)(hist.Get<float>(i) * 512);
                        if (len != 0)
                        {
                            Cv2.Line(Dst, new(i, 511), new(i, 512 - len), new Scalar(255));
                        }
                    }
                }
                finally
                {
                    AutoRunning = false;
                }
            });
            AutoRunList.Add("灰度直方图均衡化", mat =>
            {
                if (AutoRunning) return;
                try
                {
                    Dst = new();
                    Mat gray = new();
                    if (mat.Type() == MatType.CV_8UC1)
                        gray = mat;
                    if (mat.Type() == MatType.CV_8UC3)
                        Cv2.CvtColor(mat, gray, ColorConversionCodes.BGR2GRAY);

                    Cv2.EqualizeHist(gray, Dst);
                }
                finally
                {
                    AutoRunning = false;
                }
            });

            AutoRunList.Add("限制直方图均衡化", mat =>
            {
                if (AutoRunning) return;
                try
                {
                    Dst = new();
                    Mat gray = new();
                    if (mat.Type() == MatType.CV_8UC1)
                        gray = mat;
                    if (mat.Type() == MatType.CV_8UC3)
                        Cv2.CvtColor(mat, gray, ColorConversionCodes.BGR2GRAY);

                    var clahe = Cv2.CreateCLAHE();
                    clahe.Apply(gray, gray);
                    Cv2.EqualizeHist(gray, Dst);
                }
                finally
                {
                    AutoRunning = false;
                }
            });
            AutoRunList.Add("行灰度投影直方图", src =>
            {
                if (AutoRunning) return;
                try
                {
                    Mat gray = new();
                    if (src.Type() == MatType.CV_8UC1)
                        gray = src;
                    if (src.Type() == MatType.CV_8UC3)
                        Cv2.CvtColor(src, gray, ColorConversionCodes.BGR2GRAY);

                    Mat reduceRow = new();

                    //每一列的平均值
                    Cv2.Reduce(gray, reduceRow, ReduceDimension.Row, ReduceTypes.Avg, -1);

                    Dst = new(512, reduceRow.Width, MatType.CV_8UC1, Scalar.Black);
                    for (int i = 0; i < reduceRow.Width; i++)
                    {
                        int len = reduceRow.Get<byte>(0, i);
                        if (len != 0)
                        {
                            Cv2.Line(Dst, new(i, 511), new(i, 512 - len), Scalar.White);
                        }
                    }
                }
                finally
                {
                    AutoRunning = false;
                }
            });

            AutoRunList.Add("列灰度投影直方图", src =>
            {
                if (AutoRunning) return;
                try
                {
                    Mat gray = new();
                    if (src.Type() == MatType.CV_8UC1)
                        gray = src;
                    if (src.Type() == MatType.CV_8UC3)
                        Cv2.CvtColor(src, gray, ColorConversionCodes.BGR2GRAY);
                    Mat reduceColumn = new();

                    //每一列的平均值
                    Cv2.Reduce(gray, reduceColumn, ReduceDimension.Column, ReduceTypes.Avg, -1);

                    Dst = new(reduceColumn.Height, 512, MatType.CV_8UC1, Scalar.Black);
                    for (int i = 0; i < reduceColumn.Height; i++)
                    {
                        int len = reduceColumn.Get<byte>(i, 0);
                        if (len != 0)
                        {
                            Cv2.Line(Dst, new(512 - len, i), new(511, i), Scalar.White);
                        }
                    }
                }
                finally
                {
                    AutoRunning = false;
                }
            });
            //https://github.com/sdcb/PaddleSharp
            AutoRunList.Add("PaddleSharp", src =>
            {
                if (AutoRunning) return;
                try
                {
                    OCRModel model = KnownOCRModel.PPOcrV2;
                    model.EnsureAll();
                    string root = model.RootDirectory;
                    string key = model.KeyPath;

                    using PaddleOcrAll all = new PaddleOcrAll("Common/ppocr-v2", "Common/ppocr-v2/key.txt")
                    {
                        AllowRotateDetection = true, /* 允许识别有角度的文字 */
                        Enable180Classification = false, /* 允许识别旋转角度大于90度的文字 */
                    };

                    PaddleOcrResult result = all.Run(src);
                    //Console.WriteLine("Detected all texts: \n" + result.Text);
                    Dst = src.Clone();
                    foreach (PaddleOcrResultRegion region in result.Regions)
                    {
                        Dst.DrawRotatedRect(region.Rect, Scalar.Lime);
                        Dst.PutTextZh(region.Text, (Point)region.Rect.Center, FontSize);
                        // Console.WriteLine($"Text: {region.Text}, Score: {region.Score}, RectCenter: {region.Rect.Center}, RectSize:    {region.Rect.Size}, Angle: {region.Rect.Angle}");
                    }
                    
                }
                finally
                {
                    AutoRunning = false;
                }
            });
            AutoRun = AutoRunList.FirstOrDefault();
        }

        private float _RotateAngle = 30;

        public float RotateAngle
        {
            get { return _RotateAngle; }
            set
            {
                SetProperty(ref _RotateAngle, value);
                if (Pool.SelectImage.HasValue && !Pool.SelectImage.Value.Value.Empty())
                {
                    Mat mat = Pool.SelectImage.Value.Value;
                    Dst = mat.Rotate(RotateAngle);
                    ImgDst = WriteableBitmapConverter.ToWriteableBitmap(Dst);
                }
            }
        }

        private float _Gamma = 0.5f;

        public float Gamma
        {
            get { return _Gamma; }
            set
            {
                SetProperty(ref _Gamma, value);
                if (Pool.SelectImage.HasValue && !Pool.SelectImage.Value.Value.Empty())
                {
                    Src = Pool.SelectImage.Value.Value;

                    Mat gammaImage = new();
                    Src.ConvertTo(gammaImage, MatType.CV_64F);
                    gammaImage = gammaImage.Pow(_Gamma);
                    Dst = new();
                    gammaImage.ConvertTo(Dst, MatType.CV_8U);
                    ImgDst = WriteableBitmapConverter.ToWriteableBitmap(Dst);
                }
            }
        }

        private bool AutoRunning;

        private DelegateCommand<RotateRectROI> _DrawRotateRect;

        public DelegateCommand<RotateRectROI> DrawRotateRect =>
             _DrawRotateRect ??= new DelegateCommand<RotateRectROI>(ExecuteDrawRotateRect);

        private void ExecuteDrawRotateRect(RotateRectROI rrr)
        {
            if (!Pool.SelectImage.HasValue || !Pool.SelectImage.Value.Value.GetBgr(out Dst)) return;

            RotatedRect rr = new(
                new Point2f((float)rrr.CenterX, (float)rrr.CenterY),
                new Size2f(rrr.RectWidth, rrr.RectHeight),
                (float)rrr.RectAngle);
            Dst.DrawRotatedRect(rr, Scalar.RandomColor());
            ImgDst = WriteableBitmapConverter.ToWriteableBitmap(Dst);
        }
    }
}