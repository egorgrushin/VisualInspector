using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace Foundation
{
    public class ViewModelBase : PropertyChangeNotification
    {
        private const string IndexerName = System.Windows.Data.Binding.IndexerName; /* "Item[]" */

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


       


    }
}
