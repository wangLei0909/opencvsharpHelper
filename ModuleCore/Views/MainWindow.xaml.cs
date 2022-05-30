
using MaterialDesignThemes.Wpf;

using Prism.Regions;

using System.Windows;
using System.Windows.Input;

namespace ModuleCore.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow(IRegionManager regionManager)
        {
            InitializeComponent();
            RegionManager.SetRegionManager(ContentRegionCore, regionManager);
            this.Height = SystemParameters.WorkArea.Height - 100;
            this.Width = SystemParameters.WorkArea.Width - 100;
            Left = 50;
            Top = 50;
        }

        private void BtnClose(object sender, RoutedEventArgs e)
        {

            Application.Current.Shutdown();
        }

        private void BtnMin(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        #region 标题栏事件

        private void Border_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            //如果已经最大化了，就不响应标题栏拖拽
            if (WindowState == WindowState.Maximized)
                return;
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        //双击标题栏事件
        private void Border_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {

                WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
                btnNormalIcon.Kind = WindowState == WindowState.Maximized ? PackIconKind.WindowRestore : PackIconKind.WindowMaximize;
            }
        }

        #endregion 标题栏事件


        /// <summary>
        /// 最大化
        /// </summary>
        private void BtnMaximize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
            btnNormalIcon.Kind = WindowState == WindowState.Maximized ? PackIconKind.WindowRestore : PackIconKind.WindowMaximize;
        }

    }
}