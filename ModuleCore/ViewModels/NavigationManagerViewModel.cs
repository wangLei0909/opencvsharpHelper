using ModuleCore.Mvvm;
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
using System.Windows.Controls;
using System.Windows.Input;

namespace ModuleCore.ViewModels
{
    public class NavigationManagerViewModel : BindableBase 
    {

        public NavigateModel Navigate { get; set; }
        public NavigationManagerViewModel(IContainerExtension container)
        {
            Navigate = container.Resolve<NavigateModel>();
        }
 
 
    }
}