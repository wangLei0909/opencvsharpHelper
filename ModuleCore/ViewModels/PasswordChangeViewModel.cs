using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;

namespace ModuleCore.ViewModels
{
    public class PasswordChangeViewModel : BindableBase, IDialogAware
    {
        private string _Name;

        public event Action<IDialogResult> RequestClose;

        public string Name
        {
            get { return _Name; }
            set { SetProperty(ref _Name, value); }
        }

        public string Title { get { return $" 正在修改用户: {Name} 的密码"; } }

        public bool CanCloseDialog()
        {
            return true;
        }

        public void Close()
        {
            RequestClose?.Invoke(new DialogResult(ButtonResult.Yes));
        }

        public void OnDialogClosed()
        {
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            Name = parameters.GetValue<string>("name");
        }

    }
}