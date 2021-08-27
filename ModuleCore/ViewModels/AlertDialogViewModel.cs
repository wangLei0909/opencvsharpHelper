using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Media;

namespace ModuleCore.ViewModels
{
    public class AlertDialogViewModel : BindableBase, IDialogAware
    {
        private DelegateCommand<string> _closeDialogCommand;

        public DelegateCommand<string> CloseDialogCommand =>
                _closeDialogCommand ??= new DelegateCommand<string>(ExecuteCloseDialogCommand);

        private void ExecuteCloseDialogCommand(string parameter)
        {
            ButtonResult result = ButtonResult.None;
            if (parameter?.ToLower() == "true")
                result = ButtonResult.Yes;
            else if (parameter?.ToLower() == "false")
                result = ButtonResult.No;
            RaiseRequestClose(new DialogResult(result));
        }

        private Uri imageName = new ("pack://application:,,,/ModuleCore;Component/Images/success.png");

        public Uri ImageName
        {
            get { return imageName; }
            set { SetProperty(ref imageName, value); }
        }

        //触发窗体关闭事件
        public virtual void RaiseRequestClose(IDialogResult dialogResult)
        {
            RequestClose?.Invoke(dialogResult);
        }

        private string _message;

        public string Message
        {
            get { return _message; }
            set { SetProperty(ref _message, value); }
        }

        private string messageType;

        public string MessageType
        {
            get { return messageType; }
            set { SetProperty(ref messageType, value); }
        }

        private string _title = "Notification";

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
            Source.Add(new Button() { Content = "确认", Background = Brushes.Green, Command = CloseDialogCommand, CommandParameter = "true" });

            Message = parameters.GetValue<string>("message");
            var msg = Message.Split(':');
            Title = msg[0];

            ImageName = Title switch
            {
                "message" => new Uri("pack://application:,,,/ModuleCore;Component/Images/success.png"),
                _ => new Uri("pack://application:,,,/ModuleCore;Component/Images/alter.png"),
            };

            if (Title == "choose")
                Source.Add(new Button() { Content = "取消", Background = Brushes.Red, Command = CloseDialogCommand, CommandParameter = "false" });
        }

        public ObservableCollection<Button> Source { get; set; } = new ObservableCollection<Button>();
    }
}