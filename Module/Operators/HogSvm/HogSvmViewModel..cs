using ModuleCore.Mvvm;
using ModuleCore.Services;
using ModuleCore.UserControls;
using OpenCvSharp;
using OpenCvSharp.ML;
using OpenCvSharp.WpfExtensions;
using OpencvsharpModule.Common;
using OpencvsharpModule.Models;
using OpencvsharpModule.Views;
using Prism.Commands;
using Prism.Ioc;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace OpencvsharpModule.ViewModels
{
    public partial class HogSvmViewModel : RegionViewModelBase
    {
        public ImagePool Pool { get; set; }
        private string _ViewName;

        public string ViewName
        {
            get { return _ViewName; }
            set { SetProperty(ref _ViewName, value); }
        }

        public HogSvmViewModel(IContainerExtension container, IRegionManager regionManager) : base(regionManager)
        {
            Pool = container.Resolve<ImagePool>();
            ViewName = this.GetType().Name;
            regionManager.RegisterViewWithRegion(ViewName, typeof(CommonView));
        }

        private List<UserControl> _ROIList = new();

        public List<UserControl> ROIList
        {
            get { return _ROIList; }
            set { SetProperty(ref _ROIList, value); }
        }

        private WriteableBitmap _imgDst;

        public WriteableBitmap ImgDst
        {
            get { return _imgDst; }
            set { SetProperty(ref _imgDst, value); }
        }

        private WriteableBitmap _ImgSrc;

        public WriteableBitmap ImgSrc
        {
            get { return _ImgSrc; }
            set { SetProperty(ref _ImgSrc, value); }
        }

        private System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();

        public Mat Src { get; set; } = new Mat();
        public Mat Dst { get; set; } = new Mat();

        private long _CT;

        public long CT
        {
            get { return _CT; }
            set { SetProperty(ref _CT, value); }
        }

        private string _CommandText;

        public string CommandText
        {
            get { return _CommandText; }
            set { SetProperty(ref _CommandText, value); }
        }
    }

    public partial class HogSvmViewModel
    {
        private string _TrainDataFolder;

        public string TrainDataFolder
        {
            get { return _TrainDataFolder; }
            set { SetProperty(ref _TrainDataFolder, value); }
        }

        private DelegateCommand _TrainDataFolderSelect;

        public DelegateCommand TrainDataFolderSelect =>
             _TrainDataFolderSelect ??= new DelegateCommand(ExecuteTrainDataFolderSelect);

        private void ExecuteTrainDataFolderSelect()
        {
            Microsoft.Win32.OpenFileDialog openFileDialog = new()
            {
                Title = "选择文件夹",
                Filter = "文件夹|*.directory",
                FileName = "选择此文件夹",

                ValidateNames = false,
                CheckFileExists = false,
                CheckPathExists = true,
                Multiselect = true,//允许同时选择多个文件
                //InitialDirectory = FilePath//指定启动路径
            };

            if (openFileDialog.ShowDialog() == true)

            {
                TrainDataFolder = openFileDialog.FileName.Replace("选择此文件夹.directory", "");
            }
        }

        private DelegateCommand _GoTrain;

        public DelegateCommand GoTrain =>
             _GoTrain ??= new DelegateCommand(ExecuteGoTrain);

        async private void ExecuteGoTrain()
        {
            if (string.IsNullOrEmpty(TrainDataFolder)) return;
            if (!System.IO.Directory.Exists(TrainDataFolder)) return;

            var folders = Directory.GetDirectories(TrainDataFolder);
            labelsCN = folders.Select(str => str.Replace(TrainDataFolder, "")).ToList();
            TrainData = new();
            TrainLabel = new();
            for (int i = 0; i < folders.Length; i++)
            {
                var path = folders[i];
                var trainImg = Directory.GetFiles(path).ToList();
                trainImg.ForEach(str =>
                {
                    var vecMat = GetVec(str);

                    TrainData.PushBack(vecMat);
                    var labelMat = new Mat(1, 1, MatType.CV_32SC1, new int[] { i });
                    TrainLabel.PushBack(labelMat);
                });
            }
            svm = SVM.Create();
            svm.KernelType = SVM.KernelTypes.Linear;
            svm.Type = SVM.Types.CSvc;
            svm.Gamma = 5.383;

            svm.C = 2.67;

            await Task.Run(() => svm.Train(TrainData, SampleTypes.RowSample, TrainLabel));
            System.Windows.MessageBox.Show("训练成功");
        }

        private SVM svm;
        private Mat TrainData;
        private Mat TrainLabel;
        private List<string> labelsCN;

        private DelegateCommand _GoPredict;

        public DelegateCommand GoPredict =>
             _GoPredict ??= new DelegateCommand(ExecuteGoPredict);

        private void ExecuteGoPredict()
        {
            if (svm is null) return;
            Microsoft.Win32.OpenFileDialog ofd = new()
            {
                DefaultExt = ".*",
                Filter = "图像文件(*.jpg;*.png;*.bmp)|*.jpg;*.png;*.bmp"
            };
            if (ofd.ShowDialog() == true)
            {
                try
                {
                    Mat src = Cv2.ImRead(ofd.FileName);

                    Pool.ImgSrc = WriteableBitmapConverter.ToWriteableBitmap(src);

                    var vecMat = GetVec(ofd.FileName);
                    var predictlabel1 = svm.Predict(vecMat);
                    CommandText = "识别结果： " + labelsCN[(int)predictlabel1];

                    var mat1 = new Mat(128, 64, MatType.CV_8UC1, Scalar.Black);
                    var scale = 64 / (double)src.Cols < 128 / (double)src.Rows ? 64 / (double)src.Cols : 128 / (double)src.Rows;
                    var width = (int)(src.Cols * scale);
                    var height = (int)(src.Rows * scale);
                    src.Resize(new Size(width, height)).GetGray(out Mat mat2);
                    mat1[(128 - height) / 2, (128 - height) / 2 + height, (64 - width) / 2, (64 - width) / 2 + width] = mat2;
                    mat1 = mat1.CvtColor(ColorConversionCodes.GRAY2BGR);
                    mat1.PutText(labelsCN[(int)predictlabel1], new(10, 10), HersheyFonts.HersheyDuplex, 0.5d, Scalar.Red);
                    ImgDst = WriteableBitmapConverter.ToWriteableBitmap(mat1);
                }
                catch (Exception ex)
                {
                    CommandText = ex.Message;
                }
            }
        }

        private DelegateCommand _GoPredictROI;

        public DelegateCommand GoPredictROI =>
             _GoPredictROI ??= new DelegateCommand(ExecuteGoPredictSrc);

        private void ExecuteGoPredictSrc()
        {
            if (svm is null) return;
            if (!Pool.SelectImage.HasValue) return;

            Src = Pool.SelectImage.Value.Value;
            if (Src is null || Src.Empty()) return;
            if (ROIList.Count < 1) return;

            try
            {
                CommandText = "识别结果： ";
                Dst = new();
                foreach (var item in ROIList)
                {
                    var roi = item as RectROI;

                    var mat = Src[roi.RectTop, roi.RectTop + roi.RectHeight, roi.RectLeft, roi.RectLeft + roi.RectWidth].Clone();
                    var vecMat = GetVec(mat, out Mat mat1);

                    var predictlabel1 = svm.Predict(vecMat);
                    var label = labelsCN[(int)predictlabel1];
                    CommandText += label + " ";
                    mat1 = mat1.CvtColor(ColorConversionCodes.GRAY2BGR);
                    mat1.PutText(label, new(10, 10), HersheyFonts.HersheyDuplex, 0.5d, Scalar.Red);
                    Cv2.Transpose(mat1, mat1);
                    Cv2.Flip(mat1, mat1, FlipMode.Y);
                    Dst.PushBack(mat1);
                }
                Cv2.Transpose(Dst, Dst);
                Cv2.Flip(Dst, Dst, FlipMode.X);
                ImgDst = WriteableBitmapConverter.ToWriteableBitmap(Dst);
            }
            catch (Exception ex)
            {
                CommandText = ex.Message;
            }
        }

        private DelegateCommand _GoSaveTrain;

        public DelegateCommand GoSaveTrain =>
             _GoSaveTrain ??= new DelegateCommand(ExecuteGoSaveTrain);

        private void ExecuteGoSaveTrain()
        {
            if (svm is null) return;

            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "模型文件(*.dat) | *.dat",
                Title = "Save File"
            };

            if (saveFileDialog.ShowDialog() == false) return;
            if (string.IsNullOrEmpty(saveFileDialog.FileName)) return;
            try
            {
                svm.Save(saveFileDialog.FileName);
                JsonService.ObjectToFile(labelsCN, saveFileDialog.FileName + ".json");
                System.Windows.MessageBox.Show("保存完成");
            }
            catch (Exception ex)
            {
                CommandText = ex.Message;
            }
        }

        private DelegateCommand _GoLoadTrain;

        public DelegateCommand GoLoadTrain =>
             _GoLoadTrain ??= new DelegateCommand(ExecuteGoLoadTrain);

        async private void ExecuteGoLoadTrain()
        {
            Microsoft.Win32.OpenFileDialog ofd = new()
            {
                DefaultExt = ".*",
                Filter = "模型文件(*.dat)|*.dat"
            };
            if (ofd.ShowDialog() == true)
            {
                try
                {
                    await Task.Run(() => svm = SVM.Load(ofd.FileName));
                    labelsCN = JsonService.JObjectFromFile<List<string>>(ofd.FileName + ".json");
                    System.Windows.MessageBox.Show("载入成功");
                }
                catch (Exception ex)
                {
                    CommandText = ex.Message;
                }
            }
        }

        private static Mat GetVec(string file)
        {
            var mat = Cv2.ImRead(file, ImreadModes.Grayscale);
            var mat1 = Mat.Zeros(128, 64, MatType.CV_8UC1).ToMat();
            var scale = 64 / (double)mat.Cols < 128 / (double)mat.Rows ? 64 / (double)mat.Cols : 128 / (double)mat.Rows;
            var width = (int)(mat.Cols * scale);
            var height = (int)(mat.Rows * scale);
            var mat2 = mat.Resize(new Size(width, height));
            mat1[(128 - height) / 2, (128 - height) / 2 + height, (64 - width) / 2, (64 - width) / 2 + width] = mat2;

            var hog = new HOGDescriptor();

            var vec = hog.Compute(mat1);
            var vecMat = new Mat(1, vec.Length, MatType.CV_32FC1, vec);
            return vecMat;
        }

        private static Mat GetVec(Mat mat, out Mat format)
        {
            if (mat.Channels() == 3) mat = mat.CvtColor(ColorConversionCodes.BGR2GRAY);
            var mat1 = Mat.Zeros(128, 64, MatType.CV_8UC1).ToMat();
            var scale = 64 / (double)mat.Cols < 128 / (double)mat.Rows ? 64 / (double)mat.Cols : 128 / (double)mat.Rows;
            var width = (int)(mat.Cols * scale);
            var height = (int)(mat.Rows * scale);
            var mat2 = mat.Resize(new Size(width, height));
            mat1[(128 - height) / 2, (128 - height) / 2 + height, (64 - width) / 2, (64 - width) / 2 + width] = mat2;

            var hog = new HOGDescriptor();

            var vec = hog.Compute(mat1);
            var vecMat = new Mat(1, vec.Length, MatType.CV_32FC1, vec);
            format = mat1;
            return vecMat;
        }
    }
}