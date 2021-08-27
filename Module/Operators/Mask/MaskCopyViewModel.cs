using ModuleCore.Mvvm;
using ModuleCore.UserControls;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using OpencvsharpModule.Common;
using OpencvsharpModule.Models;
using Prism.Commands;
using Prism.Ioc;
using Prism.Regions;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace OpencvsharpModule.ViewModels
{
    public partial class MaskCopyViewModel : RegionViewModelBase
    {
        public ImagePool Pool { get; set; }

        public MaskCopyViewModel(IContainerExtension container, IRegionManager regionManager) : base(regionManager)
        {
            Pool = container.Resolve<ImagePool>();
            SeamlessCloneMethodList.Add("MixedClone", SeamlessCloneMethods.MixedClone);
            SeamlessCloneMethodList.Add("MonochromeTransfer", SeamlessCloneMethods.MonochromeTransfer);
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
            if (Dst == null || Dst.Empty()) return;
            MatName ??= "MaskCopy" + add;
            while (Pool.Images.ContainsKey(MatName))
            {
                MatName = "MaskCopy" + add++;
            }
            Pool.Images[MatName] = Dst.Clone();
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
        public Mat Target { get; set; }
        public Mat Mask { get; set; }
        public Mat Dst;

        private DelegateCommand _GoCopyTo;

        public DelegateCommand GoCopyTo =>
             _GoCopyTo ??= new DelegateCommand(ExecuteGoCopyTo);

        private void ExecuteGoCopyTo()
        {
            if (!Pool.SelectImage.HasValue) return;
            if (!Pool.SelectImage2.HasValue) return;
            if (Pool.SelectImage.Value.Value.Empty()) return;
            if (Pool.SelectImage2.Value.Value.Empty()) return;

            Src = Pool.SelectImage.Value.Value;
            Mask = Pool.SelectImage2.Value.Value.Clone();

            Dst = new(Src.Size(), MatType.CV_8UC1, Scalar.Black);

            if (Mask.Size() != Src.Size())
            {
                Cv2.Resize(Mask, Mask, Src.Size());
            }
            sw.Restart();
            Cv2.CopyTo(Src, Dst, Mask);
            sw.Stop();
            CT = sw.ElapsedMilliseconds;
            ImgDst = WriteableBitmapConverter.ToWriteableBitmap(Dst);
        }

        private DelegateCommand _GoAdditive;

        public DelegateCommand GoAdditive =>
             _GoAdditive ??= new DelegateCommand(ExecuteGoAdditive);

        private void ExecuteGoAdditive()
        {
            if (!Pool.SelectImage.HasValue) return;
            if (!Pool.SelectImage2.HasValue) return;
            if (Pool.SelectImage.Value.Value.Empty()) return;
            if (Pool.SelectImage2.Value.Value.Empty()) return;

            Src = Pool.SelectImage.Value.Value;
            Target = Pool.SelectImage2.Value.Value.Clone();

            if (Target.Size() != Src.Size())
            {
                Cv2.Resize(Target, Target, Src.Size());
            }
            sw.Restart();

            if (Target.Type() != Src.Type()) return;

            Dst = Src + Target;
            sw.Stop();
            CT = sw.ElapsedMilliseconds;
            ImgDst = WriteableBitmapConverter.ToWriteableBitmap(Dst);
        }

        private DelegateCommand _GoSrcMinusTarget;

        public DelegateCommand GoSrcMinusTarget =>
             _GoSrcMinusTarget ??= new DelegateCommand(ExecuteGoSrcMinusTarget);

        private void ExecuteGoSrcMinusTarget()
        {
            if (!Pool.SelectImage.HasValue) return;
            if (!Pool.SelectImage2.HasValue) return;
            if (Pool.SelectImage.Value.Value.Empty()) return;
            if (Pool.SelectImage2.Value.Value.Empty()) return;

            Src = Pool.SelectImage.Value.Value;
            Target = Pool.SelectImage2.Value.Value.Clone();

            if (Target.Size() != Src.Size())
            {
                Cv2.Resize(Target, Target, Src.Size());
            }
            sw.Restart();
            Dst = Src - Target;
            sw.Stop();
            CT = sw.ElapsedMilliseconds;
            ImgDst = WriteableBitmapConverter.ToWriteableBitmap(Dst);
        }

        private DelegateCommand _GoTargetMinusSrc;

        public DelegateCommand GoTargetMinusSrc =>
             _GoTargetMinusSrc ??= new DelegateCommand(ExecuteGoTargetMinusSrc);

        private void ExecuteGoTargetMinusSrc()
        {
            if (!Pool.SelectImage.HasValue) return;
            if (!Pool.SelectImage2.HasValue) return;
            if (Pool.SelectImage.Value.Value.Empty()) return;
            if (Pool.SelectImage2.Value.Value.Empty()) return;

            Src = Pool.SelectImage.Value.Value;
            Target = Pool.SelectImage2.Value.Value.Clone();

            if (Target.Size() != Src.Size())
            {
                Cv2.Resize(Target, Target, Src.Size());
            }
            sw.Restart();
            Dst = Target - Src;
            sw.Stop();
            CT = sw.ElapsedMilliseconds;
            ImgDst = WriteableBitmapConverter.ToWriteableBitmap(Dst);
        }

        private DelegateCommand _GoSrcDiffTarget;

        public DelegateCommand GoSrcDiffTarget =>
             _GoSrcDiffTarget ??= new DelegateCommand(ExecuteGoSrcDiffTarget);

        private void ExecuteGoSrcDiffTarget()
        {
            if (!Pool.SelectImage.HasValue) return;
            if (!Pool.SelectImage2.HasValue) return;
            if (Pool.SelectImage.Value.Value.Empty()) return;
            if (Pool.SelectImage2.Value.Value.Empty()) return;

            Src = Pool.SelectImage.Value.Value;
            Target = Pool.SelectImage2.Value.Value.Clone();

            if (Target.Size() != Src.Size())
            {
                Cv2.Resize(Target, Target, Src.Size());
            }
            sw.Restart();
            Cv2.Absdiff(Src, Target, Dst);
            sw.Stop();
            CT = sw.ElapsedMilliseconds;
            ImgDst = WriteableBitmapConverter.ToWriteableBitmap(Dst);
        }

        private System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

        //        g(x) = (1 - a)f0(x) + af1(x)
        //    对两幅图像 f0（x）和f1（x）  叠化（cross-dissolve）效果。
        //    可以看如下公式：dst = src1[I]*alpha+ src2[I]*beta + gamma;
        //    void addWeighted(InputArray src1,    //    src1:需要混合的第一张图片。
        //    double alpha,                        //    alpha：第一张图片的权重。
        //    InputArray src2,                     //    src2:需要混合的第二张图片。
        //    double beta,                         //    beta:第二张图片的权重。
        //    double gamma,                        //    gamma：一个加到权重总和上的标量值。
        //    OutputArray dst,//    dst：混合后的目标图片。
        //    int dtype = -1);//    dtype:输出阵列的可选深度。
        //

        private DelegateCommand _GoAddWeighted;

        public DelegateCommand GoAddWeighted =>
             _GoAddWeighted ??= new DelegateCommand(ExecuteGoAddWeighted);

        private void ExecuteGoAddWeighted()
        {
            if (Running) return;
            Running = true;
            try
            {
                if (!Pool.SelectImage.HasValue) return;
                if (!Pool.SelectImage2.HasValue) return;
                if (Pool.SelectImage.Value.Value.Empty()) return;
                if (Pool.SelectImage2.Value.Value.Empty()) return;

                Src = Pool.SelectImage.Value.Value;
                Target = Pool.SelectImage2.Value.Value.Clone();
                Dst = new();
                if (Target.Size() != Src.Size())
                {
                    Cv2.Resize(Target, Target, Src.Size());
                }
                sw.Restart();
                Cv2.AddWeighted(Src, Alpha, Target, Beta, 0.0, Dst);
                sw.Stop();
                CT = sw.ElapsedMilliseconds;
                ImgDst = WriteableBitmapConverter.ToWriteableBitmap(Dst);
            }
            finally
            {
                Running = false;
            }
        }

        private bool Running;
        private double _Alpha = 0.5;

        public double Alpha
        {
            get { return _Alpha; }
            set { SetProperty(ref _Alpha, value); ExecuteGoAddWeighted(); }
        }

        private double _Beta = 0.5;

        public double Beta
        {
            get { return _Beta; }
            set { SetProperty(ref _Beta, value); ExecuteGoAddWeighted(); }
        }

        private DelegateCommand _GoSeamlessClone;

        public DelegateCommand GoSeamlessClone =>
             _GoSeamlessClone ??= new DelegateCommand(ExecuteGoSeamlessClone);

        private void ExecuteGoSeamlessClone()
        {
            if (!Pool.SelectImage.HasValue) return;
            if (!Pool.SelectImage2.HasValue) return;
            if (Pool.SelectImage.Value.Value.Empty()) return;
            if (Pool.SelectImage2.Value.Value.Empty()) return;

            Src = Pool.SelectImage.Value.Value;
            Target = Pool.SelectImage2.Value.Value.Clone();
            Dst = new();
            if (Target.Size() != Src.Size())
            {
                Cv2.Resize(Target, Target, Src.Size());
            }

            if (Src.Type() != Target.Type()) return;
            sw.Restart();
            Cv2.SeamlessClone(Src, Target, null, new(Src.Width / 2, Src.Height / 2), Dst, SeamlessCloneMethodThis);
            sw.Stop();
            CT = sw.ElapsedMilliseconds;
            ImgDst = WriteableBitmapConverter.ToWriteableBitmap(Dst);
        }

        private SeamlessCloneMethods _SeamlessCloneMethodThis = SeamlessCloneMethods.NormalClone;

        public SeamlessCloneMethods SeamlessCloneMethodThis
        {
            get { return _SeamlessCloneMethodThis; }
            set { SetProperty(ref _SeamlessCloneMethodThis, value); }
        }

        private ObservableDictionary<string, SeamlessCloneMethods> _SeamlessCloneMethodList = new();

        public ObservableDictionary<string, SeamlessCloneMethods> SeamlessCloneMethodList
        {
            get { return _SeamlessCloneMethodList; }
            set { SetProperty(ref _SeamlessCloneMethodList, value); }
        }

        #region RotateROIList

        private List<UserControl> _RotateROIList = new();

        public List<UserControl> RotateROIList
        {
            get { return _RotateROIList; }
            set { SetProperty(ref _RotateROIList, value); }
        }

        private DelegateCommand _GoGetRotateROIList;

        public DelegateCommand GoGetRotateROIList =>
             _GoGetRotateROIList ??= new DelegateCommand(ExecuteGoGetRotateROIList);

        private void ExecuteGoGetRotateROIList()
        {
            if (!Pool.SelectImage.HasValue) return;

            if (RotateROIList.Count == 0) return;

            var src = Pool.SelectImage.Value.Value;

            var rotateROI = RotateROIList[0] as RotateRectROI;

            Src = Pool.SelectImage.Value.Value;
            Point center = new(rotateROI.CenterX, rotateROI.CenterY);
            Mat srcrotate = Src.Rotate((float)rotateROI.RectAngle, ref center);

            Rect roi = new(center.X - (int)rotateROI.RectWidth / 2, center.Y - (int)rotateROI.RectHeight / 2, (int)rotateROI.RectWidth, (int)rotateROI.RectHeight);
            Dst = srcrotate[roi];

            ImgDst = WriteableBitmapConverter.ToWriteableBitmap(Dst);
        }

        private DelegateCommand _GoSetRotateROIList;

        public DelegateCommand GoSetRotateROIList =>
             _GoSetRotateROIList ??= new DelegateCommand(ExecuteGoSetRotateROIList);

        private void ExecuteGoSetRotateROIList()
        {
            if (!Pool.SelectImage.HasValue) return;
            if (!Pool.SelectImage2.HasValue) return;
            if (Pool.SelectImage.Value.Value.Empty()) return;
            if (Pool.SelectImage2.Value.Value.Empty()) return;

            if (RotateROIList.Count == 0) return;

            var rrr = RotateROIList[0] as RotateRectROI;

            Src = Pool.SelectImage.Value.Value;
            var subMat = Pool.SelectImage2.Value.Value;

            //求旋转框4个角的坐标
            Mat rectrot = Cv2.GetRotationMatrix2D(new Point2f((float)rrr.CenterX, (float)rrr.CenterY), -rrr.RectAngle, 1);

            var x = rrr.CenterX - rrr.RectWidth / 2;
            var y = rrr.CenterY - rrr.RectHeight / 2;

            var x1 = x * rectrot.At<double>(0, 0) + y * rectrot.At<double>(0, 1) + rectrot.At<double>(0, 2);
            var y1 = x * rectrot.At<double>(1, 0) + y * rectrot.At<double>(1, 1) + rectrot.At<double>(1, 2);

            var x2 = (x + subMat.Width) * rectrot.At<double>(0, 0) + (y * rectrot.At<double>(0, 1)) + rectrot.At<double>(0, 2);
            var y2 = (x + subMat.Width) * rectrot.At<double>(1, 0) + (y * rectrot.At<double>(1, 1)) + rectrot.At<double>(1, 2);

            var x3 = (x + subMat.Width) * rectrot.At<double>(0, 0) + ((y + subMat.Height) * rectrot.At<double>(0, 1)) + rectrot.At<double>(0, 2);
            var y3 = (x + subMat.Width) * rectrot.At<double>(1, 0) + ((y + subMat.Height) * rectrot.At<double>(1, 1)) + rectrot.At<double>(1, 2);

            var x4 = x * rectrot.At<double>(0, 0) + ((y + subMat.Height) * rectrot.At<double>(0, 1)) + rectrot.At<double>(0, 2);
            var y4 = x * rectrot.At<double>(1, 0) + ((y + subMat.Height) * rectrot.At<double>(1, 1)) + rectrot.At<double>(1, 2);
            List<Point2f> dstPoints = new()
            {
                new((float)x1, (float)y1),
                new((float)x2, (float)y2),
                new((float)x3, (float)y3),
                new((float)x4, (float)y4)
            };

            //得到旋转框的变换矩阵
            List<Point2f> srcPoints = new()
            {
                new(0, 0),
                new(subMat.Width, 0),
                new(subMat.Width, subMat.Height),
                new(0, subMat.Height)
            };

            Mat rot = Cv2.GetPerspectiveTransform(srcPoints, dstPoints);

            Mat dst = Src.EmptyClone() * 0;

            Cv2.WarpPerspective(subMat, dst, rot, Src.Size());
            Dst = Src.Clone();

            //求旋转框的蒙板
            //RotatedRect rr = new(
            //                new Point2f((float)rrr.CenterX, (float)rrr.CenterY),
            //                new Size2f(rrr.RectWidth, rrr.RectHeight),
            //                (float)rrr.RectAngle);
            //_ = Src.GetGray(out Mat mask);
            //mask *= 0;
            //List<Point> points = new();
            //rr.Points().ToList().ForEach(p => points.Add((Point)p));

            //mask.FillPolygon(points);

            //Cv2.CopyTo(dst, Dst, mask);

            //使用同一个矩阵获得蒙版

            Mat mask = new(subMat.Size(), MatType.CV_8UC1, Scalar.White);

            Cv2.WarpPerspective(mask, mask, rot, Src.Size());
            Cv2.CopyTo(dst, Dst, mask);
            ImgDst = WriteableBitmapConverter.ToWriteableBitmap(Dst);
        }

        #endregion RotateROIList
    }
}