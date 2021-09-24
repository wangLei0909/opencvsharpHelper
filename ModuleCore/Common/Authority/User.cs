using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModuleCore.Common.Authority
{
    public class User
    {
        public User(string name, string password, Authority authority)
        {
            Name = name;
            Password = password;
            Authority = authority;
        }
        public string Name { get; set; }
        public string Password { get; set; }

        public Authority Authority { get; set; }

        private DelegateCommand _Delete;
        public DelegateCommand Delete =>
             _Delete ??= new DelegateCommand(ExecuteDelete);

        void ExecuteDelete()
        {
            DeleteMe?.Invoke(Name);
        }

        public event Action<string> DeleteMe;

        private DelegateCommand _ChangePassword;
        public DelegateCommand ChangePassword =>
             _ChangePassword ??= new DelegateCommand(ExecuteChangePassword);

        void ExecuteChangePassword()
        {
            ChangeMyPassword?.Invoke(Name);
        }

        public event Action<string> ChangeMyPassword;

        public override string ToString()
        {
            return $"{Name}";
        }

    }
}
