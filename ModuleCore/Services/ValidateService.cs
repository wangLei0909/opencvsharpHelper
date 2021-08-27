using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace ModuleCore.Services
{
    public class ValidateService : BindableBase, IDataErrorInfo
    {
        #region 属性

        /// <summary>
        /// 验证错误集合
        /// </summary>
        private readonly Dictionary<string, string> dataErrors = new();

        /// <summary>
        /// 是否验证通过
        /// </summary>
        public bool IsValidated
        {
            get
            {
                if (dataErrors != null && dataErrors.Count > 0)
                {
                    return false;
                }
                return true;
            }
        }

        #endregion 属性

        public string this[string columnName]
        {
            get
            {
                ValidationContext vc = new(this, null, null)
                {
                    MemberName = columnName
                };
                var res = new List<ValidationResult>();
                var result = Validator.TryValidateProperty(this.GetType().GetProperty(columnName).GetValue(this, null), vc, res);
                if (res.Count > 0)
                {
                    AddDic(dataErrors, vc.MemberName);
                    return string.Join(Environment.NewLine, res.Select(r => r.ErrorMessage).ToArray());
                }
                RemoveDic(dataErrors, vc.MemberName);
                return null;
            }
        }

        public string Error
        {
            get
            {
                return null;
            }
        }

        #region 附属方法

        /// <summary>
        /// 移除字典
        /// </summary>
        /// <param name="dics"></param>
        /// <param name="dicKey"></param>
        private static void RemoveDic(Dictionary<string, string> dics, string dicKey)
        {
            dics.Remove(dicKey);
        }

        /// <summary>
        /// 添加字典
        /// </summary>
        /// <param name="dics"></param>
        /// <param name="dicKey"></param>
        private static void AddDic(Dictionary<string, string> dics, string dicKey)
        {
            if (!dics.ContainsKey(dicKey)) dics.Add(dicKey, "");
        }

        #endregion 附属方法
    }
}