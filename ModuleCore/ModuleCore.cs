using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModuleCore.Models;
using ModuleCore.Mvvm;
using ModuleCore.Views;
using Prism.Ioc;
using Prism.Modularity;

namespace ModuleCore
{
    public class CoreModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
            if (!Directory.Exists("./Config/")) Directory.CreateDirectory("./Config/");
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
  
            //注册导航菜单
            _ = containerRegistry.RegisterSingleton<NavigateModel>();

            //注册权限系统
            _ = containerRegistry.RegisterSingleton<LoginModel>();
            //注册窗体
            containerRegistry.RegisterDialog<AlertDialog, ViewModels.AlertDialogViewModel>();
            containerRegistry.RegisterDialog<RegistView, ViewModels.RegistViewModel>();
            containerRegistry.RegisterDialog<LoginView, ViewModels.LoginViewModel>();
            containerRegistry.RegisterDialog<UserManage, ViewModels.UserManageViewModel>();
            containerRegistry.RegisterDialog<PasswordChange, ViewModels.PasswordChangeViewModel>();
            //注入导航
            containerRegistry.RegisterForNavigation<Setting>();
        }
    }
}
