using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;
namespace Foundation
{
    public class PropertyChangeNotification : PropertyNameProvider, INotifyPropertyChanged, INotifyPropertyChanging
    {
        protected Dictionary<string, object> propertyValueMap = new Dictionary<string, object>();
        public event PropertyChangingEventHandler PropertyChanging = (sender, args) => { };

        public event PropertyChangedEventHandler PropertyChanged = (sender, args) => { };

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

        protected virtual TProperty Get<TProperty>(Expression<Func<TProperty>> expression, TProperty defaultValue)
        {
            var propertyName = GetPropertyName(expression);
            if (!propertyValueMap.ContainsKey(propertyName))
                propertyValueMap.Add(propertyName, defaultValue);
            return (TProperty)propertyValueMap[propertyName];
        }

        protected TProperty Get<TProperty>(Expression<Func<TProperty>> expression)
        {
            return Get(expression, default(TProperty));
        }


        protected void RaisePropertyChanging(string propertyName)
        {
            var handler = PropertyChanging;
            handler(this, new PropertyChangingEventArgs(propertyName));
        }
        protected void RaisePropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
