using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Windows.Threading;

namespace Foundation
{
	public class TreadSafeObservableCollection<T> : ObservableCollection<T> 
	{
		public TreadSafeObservableCollection()
			: base()
		{
		}
		public TreadSafeObservableCollection(IEnumerable<T> collection)
			: base(collection)
		{
		}
		public TreadSafeObservableCollection(List<T> list) 
			: base(list)
		{
		}
		public override event NotifyCollectionChangedEventHandler CollectionChanged;
		protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
		{
			var eh = CollectionChanged;
			if(eh != null)
			{
				var dispatcher = (from NotifyCollectionChangedEventHandler nh in eh.GetInvocationList()
										 let dpo = nh.Target as DispatcherObject
										 where dpo != null
										 select dpo.Dispatcher).FirstOrDefault();

				if(dispatcher != null && !dispatcher.CheckAccess())
				{
					dispatcher.Invoke(DispatcherPriority.DataBind, (Action)(() => OnCollectionChanged(e)));
				}
				else
				{
					foreach(NotifyCollectionChangedEventHandler nh in eh.GetInvocationList())
					{
						nh.Invoke(this, e);
					}
				}
			}
		}
	}
}
