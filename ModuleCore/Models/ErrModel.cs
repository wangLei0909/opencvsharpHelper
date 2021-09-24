using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Threading.Tasks;

namespace ModuleCore.Models
{
    public class ErrModel : BindableBase
    {
        public ErrModel()
        {
            AutoConfirm();
        }

        private async void AutoConfirm()
        {
            for (int i = 10; i >= 0; i--)
            {
                ConfirmTime = "确认" + i;
                await Task.Delay(1000);
            }

            ExecuteConfirm();
        }

        private string _ConfirmTime;

        public string ConfirmTime
        {
            get { return _ConfirmTime; }
            set { SetProperty(ref _ConfirmTime, value); }
        }

        private string _ErrMsg;

        public string ErrMsg
        {
            get { return _ErrMsg; }
            set { SetProperty(ref _ErrMsg, value); }
        }

        private DelegateCommand _Confirm;

        public DelegateCommand Confirm =>
             _Confirm ??= new DelegateCommand(ExecuteConfirm);

        private void ExecuteConfirm()
        {
            ErrMsg = "";
            Confirmed?.Invoke();
        }

        public event Action Confirmed;
    }
}