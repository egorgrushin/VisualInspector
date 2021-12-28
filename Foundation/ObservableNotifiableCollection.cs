using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Foundation
{
    public class ObservableNotifiableCollection<T> : ObservableCollection<T> where T : INotifyPropertyChanged
    {
        public event ItemPropertyChangedEventHandler ItemPropertyChanged;
        public event EventHandler CollectionCleared;

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            base.OnCollectionChanged(args);

            if (base.Count == 0)
                OnCollectionCleared();

            if (args.NewItems != null)
                foreach (INotifyPropertyChanged item in args.NewItems)
                    item.PropertyChanged += OnItemPropertyChanged;

            if (args.OldItems != null)
                foreach (INotifyPropertyChanged item in args.OldItems)
                    item.PropertyChanged -= OnItemPropertyChanged;
        }

        private void OnCollectionCleared()
        {
            if (CollectionCleared != null)
                CollectionCleared(this, EventArgs.Empty);
        }

        void OnItemPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (ItemPropertyChanged != null)
                ItemPropertyChanged(
                    this,
                    new ItemPropertyChangedEventArgs(sender, args.PropertyName));
        }
    }
}
