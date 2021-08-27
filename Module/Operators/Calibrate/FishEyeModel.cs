using ModuleCore.Mvvm;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;

namespace OpencvsharpModule.Models
{
    public class FishEyeModel : BindableBase
    {
        public FishEyeModel(IContainerExtension container)
        {
            Common = container.Resolve<CalibrateCommon>();

        }

        CalibrateCommon Common { get; set; }

        Mat[] object_pointsmat, image_points_seqmat;
        private bool _IsSubpix;

        public bool IsSubpix
        {
            get { return _IsSubpix; }
            set { SetProperty(ref _IsSubpix, value); }
        }

        private int _SubpixSize = 7;
        public int SubpixSize
        {
            get { return _SubpixSize; }
            set
            {
                if (value < 3 || value > 30) return;
                SetProperty(ref _SubpixSize, value);
            }
        }
        private DelegateCommand _Calibration;
        public DelegateCommand Calibration =>
             _Calibration ??= new DelegateCommand(ExecuteCalibration);

        void ExecuteCalibration()
        {
            Size board_size = new Size(Common.BoardSizeX, Common.BoardSizeY);// 标定板角点的尺寸
            if (Common.Images.Count < 1) return;
            // 检测角点，内角点对应的图像坐标点
            image_points_seqmat = new Mat[Common.Images.Count];
            Size imageSize = new Size();
            var mats = Common.Images.Values.ToArray();
            for (var i = 0; i < Common.Images.Count; i++)
            {
                Mat imageInput = mats[i];
                Mat gray;
                if (imageInput.Channels() == 3)
                    gray = imageInput.CvtColor(ColorConversionCodes.BGR2GRAY);
                else
                    gray = imageInput.Clone();
                imageSize = imageInput.Size();
                Mat image_points_bufmat = new Mat();
                if (!Cv2.FindChessboardCorners(gray, board_size, image_points_bufmat))
                {
                    //找角点异常，一般原因是角点的数量没设置对
                    System.Windows.MessageBox.Show("找角点异常，一般原因是角点的数量没设置对");
                   
                    return;
                }
                else
                {
                    if(IsSubpix)
                        Cv2.Find4QuadCornerSubpix(gray, image_points_bufmat, new Size(SubpixSize, SubpixSize));
                    image_points_seqmat[i] = image_points_bufmat;
                    //  在图像上显示角点位置
                    Mat DrawImage;
                    if (imageInput.Depth() == 1)
                        DrawImage = imageInput.CvtColor(ColorConversionCodes.GRAY2BGR);
                    else
                        DrawImage = imageInput.Clone();

                    Cv2.DrawChessboardCorners(DrawImage, board_size, image_points_bufmat, true);
                    Common.ProcessImages.Add(DateTime.Now.Ticks.ToString(), DrawImage);
                }
            }

            Size square_size = new Size(60, 60);  // 世界坐标系下 黑白块宽度  


            Mat world = new Mat();
            object_pointsmat = new Mat[Common.Images.Count];

            for (int i = 0; i < board_size.Height; i++)
            {
                for (int j = 0; j < board_size.Width; j++)
                {
                    Point3f realPoint = new Point3f
                    {

                        //假设标定板放在世界坐标系中z=0的平面上
                        X = i * square_size.Width,
                        Y = j * square_size.Height,
                        Z = 0
                    };
                    world.PushBack(realPoint);
                }
            }
            for (var i = 0; i < Common.Images.Count; i++)
            {
                object_pointsmat[i] = world;
            }
            IEnumerable<Mat> rv, tv;

            try
            {


                Cv2.FishEye.Calibrate(
                                    object_pointsmat,               //世界坐标系中的三维点
                                    image_points_seqmat,            //内角点对应的图像坐标点
                                    imageSize,                      //图像的像素尺寸 
                                    cameraMatrix,                   //内参矩阵
                                    distCoeffs,                     //畸变矩阵
                                    out rv,        //旋转向量                                
                                    out tv,        //平移矩阵
                                   FishEyeCalibrationFlags.RecomputeExtrinsic | FishEyeCalibrationFlags.CheckCond | FishEyeCalibrationFlags.FixSkew
                                    );
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("标定失败，一般的原因是图像有问题\n" + ex.Message);
  
                return;

            }

            rvecsMat = new List<Mat>();
            foreach (var r in rv) rvecsMat.Add(r);
            tvecsMat = new List<Mat>();
            foreach (var t in tv) tvecsMat.Add(t);
            StringBuilder msg = new StringBuilder();
            msg.Append(Cv2.Format(distCoeffs) + "\n" + Cv2.Format(cameraMatrix));


            for (int index = 0; index < Common.Images.Count; index++)
            {
                double err; // 每幅图像的平均误差
                            // 将世界坐标系的点投影到图像上（根据已知的内参、旋转、平移）

                Mat image_points = new Mat();

                Cv2.FishEye.ProjectPoints(
                   object_pointsmat[index],
                   image_points,
                   rvecsMat[index],
                   tvecsMat[index],
                   cameraMatrix,
                   distCoeffs

                  );
                // Console.WriteLine("image_points:=====================================================================");
                // Console.WriteLine(Cv2.Format(image_points, FormatType.Python));
                //// 计算图像上的角点 与 计算出来的图像上的点 比较

                // Console.WriteLine("image_points_seqmat[index]:======================================================================");
                // Console.WriteLine(Cv2.Format(image_points_seqmat[index], FormatType.Python));
                err = Cv2.Norm(image_points, image_points_seqmat[index], NormTypes.L2);
                msg.Append("\n第" + (index + 1) + "幅图像的平均误差：\n" + err + "像素\n");
            }
            Common.Msg = msg.ToString();
            System.Windows.MessageBox.Show("标定成功");

        }
        List<Mat> tvecsMat, rvecsMat;
        /// <summary>内参矩阵</summary>
        Mat cameraMatrix = new Mat(3, 3, MatType.CV_32FC1);
        /// <summary>畸变矩阵</summary>
        Mat distCoeffs = new Mat(1, 4, MatType.CV_32FC1);


