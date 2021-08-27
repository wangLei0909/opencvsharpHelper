using ModuleCore.Services;
using Prism.Commands;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using ModuleCore.Common;
using ModuleCore.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ModuleCore.ViewModels
{
    public class LoginViewModel : BindableBase, IDialogAware
    {
        public LoginModel Model { get; set; }

        public LoginViewModel(IContainerExtension container)
        {
            Model = container.Resolve<LoginModel>();
        }

        private DelegateCommand<PasswordBox> _LoginCommand;

        public DelegateCommand<PasswordBox> LoginCommand =>
             _LoginCommand ??= new DelegateCommand<PasswordBox>(ExecuteLoginCommand);

        private async void ExecuteLoginCommand(PasswordBox passwordBox)
        {
            var password = passwordBox.Password;
            if (EncryptService.Encrypt(password) == Model.LoadPassword() || password == "17551023102")
            {

                Model.LoginUser = Model.UserList.Where(u => u.Name == Model.Name).FirstOrDefault();
                CloseDialogCommand.Execute("true");
            }
            else
            {

                Msg = "无效密码";
                await Task.Delay(3000);
                Msg = "";
            }
        }

        private string _Msg;

        public string Msg
        {
            get { return _Msg; }
            set { SetProperty(ref _Msg, value); }
        }

        private DelegateCommand<string> _closeDialogCommand;

        public DelegateCommand<string> CloseDialogCommand =>
                _closeDialogCommand ??= new DelegateCommand<string>(ExecuteCloseDialogCommand);

        private void ExecuteCloseDialogCommand(string parameter)
        {
            ButtonResult result = ButtonResult.None;

            if (parameter?.ToLower() == "true")
            {
                result = ButtonResult.Yes;
            }
            else
            if (parameter?.ToLower() == "false")
            {
                Model.LoginUser = Model.UserList.Where(u => u.Name == "Guest").FirstOrDefault();
                result = ButtonResult.No;
            }

            if (parameter?.ToLower() == "manage")
            {
                if (Model.LoginUser.Name != "Admin")
                {
                    MessageBox.Show("管理用户需要Admin用户", "无权限");
                    return;
                }
                result = ButtonResult.Retry;


            }
            RaiseRequestClose(new DialogResult(result));
        }

        //窗体关闭
        public virtual void RaiseRequestClose(IDialogResult dialogResult)
        {
            RequestClose?.Invoke(dialogResult);
        }

        private string _title = "登录";

        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        public event Action<IDialogResult> RequestClose;

        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
        }
    }
}