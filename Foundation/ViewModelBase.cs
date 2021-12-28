using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Foundation
{
    public class ViewModelBase : PropertyNameProvider, INotifyPropertyChanged, INotifyPropertyChanging
    {
        protected Dictionary<string, object> propertyValueMap = new Dictionary<string, object>();
        private const string IndexerName = System.Windows.Data.Binding.IndexerName; /* "Item[]" */

        public event PropertyChangingEventHandler PropertyChanging = (sender, args) => { };

        public event PropertyChangedEventHandler PropertyChanged = (sender, args) => { };


        public object this[string key]
        {
            get { return propertyValueMap.ContainsKey(key) ? propertyValueMap[key] : null; }
            set
            {
                RaisePropertyChanging(IndexerName);
                if (propertyValueMap.ContainsKey(key)) propertyValueMap[key] = value;
                else propertyValueMap.Add(key, value);
                RaisePropertyChanged(IndexerName);
            }
        }

        public object this[string key, object defaultValue]
        {
            get
            {
                if (propertyValueMap.ContainsKey(key)) return propertyValueMap[key];
                propertyValueMap.Add(key, defaultValue);
                return defaultValue;
            }
            set { this[key] = value; }
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
