using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

// 转自 https://www.cnblogs.com/lekko/archive/2012/07/23/2604257.html
namespace ModuleCore.UserControls
{
    public partial class RangeSlider : UserControl
    {
        #region 私有变量

        private static readonly int _width = 150;  // 拖动条初始宽度
        private static readonly int _height = 30;  // 高度
        private static readonly int _freq = 1;    // 出现刻度的间距

        #endregion 私有变量

        public RangeSlider()
        {
            InitializeComponent();
        }

        #region 私有属性

        /// <summary>
        /// 裁剪矩阵（头）
        /// </summary>
        private Rect StartRect
        {
            // get { return (Rect)GetValue(StartRectProperty); }
            set { SetValue(StartRectProperty, value); }
        }

        private static readonly DependencyProperty StartRectProperty =
            DependencyProperty.Register("StartRect", typeof(Rect), typeof(RangeSlider));

        /// <summary>
        /// 裁剪矩阵（尾）
        /// </summary>
        private Rect EndRect
        {
            //  get { return (Rect)GetValue(EndRectProperty); }
            set { SetValue(EndRectProperty, value); }
        }

        private static readonly DependencyProperty EndRectProperty =
            DependencyProperty.Register("EndRect", typeof(Rect), typeof(RangeSlider));

        #endregion 私有属性

        #region 公开依赖属性

        /// <summary>
        /// 刻度间距，默认为10
        /// </summary>
        public int SliderTickFrequency
        {
            get { return (int)GetValue(SliderTickFrequencyProperty); }
            set { SetValue(SliderTickFrequencyProperty, value); }
        }

        public static readonly DependencyProperty SliderTickFrequencyProperty =
            DependencyProperty.Register("SliderTickFrequency", typeof(int), typeof(RangeSlider), new PropertyMetadata(_freq));

        /// <summary>
        /// 控件高度，默认为30
        /// </summary>
        public int SilderHeight
        {
            get { return (int)GetValue(SilderHeightProperty); }
            set { SetValue(SilderHeightProperty, value); }
        }

        public static readonly DependencyProperty SilderHeightProperty =
            DependencyProperty.Register("SilderHeight", typeof(int), typeof(RangeSlider), new PropertyMetadata(_height));

        /// <summary>
        /// 拖动条宽度，默认为150
        /// </summary>
        public int SilderWidth
        {
            get { return (int)GetValue(SilderWidthProperty); }
            set { SetValue(SilderWidthProperty, value); }
        }

        public static readonly DependencyProperty SilderWidthProperty =
            DependencyProperty.Register("SilderWidth", typeof(int), typeof(RangeSlider), new PropertyMetadata(_width));

        /// <summary>
        /// 最小值，默认为0
        /// </summary>
        public int Minimum
        {
            get { return (int)GetValue(MinimumProperty); }
            set { SetValue(MinimumProperty, value); }
        }

        public static readonly DependencyProperty MinimumProperty =
            DependencyProperty.Register("Minimum", typeof(int), typeof(RangeSlider),
                new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.AffectsRender)
                );

        /// <summary>
        /// 最大值，默认为100
        /// </summary>
        public int Maximum
        {
            get { return (int)GetValue(MaximumProperty); }
            set { SetValue(MaximumProperty, value); }
        }

        public static readonly DependencyProperty MaximumProperty =
            DependencyProperty.Register("Maximum", 
                typeof(int), typeof(RangeSlider),
                new FrameworkPropertyMetadata(100, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.AffectsRender, MaximumChangedCallback));
        private static void MaximumChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var u = d as RangeSlider;
            if (u.EndValue > u.Maximum)
                u.EndValue = u.Maximum;
            u.ClipSilder();
        }
        /// <summary>
        /// 选中开始值，默认为0
        /// </summary>
        public int StartValue
        {
            get { return (int)GetValue(StartValueProperty); }
            set { SetValue(StartValueProperty, value); }
        }

        public static readonly DependencyProperty StartValueProperty =
            DependencyProperty.Register("StartValue", typeof(int), typeof(RangeSlider), new FrameworkPropertyMetadata(0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.AffectsRender));

        /// <summary>
        /// 选中结束值，默认为100
        /// </summary>
        public int EndValue
        {
            get { return (int)GetValue(EndValueProperty); }
            set { SetValue(EndValueProperty, value); }
        }

        public static readonly DependencyProperty EndValueProperty =
            DependencyProperty.Register("EndValue", typeof(int), typeof(RangeSlider), new FrameworkPropertyMetadata(100, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault | FrameworkPropertyMetadataOptions.AffectsRender));

        #endregion 公开依赖属性

        #region 前台交互

        /// <summary>
        /// 对两个拖动条进行裁剪
        /// </summary>
        private void ClipSilder()
        {
            int selectedValue = EndValue - StartValue;
            int totalValue = Maximum - Minimum;
            double sliderClipWidth = SilderWidth * (StartValue - Minimum + selectedValue / 2) / totalValue;
            sliderClipWidth = sliderClipWidth < 0 ? 0 : sliderClipWidth;
            // 对第一个拖动条进行裁剪
            StartRect = new Rect(0, 0, sliderClipWidth, SilderHeight);
            // 对第二个拖动条进行裁剪
            EndRect = new Rect(sliderClipWidth, 0, SilderWidth, SilderHeight);
        }

        /// <summary>
        /// 初始化裁剪
        /// </summary>
        private void UC_Arrange_Loaded(object sender, RoutedEventArgs e)
        {
            ClipSilder();
        }

        private void SL_Bat1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (e.NewValue > EndValue)    // 检查值范围
                StartValue = EndValue;    // 超出，重设为最大值
            ClipSilder();
        }

        private void SL_Bat2_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (e.NewValue < StartValue)
                EndValue = StartValue;
            ClipSilder();
        }

        #endregion 前台交互
    }
}