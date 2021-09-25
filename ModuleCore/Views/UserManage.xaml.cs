using ModuleCore.Common;
using Prism.Ioc;
using ModuleCore.Models;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using ModuleCore.Common.Authority;

namespace ModuleCore.Views
{
    /// <summary>
    /// AlertDialog.xaml 的交互逻辑
    /// </summary>
    public partial class UserManage : UserControl
    {
        public UserManage(IContainerExtension container)
        {
            Model = container.Resolve<LoginModel>();
            InitializeComponent();
        }
        public LoginModel Model { get; set; }

        private void AddUser(object sender, System.Windows.RoutedEventArgs e)
        {
            if (!isEqual) return;

            var checkUser = Model.UserList.Where(u => u.Name == UserName.Text);
            if (checkUser.Any())
            {
                MessageBox.Show("添加失败，用户已经存在");

                return;
            }

            Model.AddUser(new(UserName.Text, passwordBoxFirst.Password, (Authority)Model.AuthoritySelect));
            MessageBox.Show("添加成功");

        }
        private bool isEqual;
        private void CheckPassword(object sender, System.Windows.RoutedEventArgs e)
        {
            isEqual = passwordBoxFirst.Password == passwordBoxSecond.Password && passwordBoxFirst.Password.Length > 0;

            if (!(passwordBoxFirst.Password == passwordBoxSecond.Password))
                textBlockMsg.Text = "两次输入不一致";
            if (!(passwordBoxFirst.Password.Length > 0))
                textBlockMsg.Text = "密码不能为空";
            if (isEqual)
                textBlockMsg.Text = "";
        }
    }
}