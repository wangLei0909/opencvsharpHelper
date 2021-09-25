using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ModuleCore.UserControls
{
    /// <summary>
    /// ImageEdit.xaml 的交互逻辑
    /// </summary>
    public partial class ImageEdit : UserControl
    {
        public ImageEdit()
        {
            InitializeComponent();
        }

        #region 缩放

        private double zoom = 1d;

        private void Canvas_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            //鼠标位置
            Point p = e.GetPosition(mainBox1);

            //bs 缩放系数 e.Delta 上滚120 & 下滚-120
            double bs = 1 + e.Delta * 0.001;

            //相对鼠标的移动量
            double offX = p.X - p.X * bs;
            double offY = p.Y - p.Y * bs;

            //变换矩阵
            var newMatrix = new Matrix(bs, 0, 0, bs, offX, offY);

            //
            matrix.Matrix = newMatrix * matrix.Matrix;
            zoom *= bs;
            rate.Text = zoom.ToString("P2");
        }

        #endregion 缩放

        #region 图片

        public WriteableBitmap ImageSource
        {
            get { return (WriteableBitmap)GetValue(ImageSourceProperty); }
            set { SetValue(ImageSourceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ImageSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ImageSourceProperty =
            DependencyProperty.Register("ImageSource",
                typeof(WriteableBitmap),
                typeof(ImageEdit),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, ImageSourceChangedCallback));

        private static void ImageSourceChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var u = d as ImageEdit;
            u.img1.Source = (WriteableBitmap)e.NewValue;
            u.matrix.Matrix = new Matrix(1, 0, 0, 1, 0, 0);
            u.zoom = 1d;
            u.rate.Text = u.zoom.ToString("P2");
        }

        #endregion 图片

        #region 拖动

        private bool isMouseLeftButtonDown = false;

        private Point previousMousePoint;
        private Point position;

        private void Img_MouseDown1(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Released) return;
            isMouseLeftButtonDown = true;
            previousMousePoint = e.GetPosition(mainBox1);
        }

        private void Img_MouseUp1(object sender, MouseButtonEventArgs e)
        {
            if (isMouseLeftButtonDown && isDrawing)
            {
                if (mainBox1.Children.Contains(ROItemp))
                    mainBox1.Children.Remove(ROItemp);
                switch (DrawType)
                {
                    case "Rect":
                        RectROI roi = new();

                        roi.RectHeight = ROItemp.RectHeight;
                        roi.RectWidth = ROItemp.RectWidth;
                        mainBox1.Children.Add(roi);
                        roi.RectTop = (int)Canvas.GetTop(ROItemp);
                        roi.RectLeft = (int)Canvas.GetLeft(ROItemp);
                        ROIList.Add(roi);
                        roi.RemoveEvent += RemoveROI;
                        break;

                    case "RotateRect":
                        RotateRectROI rrroi = new();
                        rrroi.roi.Width = ROItemp.RectWidth;
                        rrroi.roi.Height = ROItemp.RectHeight;

                        rrroi.RectWidth = ROItemp.RectWidth;
                        rrroi.RectHeight = ROItemp.RectHeight;

                        var top = (int)Canvas.GetTop(ROItemp);
                        var left = (int)Canvas.GetLeft(ROItemp);

                        rrroi.CenterX = left + rrroi.RectWidth / 2;
                        rrroi.CenterY = top + rrroi.RectHeight / 2;
                        mainBox1.Children.Add(rrroi);
                        rrroi.RemoveEvent += RemoveRotateROI;
                        Canvas.SetTop(rrroi.roi, top);
                        Canvas.SetLeft(rrroi.roi, left);
                        RotateROIList.Add(rrroi);
                        break;

                    default:
                        break;
                }

                isDrawing = false;
            }
            isMouseLeftButtonDown = false;
        }

        private void RemoveROI(UserControl obj)
        {
            mainBox1.Children.Remove(obj);
            ROIList.Remove(obj);
        }

        private void RemoveRotateROI(UserControl obj)
        {
            mainBox1.Children.Remove(obj);
            RotateROIList.Remove(obj);
        }

        private void Img_MouseLeave1(object sender, MouseEventArgs e)
        {
            isMouseLeftButtonDown = false;
        }

        private RectDraw ROItemp = new();

        private void Img_MouseMove1(object sender, MouseEventArgs e)
        {
            position = e.GetPosition(mainBox1);
            mousex.Text = position.X.ToString("F0");
            mousey.Text = position.Y.ToString("F0");

            if (isMouseLeftButtonDown == false) return;

            if (isDrawing)
            {
                if (mainBox1.Children.Contains(ROItemp))
                    mainBox1.Children.Remove(ROItemp);
                var startX = System.Math.Min(position.X, previousMousePoint.X);
                var startY = System.Math.Min(position.Y, previousMousePoint.Y);
                var endX = System.Math.Max(position.X, previousMousePoint.X);
                var endY = System.Math.Max(position.Y, previousMousePoint.Y);
                double width = endX - startX;
                double height = endY - startY;

                if (width < 1 || height < 1) return;

                ROItemp.RectWidth = (int)width;
                ROItemp.RectHeight = (int)height;
                mainBox1.Children.Add(ROItemp);
                Canvas.SetTop(ROItemp, startY);
                Canvas.SetLeft(ROItemp, startX);

                return;
            }

            double offX = position.X - previousMousePoint.X;
            double offY = position.Y - previousMousePoint.Y;
            //变换矩阵
            var newMatrix = new Matrix(1, 0, 0, 1, offX, offY);
            matrix.Matrix = newMatrix * matrix.Matrix;
        }

        #endregion 拖动

        //画矩形
        private bool isDrawing;

        private void RecoverMatrix(object sender, MouseButtonEventArgs e)
        {
            matrix.Matrix = new Matrix(1, 0, 0, 1, 0, 0);
            zoom = 1d;
            rate.Text = zoom.ToString("P2");
        }

        private void DrawRect(object sender, MouseButtonEventArgs e)
        {
            isDrawing = true;
            DrawType = "Rect";
        }

        #region 选区

        public List<UserControl> ROIList
        {
            get { return (List<UserControl>)GetValue(ROIListProperty); }
            set { SetValue(ROIListProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ROIListProperty =
            DependencyProperty.Register("ROIList", typeof(List<UserControl>), typeof(ImageEdit), new PropertyMetadata(new List<UserControl>()));

        public List<UserControl> RotateROIList
        {
            get { return (List<UserControl>)GetValue(RotateROIListProperty); }
            set { SetValue(RotateROIListProperty, value); }
        }

        public static readonly DependencyProperty RotateROIListProperty =
          DependencyProperty.Register("RotateROIList", typeof(List<UserControl>), typeof(ImageEdit), new PropertyMetadata(new List<UserControl>()));

        #endregion 选区

        private void DrawRotateRect(object sender, MouseButtonEventArgs e)
        {
            isDrawing = true;
            DrawType = "RotateRect";
        }

        private string DrawType = "Rect";
    }
}