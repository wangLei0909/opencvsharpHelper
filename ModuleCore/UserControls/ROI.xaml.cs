using System;
using System.Windows;
using System.Windows.Controls;

namespace ModuleCore.UserControls
{
    /// <summary>
    /// UserControlChoose.xaml 的交互逻辑
    /// </summary>
    public partial class ROI : UserControl
    {
        public ROI()
        {
            InitializeComponent();
        }

        public double CenterY
        {
            get { return (double)GetValue(CenterYProperty); }
            set { SetValue(CenterYProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Top.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CenterYProperty =
            DependencyProperty.Register("CenterY", typeof(double), typeof(RotateRectROI), new PropertyMetadata(50d));

        public double CenterX
        {
            get { return (double)GetValue(CenterXProperty); }
            set { SetValue(CenterXProperty, value); }
        }

        public static readonly DependencyProperty CenterXProperty =
            DependencyProperty.Register("CenterX", typeof(double), typeof(RotateRectROI), new PropertyMetadata(50d));

        public double RectWidth
        {
            get { return (double)GetValue(RectWidthProperty); }
            set { SetValue(RectWidthProperty, value); }
        }

        // Using a DependencyProperty as the backing store for RRWidth.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RectWidthProperty =
            DependencyProperty.Register("RectWidth", typeof(double), typeof(RotateRectROI), new PropertyMetadata(100d));

        public double RectHeight
        {
            get { return (double)GetValue(RectHeightProperty); }
            set { SetValue(RectHeightProperty, value); }
        }

        // Using a DependencyProperty as the backing store for RRHeig.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RectHeightProperty =
            DependencyProperty.Register("RectHeight", typeof(double), typeof(RotateRectROI), new PropertyMetadata(100d));

        public double RectAngle
        {
            get { return (double)GetValue(RectAngleProperty); }
            set { SetValue(RectAngleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for RectAngle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RectAngleProperty =
            DependencyProperty.Register("RectAngle", typeof(double), typeof(RotateRectROI), new PropertyMetadata(0d));

        private void RemoveMe(object sender, RoutedEventArgs e)
        {
            RemoveEvent?.Invoke(this);
        }

        public event Action<UserControl> RemoveEvent;
    }
}