using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Foundation
{
	public class ObservableNotifiableCollection<T> : TreadSafeObservableCollection<T> where T : INotifyPropertyChanged
    {
		public ObservableNotifiableCollection()
			: base()
		{
		}
		public ObservableNotifiableCollection(IEnumerable<T> collection)
			: base(collection)
		{
		}
		public ObservableNotifiableCollection(List<T> list) 
			: base(list)
		{
		}
		public event EventHandler<ItemPropertyChangedEventArgs> ItemPropertyChanged;
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
