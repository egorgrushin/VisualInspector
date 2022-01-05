using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;

namespace Foundation
{
    public class CrossthreadObservableCollection<T> : ObservableNotifiableCollection<T> where T : INotifyPropertyChanged
    {
        private SynchronizationContext creationSyncContext;
        private Thread creationThread;

        public CrossthreadObservableCollection(SynchronizationContext creationSyncContext, Thread currentThread)
        {
            this.creationSyncContext = creationSyncContext;
            this.creationThread = currentThread; 
        }

        protected override void OnCollectionChanged(System.Collections.Specialized.NotifyCollectionChangedEventArgs args)
        {
            if (this.creationThread == Thread.CurrentThread) 
            {
                this.OnCollectionChangedInternal(args); 
            } 
            else if (this.creationSyncContext.GetType() == typeof(SynchronizationContext))
            {
                this.OnCollectionChangedInternal(args); 
            }
            else
            {
                this.creationSyncContext.Send(new SendOrPostCallback(delegate
                    {
                        this.OnCollectionChangedInternal(args);
                    }), null);
            }

        }

        internal void OnCollectionChangedInternal(NotifyCollectionChangedEventArgs e)
        {
            base.OnCollectionChanged(e);
        } 
    }
}
