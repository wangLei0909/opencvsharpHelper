using ModuleCore.Services;
using Prism.Ioc;
using ModuleCore.Common;
using ModuleCore.Models;
using ModuleCore.ViewModels;
using System;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace ModuleCore.Views
{
    /// <summary>
    /// AlertDialog.xaml 的交互逻辑
    /// </summary>
    public partial class PasswordChange : UserControl
    {

        public LoginModel Model { get; set; }

        public PasswordChange(IContainerExtension container)
        {
            Model = container.Resolve<LoginModel>();
            InitializeComponent();
        }
        private void ChangePassword(object sender, System.Windows.RoutedEventArgs e)
        {
            if (isEqual)
            {
                SavePassword(passwordBoxSecond.Password);
            }
            if (!(passwordBoxFirst.Password == passwordBoxSecond.Password))
                textBlockMsg.Text = "两次输入不一致";
            if (!(passwordBoxFirst.Password.Length > 0))
                textBlockMsg.Text = "密码不能为空";
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


        public void SavePassword(string password)
        {
            if (!isEqual) return;
            string passwordCryptic = EncryptService.Encrypt(password);
            var user = Model.UserList.Where(u => u.Name == UserName.Text).FirstOrDefault();
            user.Password = passwordCryptic;
            Model.SaveUsers();
            var viewModel = DataContext as PasswordChangeViewModel;
            MessageBox.Show("修改成功", "成功");
            viewModel.Close();
        }
    }
}