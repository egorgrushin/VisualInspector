using Foundation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace Foundation
{
    public class ValidationViewModel : ViewModel, IDataErrorInfo
    {
        private Dictionary<string, Binder> ruleMap = new Dictionary<string, Binder>();

        public bool HasErrors
        {
            get
            {
                var values = ruleMap.Values.ToList();
                values.ForEach(b => b.Update());
                return values.Any(b => b.HasError);
            }
        }

        public string Error
        {
            get
            {
                var errors = from b in ruleMap.Values where b.HasError select b.Error;
                return string.Join("\n", errors.ToArray());
            }
        }

        public new string this[string columnName]
        {
            get
            { 
                if (ruleMap.ContainsKey(columnName))
                {
                    ruleMap[columnName].Update();
                    return ruleMap[columnName].Error;
                }
                return null;
            }
        }

        protected void AddRule<T>(Expression<Func<T>> expression, Func<bool> ruleDelegate, string errorMessage)
        {
            var name = GetPropertyName(expression);
            ruleMap.Add(name,  new Binder(ruleDelegate, errorMessage));
        }
        protected override void Set<TProperty>(Expression<Func<TProperty>> expression, TProperty value, bool forceUpdate)
        {
            ruleMap[GetPropertyName(expression)].IsDirty = true;
            base.Set<TProperty>(expression, value, forceUpdate);
        }

    }
}
