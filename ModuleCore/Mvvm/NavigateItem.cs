using Prism.Mvvm;

namespace ModuleCore.Mvvm
{
    public class NavigateItem : BindableBase
    {
        private string _ViewName;

        public string ViewName
        {
            get { return _ViewName; }
            set { SetProperty(ref _ViewName, value); }
        }

        private string _IconKind;

        public string IconKind
        {
            get { return _IconKind; }
            set { SetProperty(ref _IconKind, value); }
        }

        private string _DisplayName;

        public string DisplayName
        {
            get { return _DisplayName; }
            set { SetProperty(ref _DisplayName, value); }
        }

        private int _UserLevel;

        public int UserLevel
        {
            get { return _UserLevel; }
            set { SetProperty(ref _UserLevel, value); }
        }

        private bool _Display;

        public bool Display
        {
            get { return _Display; }
            set { SetProperty(ref _Display, value); }
        }
    }
}