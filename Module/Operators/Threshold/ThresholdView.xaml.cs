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
    /// ThreshTool.xaml 的交互逻辑
    /// </summary>
    public partial class ThresholdView : UserControl
    {
        public ThresholdView()
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
    }
}
