using ModuleCore.Mvvm;
using ModuleCore.Services;
using Prism.Commands;
using Prism.Events;
using Prism.Ioc;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;
using ModuleCore.Common;
using ModuleCore.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using ModuleCore.Common.Authority;

namespace ModuleCore.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private string _title = "Prism";

        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        private readonly IDialogService _dialogService;
        public LoginModel Model { get; set; }
        public NavigateModel Navigate { get; set; }
        private readonly IEventAggregator _eventAggregator;

        public MainWindowViewModel(IDialogService dialogService,
                                    IRegionManager regionManager,
                                    IContainerExtension container)
        {
   
            _regionManager = regionManager;
           
             _dialogService = dialogService;

            Model = container.Resolve<LoginModel>();
            Navigate = container.Resolve<NavigateModel>();

            _eventAggregator = container.Resolve<IEventAggregator>();

            //注册发送给errLog的消息
            _eventAggregator.GetEvent<MessageEvent>().Subscribe(
                MessageReceived,
                ThreadOption.UIThread,
                false,
                (filter) => filter.Target.Contains("errLog"));

            LoadDefaultView();
        }

        public void IsAdmin(object sender, CanExecuteRoutedEventArgs e)
        {
            if ((int)Model.LoginUser.Authority >= 2)
            {
                e.CanExecute = true;
            }
            else
            {
                e.CanExecute = false;
            }
            //避免事件继续向上传递而降低程序性能
            e.Handled = true;
        }

        private void MessageReceived(Message message)
        {
            var msg = message.Content;
            AddErr(new ErrModel() { ErrMsg = msg });
        }

        private NavigateItem _NavigateTarget;

        public NavigateItem NavigateTarget
        {
            get { return _NavigateTarget; }
            set
            {
                SetProperty(ref _NavigateTarget, value);
                if (value is not null)
                    NavigateCommand.Execute(value.ViewName);
            }
        }

        /// <summary>
        /// 延迟加载默认工作视图
        /// </summary>
        private async void LoadDefaultView()
        {
            await Task.Delay(1000);
            ViewDisplayLoad();
           var defaultView = Navigate.DefaultView;
            NavigateCommand.Execute(defaultView);
        }
        private DataTable dt;
        private List<string> ShowList = new();
        private void ViewDisplayLoad()
        {
            dt = JsonService.DataTableFromFile("./Config/ViewConfig.json");
            if (dt == null)
            {
                return;
            }
            else
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    var viewname = dt.Rows[i]["ViewName"].ToString();
                    if (!ShowList.Contains(viewname))
                    {
                        ShowList.Add(viewname);
                    }
                }

                foreach (var item in Navigate.NavigateList)
                {
                    if (ShowList.Contains(item.ViewName))
                    {
                        item.Display = true;
                    }
                    else
                    {
                        item.Display = false;
                    }
                }
                ShowNavigateMenu(Model.LoginUser.Authority);
            }
        }

        private DelegateCommand<string> _NavigateCommand;

        public DelegateCommand<string> NavigateCommand =>
             _NavigateCommand ??= new DelegateCommand<string>(ExecuteNavigateCommand);

        private void ExecuteNavigateCommand(string navigatePath)
        {
            if (string.IsNullOrEmpty(navigatePath))
                return;

            //设置时检查权限
            if (navigatePath == "Setting" && (int)Model.LoginUser.Authority < 2)
            {
                AddErr(new ErrModel() { ErrMsg = "请登录管理员权限" });
                return;
            }
      
            _regionManager.RequestNavigate("ContentRegionCore", navigatePath);
        }

        //导航
        private readonly IRegionManager _regionManager;

        //对话框
        private DelegateCommand _AboutDialog;

        public DelegateCommand AboutDialog =>
            _AboutDialog ??= new DelegateCommand(ExecuteAboutDialog);

        private void ExecuteAboutDialog()
        {
            _dialogService.ShowDialog("AlertDialog", new DialogParameters($"message={"message:QQ123211521"}"), r =>
            {
                if (r.Result == ButtonResult.Yes)
                    Title = "YIJI";
            });
        }

        private ObservableCollection<ErrModel> _Errors = new();

        public ObservableCollection<ErrModel> Errors
        {
            get { return _Errors; }
            set { SetProperty(ref _Errors, value); }
        }

        /// <summary>
        /// AddErr( new ErrModel() { ErrMsg = "ErrMsg" }); 添加浮动报警
        /// </summary>
        /// <param name="err"></param>
        public void AddErr(ErrModel err)
        {
            Errors.Add(err);
            err.Confirmed += ClearConfirmed;
        }

        private void ClearConfirmed()
        {
            System.Windows.Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(() =>
            {
                var re = Errors.Where(x => x.ErrMsg.Length == 0).FirstOrDefault();
                Errors.Remove(re);
            }));
        }

        private DelegateCommand _Login;

        public DelegateCommand Login =>
             _Login ??= new DelegateCommand(ExecuteLogin);

        private void ExecuteLogin()
        {
            _dialogService.ShowDialog("LoginView", new DialogParameters($"message={"message:QQ123211521"}"), r =>
            {
                if (r.Result == ButtonResult.Yes)
                {
                    ShowNavigateMenu(Model.LoginUser.Authority);
                }

                if (r.Result == ButtonResult.Retry)
                {
                    _dialogService.ShowDialog("UserManage", new DialogParameters($"message={"message:QQ123211521"}"), r => { });
                }
            });
        }

        private void ShowNavigateMenu(Authority authority)
        {
            Navigate.NavigateShowList.Clear();

            foreach (var item in Navigate.NavigateList)
            {
                if (item.UserLevel <= (int)authority && item.Display)
                    Navigate.NavigateShowList.Add(item);
            }
        }
    }
}