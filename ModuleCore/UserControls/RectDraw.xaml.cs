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
    public partial class RectDraw : UserControl
    {
        public RectDraw()
        {
            InitializeComponent();
        }

        public int RectWidth
        {
            get { return (int)GetValue(RectWidthProperty); }
            set { SetValue(RectWidthProperty, value); }
        }

        public static readonly DependencyProperty RectWidthProperty =
            DependencyProperty.Register("RectWidth", typeof(int), typeof(RectDraw),
                new FrameworkPropertyMetadata(200, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, RectWidthChangedCallback));

        private static void RectWidthChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // Width 改变时  位置调整块的宽度
            var u = d as RectDraw;
            u.thumb_Move.Width = (int)e.NewValue;

          
        }

        public int RectHeight
        {
            get { return (int)GetValue(RectHeightProperty); }
            set { SetValue(RectHeightProperty, value); }
        }

        public static readonly DependencyProperty RectHeightProperty =
            DependencyProperty.Register("RectHeight", typeof(int), typeof(RectDraw),
                new FrameworkPropertyMetadata(200, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, RectHeightChangedCallback));

        private static void RectHeightChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // Height 改变时 位置调整块的高度
            var u = d as RectDraw;
            u.thumb_Move.Height = (int)e.NewValue;
      
        }



    }
}