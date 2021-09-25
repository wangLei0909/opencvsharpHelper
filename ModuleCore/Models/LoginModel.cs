using ModuleCore.Common;
using ModuleCore.Common.Authority;
using ModuleCore.Mvvm;
using ModuleCore.Services;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.ComponentModel;
using System.Data;
using System.Linq;

namespace ModuleCore.Models
{
    public class LoginModel : BindableBase
    {
        public LoginModel(IDialogService dialogService)
        {
            _dialogService = dialogService;
            //权限列表
            var authorityListString = Enum.GetNames(typeof(Authority));
            var i = 0;
            foreach (var item in authorityListString)
            {
                AuthorityList.Add(item, i);
                i++;
            }

            //已注册用户
            LoadUsers();
        }
        private readonly IDialogService _dialogService;
        #region 登录

        //登录的用户
        private User _LoginUser;

        public User LoginUser
        {
            get { return _LoginUser; }
            set { SetProperty(ref _LoginUser, value); }
        }

        #endregion 登录

        private BindingList<User> _UserList = new();

        public BindingList<User> UserList
        {
            get { return _UserList; }
            set { SetProperty(ref _UserList, value); }
        }

        private string _Name = "Admin";

        public string Name
        {
            get { return _Name; }
            set { SetProperty(ref _Name, value); }
        }

        public ObservableDictionary<string, int> AuthorityList { get; set; } = new();

        private int _AuthoritySelect;

        public int AuthoritySelect
        {
            get { return _AuthoritySelect; }
            set { SetProperty(ref _AuthoritySelect, value); }
        }



        private DataTable dt;

        public void LoadUsers()
        {
            dt = JsonService.DataTableFromEncryptFile("Authority.dll");
 
            if (dt is null)
            {
                //First run

                dt = new DataTable();

                dt.Columns.Add("Name", Type.GetType("System.String"));
                dt.Columns.Add("Password", Type.GetType("System.String"));
                dt.Columns.Add("Authority", Type.GetType("System.Int64"));

                var password = EncryptService.Encrypt("17551023102");

                AddUser(new("Guest", "password", Authority.Operator));
                AddUser(new(Name, password, Authority.Administrator));
            }
            else
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    var name = dt.Rows[i]["Name"].ToString();
                    var password = dt.Rows[i]["Password"].ToString();
                    var authority = (Authority)Convert.ToInt32((long)dt.Rows[i]["Authority"]);
                    var loaduser = new User(name, password, authority);
                    loaduser.DeleteMe += DeleteUser;
                    loaduser.ChangeMyPassword += ChangePassword;

                    UserList.Add(loaduser);
                }
#if DEBUG
            LoginUser = UserList.Where(u => u.Name == "Admin").FirstOrDefault();
#else
            LoginUser = UserList.Where(u => u.Name == "Guest").FirstOrDefault();
#endif
        }

        public string LoadPassword()
        {
            var user = UserList.Where(u => u.Name == Name).FirstOrDefault();
            return user.Password;
        }

        public void AddUser(User user)
        {
            user.DeleteMe += DeleteUser;
            UserList.Add(user);

            SaveUsers();
        }

        private void ChangePassword(string userName)
        {
            if (userName == "Admin" || userName == "Guest") return;
            _dialogService.ShowDialog("PasswordChange", new DialogParameters($"name={userName}"), r =>
            {



            });
        }

        private void DeleteUser(string userName)
        {
            if (userName == "Admin" || userName == "Guest") return;

            var user = UserList.Where(u => u.Name == userName);
            if (user.Any())
            {
                UserList.Remove(user.FirstOrDefault());
                SaveUsers();
            }
        }

        public void SaveUsers()
        {
            dt.Rows.Clear();
            for (int i = 0; i < UserList.Count; i++)
            {
                DataRow dr = dt.NewRow();
                dt.Rows.Add(dr);
                dt.Rows[i]["Name"] = UserList[i].Name;
                dt.Rows[i]["Password"] = UserList[i].Password;
                dt.Rows[i]["Authority"] = (int)UserList[i].Authority;
            }
            JsonService.DataTableToEncryptFile("Authority.dll", dt);
        }
    }
}