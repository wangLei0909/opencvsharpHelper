using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ModuleCore.UserControls
{
    /// <summary>
    /// ImageView.xaml 的交互逻辑
    /// </summary>
    public partial class ImageView : UserControl
    {
        public ImageView()
        {
            InitializeComponent();

        }

 
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
        }

        public WriteableBitmap ImageSource
        {
            get { return (WriteableBitmap)GetValue(ImageSourceProperty); }
            set { SetValue(ImageSourceProperty, value);  }
        }

        // Using a DependencyProperty as the backing store for ImageSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ImageSourceProperty =
            DependencyProperty.Register("ImageSource",
                typeof(WriteableBitmap),
                typeof(ImageView),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, ImageSourceChangedCallback));

        private static void ImageSourceChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var u = d as ImageView;
            u.img1.Source = (WriteableBitmap)e.NewValue;
            u.matrix.Matrix = new Matrix(1, 0, 0, 1, 0, 0);
        }

        //恢复原始大小
        private void Recover(object sender, RoutedEventArgs e)
        {
            matrix.Matrix = new Matrix(1, 0, 0, 1, 0, 0);
        }

        private bool isMouseLeftButtonDown = false;

        private Point previousMousePoint;
        private Point position;

        //======================================
        //拖动
        private void Img_MouseDown1(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Released)
            {
                return;
            }
            isMouseLeftButtonDown = true;
            previousMousePoint = e.GetPosition(mainBox1);
        }

        private void Img_MouseUp1(object sender, MouseButtonEventArgs e)
        {
            isMouseLeftButtonDown = false;
        }

        private void Img_MouseLeave1(object sender, MouseEventArgs e)
        {
            isMouseLeftButtonDown = false;
        }

        private void Img_MouseMove1(object sender, MouseEventArgs e)
        {
            position = e.GetPosition(mainBox1);
            if (isMouseLeftButtonDown == true)
            {
                //tlt1.X +=
                //tlt1.Y +=

                double offX = position.X - previousMousePoint.X;
                double offY = position.Y - previousMousePoint.Y;
                //变换矩阵
                var newMatrix = new Matrix(1, 0, 0, 1, offX, offY);

                matrix.Matrix = newMatrix * matrix.Matrix;
            }
        }

        private void RecoverMatrix(object sender, MouseButtonEventArgs e)
        {
            matrix.Matrix = new Matrix(1, 0, 0, 1, 0, 0);
        }
    }
}