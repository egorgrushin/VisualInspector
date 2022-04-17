using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Foundation
{
    public class ItemPropertyChangedEventArgs : PropertyChangedEventArgs
    {
        private object item;

        public ItemPropertyChangedEventArgs(object item, string propertyName)
            : base(propertyName)
        {
            this.item = item;
        }

        public object Item
        {
            get { return item; }
        }
    }
}
