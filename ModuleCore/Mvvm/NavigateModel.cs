using Prism.Mvvm;
using System.ComponentModel;

namespace ModuleCore.Mvvm
{
    public class NavigateModel : BindableBase
    {
        private BindingList<NavigateItem> _NavigateList = new();

        public BindingList<NavigateItem> NavigateList
        {
            get { return _NavigateList; }
            set { SetProperty(ref _NavigateList, value); }
        }

        private BindingList<NavigateItem> _NavigateShowList = new();
        public BindingList<NavigateItem> NavigateShowList
        {
            get { return _NavigateShowList; }
            set { SetProperty(ref _NavigateShowList, value); }
        }

        private string _DefaultView;
        public string DefaultView
        {
            get { return _DefaultView; }
            set { SetProperty(ref _DefaultView, value); }
        }
    }
}