        private DelegateCommand _FishEyeUndistorted;

        /// <summary>
        /// 畸变校正
        /// </summary>
        public DelegateCommand FishEyeUndistorted =>
             _FishEyeUndistorted ??= new DelegateCommand(ExecuteFishEyeUndistorted);

        void ExecuteFishEyeUndistorted()
        {

            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "选择图片",
                Filter = "图片|*.jpg;*.png;*.jpeg;*.bmp",
                CheckFileExists = true,
                CheckPathExists = true,
            };


            if (openFileDialog.ShowDialog() == true)
            {
                Mat imageInput = new Mat(openFileDialog.FileName, ImreadModes.AnyColor);

                Common.ImgSrc = WriteableBitmapConverter.ToWriteableBitmap(imageInput);
                Mat R = Mat.Eye(3, 3, MatType.CV_32FC1);
                Mat map1 = new Mat(), map2 = new Mat();
                Cv2.FishEye.InitUndistortRectifyMap(cameraMatrix, distCoeffs, R, cameraMatrix, imageInput.Size(), MatType.CV_32FC1, map1, map2);
                Cv2.Remap(imageInput, Undistorimage, map1, map2);
                Common.ImgDst = WriteableBitmapConverter.ToWriteableBitmap(Undistorimage);

            }
        }

        private Mat _src;
        public Mat Src
        {
            get { return _src; }
            set { SetProperty(ref _src, value); }
        }

        private Mat _undistorimage = new Mat();
        public Mat Undistorimage
        {
            get { return _undistorimage; }
            set { SetProperty(ref _undistorimage, value); }
        }

        private DelegateCommand _saveCalibrationFile;
        public DelegateCommand SaveCalibrationFile =>
             _saveCalibrationFile ??= new DelegateCommand(ExecuteSaveMat);

        void ExecuteSaveMat()
        {

            Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Title = "fisheye标定文件",
                Filter = "fisheye标定文件|*.fisheye",


            };

            if (saveFileDialog.ShowDialog() == true)
            {
                FileStorage storage = new FileStorage(saveFileDialog.FileName, FileStorage.Modes.Write);

                storage.Write("distCoeffs", distCoeffs);
                storage.Write("cameraMatrix", cameraMatrix);
                storage.Release();
            }
        }

        private DelegateCommand _loadCalibrationFile;
        public DelegateCommand LoadCalibrationFile =>
             _loadCalibrationFile ??= new DelegateCommand(ExecuteLoadCalibrationFile);

        void ExecuteLoadCalibrationFile()
        {

            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "fisheye标定文件",
                Filter = "fisheye标定文件|*.fisheye",
                CheckFileExists = true,
                CheckPathExists = true,
            };


            if (openFileDialog.ShowDialog() == true)
            {
                FileStorage storage = new FileStorage(openFileDialog.FileName, FileStorage.Modes.Read);

                distCoeffs = storage["distCoeffs"].ToMat();
                cameraMatrix = storage["cameraMatrix"].ToMat();
                Common.Msg = Cv2.Format(distCoeffs) + "\n" + Cv2.Format(cameraMatrix);
            }
        }
    }
}
