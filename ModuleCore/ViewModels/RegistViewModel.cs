using ModuleCore.Services;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using ModuleCore.Common;
using System;
using System.Data;

namespace ModuleCore.ViewModels
{
    public class RegistViewModel : BindableBase, IDialogAware
    {
        private DelegateCommand<string> _closeDialogCommand;

        public DelegateCommand<string> CloseDialogCommand =>
                _closeDialogCommand ??= new DelegateCommand<string>(ExecuteCloseDialogCommand);

        private void ExecuteCloseDialogCommand(string parameter)
        {
            ButtonResult result = ButtonResult.None;

            if (parameter?.ToLower() == "true")
            {
                result = ButtonResult.Yes;
                if (RegistCode.Length < 1) return;
                dt = new DataTable();
                dt.Columns.Add("RegistCode", Type.GetType("System.String"));
                DataRow dr = dt.NewRow();
                dt.Rows.Add(dr);
                dt.Rows[0]["RegistCode"] = RegistCode;
                JsonService.DataTableToFile("Impower.json", dt);
            }
            else if (parameter?.ToLower() == "false")
                result = ButtonResult.No;

            RaiseRequestClose(new DialogResult(result));
        }

        private DataTable dt;

        //窗体关闭
        public virtual void RaiseRequestClose(IDialogResult dialogResult)
        {
            RequestClose?.Invoke(dialogResult);
        }

        private string _title = "授权";

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

        private string _CPUID;

        public string CPUID
        {
            get { return _CPUID; }
            set { SetProperty(ref _CPUID, value); }
        }

        //窗体打开
        public void OnDialogOpened(IDialogParameters parameters)
        {
            CPUID = HardWare.GetCpuId();
        }

        private string _RegistCode = "";

        public string RegistCode
        {
            get { return _RegistCode; }
            set { SetProperty(ref _RegistCode, value); }
        }
    }
}