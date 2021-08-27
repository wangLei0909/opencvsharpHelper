using Prism.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace OpencvsharpModule.Views
{
    /// <summary>
    /// YIJICamera.xaml 的交互逻辑
    /// </summary>
    public partial class CameraView : UserControl
    {
        public CameraView()
        {
            InitializeComponent();
 
        }

        async private void LeftHide(object sender, RoutedEventArgs e)
        {
            for (int i = 10; i <= 40; i++)
            {
                await Task.Delay(10);
                var leftLength = 50 - 3 * i;
                var rightLenth = 50 + 3 * i;
                if (leftLength < 10) leftLength = 10;
                if (rightLenth > 90) rightLenth = 90;
                LeftImageView.Width = new GridLength(leftLength, GridUnitType.Star);
                RightImageView.Width = new GridLength(rightLenth, GridUnitType.Star);
            }
        }

        async private void RightHide(object sender, RoutedEventArgs e)
        {
            for (int i = 10; i <= 40; i++)
            {
                await Task.Delay(10);
                var leftLength = 50 + 3 * i;
                var rightLenth = 50 - 3 * i;
                if (leftLength > 90) leftLength = 90;
                if (rightLenth < 10) rightLenth = 10;

                LeftImageView.Width = new GridLength(leftLength, GridUnitType.Star);
                RightImageView.Width = new GridLength(rightLenth, GridUnitType.Star);
            }
        }

        private void BothShow(object sender, RoutedEventArgs e)
        {

            LeftImageView.Width = new GridLength(50, GridUnitType.Star);
            RightImageView.Width = new GridLength(50, GridUnitType.Star);

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
