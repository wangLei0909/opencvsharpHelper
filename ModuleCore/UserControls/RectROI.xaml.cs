using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace ModuleCore.UserControls
{
    /// <summary>
    /// UserControlChoose.xaml 的交互逻辑
    /// </summary>
    public partial class RectROI : UserControl
    {
        public RectROI()
        {
            InitializeComponent();
        }

        //调整位置时
        private void Move_DragDelta(object sender, DragDeltaEventArgs e)
        {
            //原始位置
            FrameworkElement designerItem = sender as FrameworkElement;
            double left = Canvas.GetLeft(designerItem);
            double top = Canvas.GetTop(designerItem);

            //调整后位置
            double X = left + e.HorizontalChange;
            double Y = top + e.VerticalChange;
            //最大位置
            var parent = VisualTreeHelper.GetParent(this) as FrameworkElement;
            var maxX = parent.ActualWidth - designerItem.ActualWidth;
            var maxY = parent.ActualHeight - designerItem.ActualHeight;

            X = X > maxX ? maxX : X;
            Y = Y > maxY ? maxY : Y;

            //不能超出左上角
            X = X < 0 ? 0 : X;
            Y = Y < 0 ? 0 : Y;

            RectLeft = (int)X;
            RectTop = (int)Y;
        }

        // 组件最小尺寸
        private readonly double obj_minesize = 30;

        // 调整大小时
        private void Size_DragDelta(object sender, DragDeltaEventArgs e)
        {
            FrameworkElement designerItem = e.Source as FrameworkElement;

            //原本的大小
            double left = Canvas.GetLeft(designerItem);
            double top = Canvas.GetTop(designerItem);

            //调整后的大小
            double X = left + e.HorizontalChange;
            double Y = top + e.VerticalChange;

            //最小尺寸
            double minL = Canvas.GetLeft(thumb_Move) + obj_minesize;
            double minT = Canvas.GetTop(thumb_Move) + obj_minesize;
            X = X < minL ? minL : X;
            Y = Y < minT ? minT : Y;

            //最大尺寸
            var parent = VisualTreeHelper.GetParent(this) as FrameworkElement;
            var maxX = parent.ActualWidth - designerItem.ActualWidth;
            var maxY = parent.ActualHeight - designerItem.ActualHeight;

            X = X > maxX ? maxX : X;
            Y = Y > maxY ? maxY : Y;

            RectWidth = (int)(X - Canvas.GetLeft(thumb_Move));
            RectHeight = (int)(Y - Canvas.GetTop(thumb_Move));
        }

        public int RectTop
        {
            get { return (int)GetValue(RectTopProperty); }
            set { SetValue(RectTopProperty, value); }
        }

        public static readonly DependencyProperty RectTopProperty =
            DependencyProperty.Register("RectTop", typeof(int), typeof(RectROI),
                new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, RectTopChangedCallback));

        private static void RectTopChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //top改变时
            var u = d as RectROI;
            Canvas.SetTop(u.thumb_Size, (int)e.NewValue + u.thumb_Move.Height - u.thumb_Size.Height / 2);
            Canvas.SetTop(u.thumb_Move, (int)e.NewValue);
            Canvas.SetTop(u.label, (int)e.NewValue);
        }

        public int RectLeft
        {
            get { return (int)GetValue(RectLeftProperty); }
            set { SetValue(RectLeftProperty, value); }
        }

        public static readonly DependencyProperty RectLeftProperty =
            DependencyProperty.RegisterAttached("RectLeft", typeof(int), typeof(RectROI),
                new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, RectLeftChangedCallback));

        private static void RectLeftChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //left改变时
            var u = d as RectROI;
            // thumb_Size
            Canvas.SetLeft(u.thumb_Size, (int)e.NewValue + u.thumb_Move.Width - u.thumb_Size.Width / 2);
            Canvas.SetLeft(u.thumb_Move, (int)e.NewValue);
            Canvas.SetLeft(u.label, (int)e.NewValue);
        }

        public int RectWidth
        {
            get { return (int)GetValue(RectWidthProperty); }
            set { SetValue(RectWidthProperty, value); }
        }

        public static readonly DependencyProperty RectWidthProperty =
            DependencyProperty.Register("RectWidth", typeof(int), typeof(RectROI),
                new FrameworkPropertyMetadata(200, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, RectWidthChangedCallback));

        private static void RectWidthChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // Width 改变时  位置调整块的宽度
            var u = d as RectROI;
            u.thumb_Move.Width = (int)e.NewValue;

            //大小调整块的位置
            var left = Canvas.GetLeft(u.thumb_Move) + (int)e.NewValue;
            Canvas.SetLeft(u.thumb_Size, left-u.thumb_Size.Width/2);
        }

        public int RectHeight
        {
            get { return (int)GetValue(RectHeightProperty); }
            set { SetValue(RectHeightProperty, value); }
        }

        public static readonly DependencyProperty RectHeightProperty =
            DependencyProperty.Register("RectHeight", typeof(int), typeof(RectROI),
                new FrameworkPropertyMetadata(200, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, RectHeightChangedCallback));

        private static void RectHeightChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // Height 改变时 位置调整块的高度
            var u = d as RectROI;
            u.thumb_Move.Height = (int)e.NewValue;
            //大小调整块的位置
            var top = Canvas.GetTop(u.thumb_Move) + (int)e.NewValue;

            Canvas.SetTop(u.thumb_Size, top-u.thumb_Size.Height / 2);
        }

        //框的标题
        public string Lable
        {
            get { return (string)GetValue(LableProperty); }
            set { SetValue(LableProperty, value); }
        }

        public static readonly DependencyProperty LableProperty =
            DependencyProperty.Register("Lable", typeof(string), typeof(RectROI),
                new FrameworkPropertyMetadata("0", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, LableCallback));

        private static void LableCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var u = d as RectROI;
            if (e.NewValue is not null)
                u.label.Text = e.NewValue.ToString();
        }

        //尺寸调整块的大小
 
        public int ThumdSize
        {
            get { return (int)GetValue(ThumdSizeProperty); }
            set { SetValue(ThumdSizeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ThumdHeight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ThumdSizeProperty =
            DependencyProperty.Register("ThumdSize", typeof(int), typeof(RectROI),
                       new FrameworkPropertyMetadata(20, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, ThumdSizeChangedCallback));

        private static void ThumdSizeChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var u = d as RectROI;
            double left = Canvas.GetLeft(u.thumb_Move);
            double top = Canvas.GetTop(u.thumb_Move);


            int newValue = (int)e.NewValue;
            u.thumb_Size.Height = newValue;
            u.thumb_Size.Width = newValue;
            Canvas.SetLeft(u.thumb_Size, left + u.thumb_Move.ActualWidth - u.thumb_Size.Width / 2);
            Canvas.SetTop(u.thumb_Size, top+ u.thumb_Move.ActualHeight - u.thumb_Size.Height / 2);
        }


        private void RemoveMe(object sender, RoutedEventArgs e)
        {
            RemoveEvent?.Invoke(this);
        }

        public event Action<UserControl> RemoveEvent;
    }
}