using Foundation;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace VisualInspector.Infrastructure
{
    public class EventVisualHost : VisualHost
    {
        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }


        // Using a DependencyProperty as the backing store for ItemsSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(EventVisualHost),
            new PropertyMetadata(OnItemSourceChanged));


        private Size itemSize;
        private int gapWidth;

		#region Selection features interface

		private List<DrawingVisual> selectedVisuals;
		public List<EventViewModel> selectedItems
		{
			get
			{
				var resultList = new List<EventViewModel>();
				foreach(var visual in selectedVisuals)
				{
					resultList.Add(visualDictionary.FirstOrDefault((x) => x.Value == visual).Key);
				}

				return resultList;
			}
		}
		public List<int> selectedIndecies
		{
			get
			{
				var resultList = new List<int>();
				foreach(var visual in selectedVisuals)
				{
					resultList.Add(visualIndexator.FirstOrDefault(x => x.Value == visual).Key);
				}

				return resultList;
			}
		}

		#endregion

		public EventVisualHost()
		{
			selectedVisuals = new List<DrawingVisual>();

			itemSize = new Size(16, 16);
			gapWidth = 1;
			Height = itemSize.Height + gapWidth;
			MouseLeftButtonDown += EventVisualHost_MouseLeftButtonDown;
		}
        void EventVisualHost_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
			var visual = GetVisual(e.GetPosition(this));
			
			if(selectedVisuals.Contains(visual))
			{
				selectedVisuals.Remove(visual);
			}
			else
			{
				selectedVisuals.Add(visual); 
			}

            var model = visualDictionary.FirstOrDefault((x) => x.Value == visual).Key;
            var index = visualIndexator.FirstOrDefault(x => x.Value == visual).Key;

            var rect = new Rect(index * (itemSize.Width + gapWidth), 0, itemSize.Width, itemSize.Height);

            model.ChangeVisual(visual);
        }


        private static void OnItemSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as EventVisualHost).OnItemSourceChanged(e);
        }

        private void OnItemSourceChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue != null)
            {
                var models = e.OldValue as ObservableNotifiableCollection<EventViewModel>;
                models.CollectionCleared -= OnCollectionCleared;
                models.CollectionChanged -= OnCollectionChanged;
                models.ItemPropertyChanged -= OnItemPropertyChanged;
            }

            if (e.NewValue != null)
            {
                var models = e.NewValue as ObservableNotifiableCollection<EventViewModel>;
                models.CollectionCleared += OnCollectionCleared;
                models.CollectionChanged += OnCollectionChanged;
                models.ItemPropertyChanged += OnItemPropertyChanged;
                Initialize();
            }
            else
            {
                Clear();
            }

            Redraw();
        }

        private void Initialize()
        {
            Clear();
            AddVisualChildren(ItemsSource);
        }


        private void Clear()
        {
            visuals.Clear();
            visualDictionary.Clear();
        }

        private DrawingVisual CreateVisualFromModel(EventViewModel model, DrawingVisual canvas, Rect rect)
        {
            return model.GetVisual(canvas, rect);
        }

        private void RemoveVisualChildren(IEnumerable models)
        {
            foreach (var item in models)
            {
                var model = item as EventViewModel;
                if (model != null)
                {
                    var visual = FindVisualForModel(model);
                    if (visual != null)
                    {
                        visuals.Remove(visual);
                        visualDictionary.Remove(model);
                    }
                }
            }
        }

        private DrawingVisual FindVisualForModel(EventViewModel model)
        {
            DrawingVisual visual = null;
            if (visualDictionary.ContainsKey(model))
                visual = visualDictionary[model];
            return visual;
        }
        private void OnItemPropertyChanged(object sender, ItemPropertyChangedEventArgs args)
        {
            var model = args.Item as EventViewModel;
            if (model == null)
                throw new ArgumentException("args.Item was expected to be of type EventViewModel but was not.");
            var visual = FindVisualForModel(model);
            var index = visualIndexator.FirstOrDefault(x => x.Value == visual).Key;
            var rect = new Rect(index * (itemSize.Width + gapWidth), 0, itemSize.Width, itemSize.Height);
            model.ChangeVisual(visual);
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            if (args.OldItems != null)
                RemoveVisualChildren(args.OldItems);

            if (args.NewItems != null)
                AddVisualChildren(args.NewItems);
            Redraw();
        }

        private void AddVisualChildren(IEnumerable models)
        {
            foreach (var item in models)
            {
                var model = item as EventViewModel;
                if (model != null)
                {
                    var visual = FindVisualForModel(model);
                    if (visual == null)
                    {
                        visual = new DrawingVisual();
                        visuals.Add(visual);
                        visualDictionary.Add(model, visual);
                    }
                }
            }
        }

        private void OnCollectionCleared(object sender, EventArgs e)
        {
            Clear();
            Redraw();
        }


        private void Redraw()
        {
            visualIndexator.Clear();
            if (ItemsSource != null)
            {
                int i = 0;
                foreach (var item in ItemsSource)
                {
                    var model = item as EventViewModel;
                    if (model != null)
                    {
                        var visual = FindVisualForModel(model);
                        visualIndexator.Add(i, visual);
                        var rect = new Rect(i * (itemSize.Width + gapWidth), 0, itemSize.Width, itemSize.Height);
                        visual = CreateVisualFromModel(model, visual, rect);
						if(selectedVisuals.Contains(visual))
						{
							model.ChangeVisual(visual);
						}
                        i++;
                    }
                }
            }
        }


        
    }
}
