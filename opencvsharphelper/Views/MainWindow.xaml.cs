using Prism.Ioc;
using Prism.Regions;
using System.Windows;

namespace opencvsharphelper.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(IContainerExtension container)
        {

            InitializeComponent();

            LoadMain(container);
        }

        async void LoadMain(IContainerExtension container)
        {

            await System.Threading.Tasks.Task.Delay(2000);

            var main = container.Resolve<ModuleCore.Views.MainWindow>();

            main.Show();

            this.Close();
        }
    }
}
