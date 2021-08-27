using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace OpencvsharpModule.Models
{
    public class CalibrateCameraModel : BindableBase
    {
        public CalibrateCameraModel(IContainerExtension container)
        {
            Common = container.Resolve<CalibrateCommon>();

            // GenChessBoard(60, 13, 9);
        }

        private CalibrateCommon Common { get; set; }

        private Mat[] object_pointsmat, image_points_seqmat;

        private DelegateCommand _Calibration;

        public DelegateCommand Calibration =>
             _Calibration ??= new DelegateCommand(ExecuteCalibration);

        private void ExecuteCalibration()
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
                Mat image_points_bufmat = new();
                if (!Cv2.FindChessboardCorners(gray, board_size, image_points_bufmat, ChessboardFlags.AdaptiveThresh | ChessboardFlags.NormalizeImage | ChessboardFlags.Accuracy))
                {
                    //找角点异常，一般原因是角点的数量没设置对
                    System.Windows.MessageBox.Show("找角点异常，一般原因是角点的数量没设置对");

                    return;
                }
                else
                {
                    if (IsSubpix)
                        Cv2.Find4QuadCornerSubpix(gray, image_points_bufmat, new Size(_SubpixSize, _SubpixSize));
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

            Size square_size = new Size(6, 6);  // 世界坐标系下 黑白块宽度

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

            Mat[] rvecsMat, tvecsMat;
            try
            {
                Cv2.CalibrateCamera(
                                    object_pointsmat,               //世界坐标系中的三维点
                                    image_points_seqmat,            //内角点对应的图像坐标点
                                    imageSize,                      //图像的像素尺寸
                                    cameraMatrix,                   //内参矩阵
                                    distCoeffs,                     //畸变矩阵
                                    out rvecsMat,        //旋转向量
                                    out tvecsMat,        //平移矩阵
                                    CalibrationFlags.RationalModel
                                    );
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("标定失败，一般的原因是图像有问题\n" + ex.Message);

                return;
            }

            StringBuilder msg = new StringBuilder();
            msg.Append(Cv2.Format(distCoeffs) + "\n" + Cv2.Format(cameraMatrix));

            for (int index = 0; index < Common.Images.Count; index++)
            {
                double err; // 每幅图像的平均误差

                Mat image_points = new Mat();
                // 将世界坐标系的点投影到图像上（根据已知的内参、旋转、平移）

                Cv2.ProjectPoints(
                   object_pointsmat[index],
                   rvecsMat[index],
                   tvecsMat[index],
                   cameraMatrix,
                   distCoeffs,
                   image_points
                  );
                Mat rvec = new(), tvec = new();
                Cv2.SolvePnP(object_pointsmat[index], image_points, cameraMatrix, distCoeffs, rvec, tvec);
                Debug.WriteLine("-------------------------------");

                Debug.WriteLine(Cv2.Format(rvecsMat[index]));
                Debug.WriteLine("-----------");
                Debug.WriteLine(Cv2.Format(rvec));
                Debug.WriteLine("-----------");

                Debug.WriteLine(Cv2.Format(tvecsMat[index]));
                Debug.WriteLine("-----------");
                Debug.WriteLine(Cv2.Format(tvec));
                Debug.WriteLine("-----------");
                //Debug.WriteLine(Cv2.Format(image_points));
                //Debug.WriteLine("-----------");

                //Debug.WriteLine(Cv2.Format(image_points_seqmat[index]));
                //Debug.WriteLine("-----------");

                err = Cv2.Norm(image_points, image_points_seqmat[index], NormTypes.L2);
                msg.Append("\n第" + (index + 1) + "幅图像的平均误差：\n" + err + "像素\n");
            }
            Common.Msg = msg.ToString();
            System.Windows.MessageBox.Show("标定成功");
        }

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

        /// <summary>内参矩阵</summary>
        private Mat cameraMatrix = new Mat(3, 3, MatType.CV_32FC1);

        /// <summary>畸变矩阵</summary>
        private Mat distCoeffs = new Mat(1, 5, MatType.CV_32FC1);

        private DelegateCommand _Undistorted;

        /// <summary>
        /// 畸变校正
        /// </summary>
        public DelegateCommand Undistorted =>
             _Undistorted ??= new DelegateCommand(ExecuteUndistorted);

        private void ExecuteUndistorted()
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
                Cv2.Undistort(imageInput, Undistorimage, cameraMatrix, distCoeffs);
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

        private void ExecuteSaveMat()
        {
            Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Title = "标定文件",
                Filter = "标定文件|*.Calibrate",
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

        private void ExecuteLoadCalibrationFile()
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "标定文件",
                Filter = "标定文件|*.Calibrate",
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

        /// <summary>
        /// 用于生成标准棋盘格的函数
        /// </summary>
        /// <param name="chesss_size">单个棋盘格子的大小</param>
        /// <param name="board_width_count">棋盘的宽的格子数量</param>
        /// <param name="board_height_count">棋盘的高的格子数量</param>
        /// <param name="savename"></param>
        /// <param name="imgformat"></param>代表图片格式，枚举型
        public static void GenChessBoard(int chesss_size, int board_width_count, int board_height_count)
        {
            int imgw = chesss_size * board_width_count;
            int imgh = chesss_size * board_height_count;
            Mat blackCell = new(chesss_size, chesss_size, MatType.CV_8UC1, Scalar.Black);
            Mat Targermat = new(imgh, imgw, MatType.CV_8UC1, Scalar.White);

            for (int i = 0; i < board_width_count; i++)
            {
                for (int j = 0; j < board_height_count; j++)
                {
                    if ((i % 2 == 0 && j % 2 == 0) || (i % 2 == 1 && j % 2 == 1))
                    {
                        Targermat[chesss_size * j, chesss_size * (j + 1), chesss_size * i, chesss_size * (i + 1)] = blackCell;
                    }
                }
            }

            Targermat.SaveImage("qipan.jpg");
        }
    }
}