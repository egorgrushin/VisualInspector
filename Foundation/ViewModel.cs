using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Foundation
{
    public class ViewModel : ViewModelBase
    {
        protected TProperty Get<TProperty>(Expression<Func<TProperty>> expression)
        {
            return Get(expression, default(TProperty));
        }

        protected virtual TProperty Get<TProperty>(Expression<Func<TProperty>> expression, TProperty defaultValue)
        {
            var propertyName = GetPropertyName(expression);
            if (!propertyValueMap.ContainsKey(propertyName))
                propertyValueMap.Add(propertyName, defaultValue);
            return (TProperty)propertyValueMap[propertyName];
        }

        protected void Set<TProperty>(Expression<Func<TProperty>> expression, TProperty value)
        {
            Set(expression, value, false);
        }

        protected virtual void Set<TProperty>(Expression<Func<TProperty>> expression, TProperty value, bool forceUpdate)
        {
            var oldValue = Get(expression);
            var propertyName = GetPropertyName(expression);
            if (!object.Equals(value, oldValue) || forceUpdate)
            {
                RaisePropertyChanging(propertyName);
                propertyValueMap[propertyName] = value;
                RaisePropertyChanged(propertyName);
            }
            //if (!Values.ContainsKey(propertyName))
            //    Values.Add(propertyName, value);
            //else Values[propertyName] = value;
        }

        
    }
}
