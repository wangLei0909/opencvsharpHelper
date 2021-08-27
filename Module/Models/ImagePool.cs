using ModuleCore.Mvvm;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using OpencvsharpModule.Common;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Windows.Media.Imaging;

namespace OpencvsharpModule.Models
{
    public class ImagePool : BindableBase
    {
        private ObservableDictionary<string, Mat> _images = new();

        public ObservableDictionary<string, Mat> Images
        {
            get { return _images; }
            set { SetProperty(ref _images, value); }
        }

        private DelegateCommand _RemoveImg;

        public DelegateCommand RemoveImg =>
             _RemoveImg ??= new DelegateCommand(ExecuteRemoveImg);

        private void ExecuteRemoveImg()
        {
            if (SelectImage.HasValue)
            {
                Images.Remove(SelectImage.Value.Key);
                SelectImage = null;
            }
        }

        private DelegateCommand _ClearPool;

        public DelegateCommand ClearPool =>
             _ClearPool ??= new DelegateCommand(ExecuteClearPool);

        private void ExecuteClearPool()
        {
            SelectImage = null;
            Images.Clear();
        }

        // 选择的图像
        private KeyValuePair<string, Mat>? _selectImage;

        public KeyValuePair<string, Mat>? SelectImage
        {
            get { return _selectImage; }
            set
            {
                if (value is not null && value.HasValue&& value.Value.Value is not null && !value.Value.Value.Empty())
                {
                    //从相机采集非ROI图像时调整
                    if (IsCamera)
                    {
                        var src = value.Value.Value;

                        if (ROIHeight + ROITop > src.Height)
                        {
                            ROIHeight = src.Height / 2;
                            ROITop = 0;
                        }
                        if (ROIWidth + ROILeft > src.Width)
                        {
                            ROILeft = 0;
                            ROIWidth = src.Width / 2;
                        }
                        IsCamera = false;
                    }
     
                    ImgSrc = WriteableBitmapConverter.ToWriteableBitmap(value.Value.Value);
                    ImageSrcMsg = value.Value.Value.ToString();
                }
                SetProperty(ref _selectImage, value);
            }
        }

        // 选择的图像
        private KeyValuePair<string, Mat>? _selectImage2;

        public KeyValuePair<string, Mat>? SelectImage2
        {
            get { return _selectImage2; }
            set
            {
                if (value is not null && value.HasValue && !value.Value.Value.Empty())
                {
                    ImgSrc2 = WriteableBitmapConverter.ToWriteableBitmap(value.Value.Value);
                }
                SetProperty(ref _selectImage2, value);
            }
        }
        // 选择的图像
        private KeyValuePair<string, Mat>? _SelectMask;

        public KeyValuePair<string, Mat>? SelectMask
        {
            get { return _SelectMask; }
            set
            {
                if (value is not null && value.HasValue && !value.Value.Value.Empty())
                {
                    value.Value.Value.GetGray(out Mat gray );

                    Mat mask = new();
                    Mat z = gray.Clone() * 0;
                    Mat[] mats = { gray, z, z.Clone() };
                    Cv2.Merge(mats, mask);
                    MaskSrc = WriteableBitmapConverter.ToWriteableBitmap(mask);
                }
                SetProperty(ref _SelectMask, value);
            }
        }
        private string _ImageSrcMsg;

        public string ImageSrcMsg
        {
            get { return _ImageSrcMsg; }
            set { SetProperty(ref _ImageSrcMsg, value); }
        }

        private WriteableBitmap _ImgSrc;

        public WriteableBitmap ImgSrc
        {
            get { return _ImgSrc; }
            set { SetProperty(ref _ImgSrc, value); }
        }
        private WriteableBitmap _MaskSrc;

        public WriteableBitmap MaskSrc
        {
            get { return _MaskSrc; }
            set { SetProperty(ref _MaskSrc, value); }
        }
        public bool IsCamera { get; set; }

        private WriteableBitmap _ImgSrc2;

        public WriteableBitmap ImgSrc2
        {
            get { return _ImgSrc2; }
            set { SetProperty(ref _ImgSrc2, value); }
        }

        private ObservableDictionary<string, Point[]> _contours = new();

        public ObservableDictionary<string, Point[]> Contours
        {
            get { return _contours; }
            set { SetProperty(ref _contours, value); }
        }

        private Point[] _contour;

        public Point[] Contour
        {
            get { return _contour; }
            set { SetProperty(ref _contour, value); }
        }

        #region ROI

        private int _ROIHeight = 100;

        public int ROIHeight
        {
            get { return _ROIHeight; }
            set { SetProperty(ref _ROIHeight, value); }
        }

        private int _ROIWidth = 100;

        public int ROIWidth
        {
            get { return _ROIWidth; }
            set { SetProperty(ref _ROIWidth, value); }
        }

        private int _ROILeft = 10;

        public int ROILeft
        {
            get { return _ROILeft; }
            set { SetProperty(ref _ROILeft, value); }
        }

        private int _ROITop = 10;

        public int ROITop
        {
            get { return _ROITop; }
            set { SetProperty(ref _ROITop, value); }
        }

        private int _ThumdSize = 20;

        public int ThumdSize
        {
            get { return _ThumdSize; }
            set { SetProperty(ref _ThumdSize, value); }
        }

        #endregion ROI
    }
}