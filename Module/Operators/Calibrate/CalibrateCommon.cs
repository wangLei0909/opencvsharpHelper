using ModuleCore.Mvvm;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using Prism.Mvvm;
using System.Collections.Generic;
using System.Windows.Media.Imaging;

namespace OpencvsharpModule.Models
{
    public class CalibrateCommon : BindableBase
    {
        private ObservableDictionary<string, Mat> _images = new();

        /// <summary>
        /// 标定图片
        /// </summary>
        public ObservableDictionary<string, Mat> Images
        {
            get { return _images; }
            set { SetProperty(ref _images, value); }
        }

        private KeyValuePair<string, Mat> _selectImage;

        public KeyValuePair<string, Mat> SelectImage
        {
            get { return _selectImage; }
            set
            {
                SetProperty(ref _selectImage, value);
                ImgSrc = WriteableBitmapConverter.ToWriteableBitmap(value.Value);
            }
        }

        private WriteableBitmap _ImgSrc;

        public WriteableBitmap ImgSrc
        {
            get { return _ImgSrc; }
            set { SetProperty(ref _ImgSrc, value); }
        }

        private WriteableBitmap _ImgDst;

        public WriteableBitmap ImgDst
        {
            get { return _ImgDst; }
            set { SetProperty(ref _ImgDst, value); }
        }

        private string _msg;

        public string Msg
        {
            get { return _msg; }
            set { SetProperty(ref _msg, value); }
        }

        private int _boardSizeX = 11;

        public int BoardSizeX
        {
            get { return _boardSizeX; }
            set { SetProperty(ref _boardSizeX, value); }
        }

        private int _boardSizeY = 8;

        public int BoardSizeY
        {
            get { return _boardSizeY; }
            set { SetProperty(ref _boardSizeY, value); }
        }

        private ObservableDictionary<string, Mat> _ProcessImages = new();

        public ObservableDictionary<string, Mat> ProcessImages
        {
            get { return _ProcessImages; }
            set { SetProperty(ref _ProcessImages, value); }
        }

        private KeyValuePair<string, Mat> _ProcessImageSelect;

        public KeyValuePair<string, Mat> ProcessImageSelect
        {
            get { return _ProcessImageSelect; }
            set
            {
                SetProperty(ref _ProcessImageSelect, value);
                ImgSrc = WriteableBitmapConverter.ToWriteableBitmap(value.Value);
            }
        }
    }
}