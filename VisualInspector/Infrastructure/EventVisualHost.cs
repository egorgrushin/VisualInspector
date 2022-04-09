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
using VisualInspector.Models;
using VisualInspector.ViewModels;
using NLog;

namespace VisualInspector.Infrastructure
{
    public class EventVisualHost : VisualHost
	{
		private static Logger logger = LogManager.GetCurrentClassLogger();
		public static int GapWidth{ get{ return 2; } }

        private Dictionary<WarningLevels, bool> filterDictionary = new Dictionary<WarningLevels, bool>()
        {
            {WarningLevels.Normal, true},
            {WarningLevels.Middle, true},
            {WarningLevels.High, true}
        };

        private int countOfVisibleItems;

		#region Constructors and initialisation
		public EventVisualHost()
		{
			Width = 1000;
			ClipToBounds = true;
			Height = EventVisualFactory.VisualSize;
			MouseLeftButtonDown += EventVisualHost_MouseLeftButtonDown;
		}
		private void Initialize()
		{
			Clear();
			AddVisualChildrens(ItemsSource);
		}
		private void Clear()
		{
			countOfVisibleItems = 0;
			visuals.Clear();
			visualDictionary.Clear();
		}
		protected override void OnRender(DrawingContext drawingContext)
		{
			base.OnRender(drawingContext);
			var rect = new Rect(0, 0, Width + 100, Height + 100);
			drawingContext.DrawRectangle(Brushes.Transparent, null, rect);
		}
		#endregion

		#region DependencyProperties
		public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

		// Using a DependencyProperty as the backing store for ItemsSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(EventVisualHost),
            new PropertyMetadata(OnItemSourceChanged));

        public EventViewModel SelectedItem
        {
            get { return (EventViewModel)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

		// Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register("SelectedItem", typeof(EventViewModel), typeof(EventVisualHost), new FrameworkPropertyMetadata(null, OnSelectedItemChanged));

        public bool HighFilter
        {
            get { return (bool)GetValue(HighFilterProperty); }
            set { SetValue(HighFilterProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HighFilter.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HighFilterProperty =
            DependencyProperty.Register("HighFilter", typeof(bool), typeof(EventVisualHost), new PropertyMetadata(true, OnAnyFilterChanged));

        public bool MiddleFilter
        {
            get { return (bool)GetValue(MiddleFilterProperty); }
            set { SetValue(MiddleFilterProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MiddleFilter.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MiddleFilterProperty =
            DependencyProperty.Register("MiddleFilter", typeof(bool), typeof(EventVisualHost), new PropertyMetadata(true, OnAnyFilterChanged));

        public bool NormalFilter
        {
            get { return (bool)GetValue(NormalFilterProperty); }
            set { SetValue(NormalFilterProperty, value); }
        }

        // Using a DependencyProperty as the backing store for NormalFilter.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NormalFilterProperty =
            DependencyProperty.Register("NormalFilter", typeof(bool), typeof(EventVisualHost), new PropertyMetadata(true, OnAnyFilterChanged));

		#endregion

		#region Callbacks
		/// <summary>
		/// Static method for callback of filters to call a method of current control
		/// </summary>
        private static void OnAnyFilterChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            (sender as EventVisualHost).OnAnyFilterChanged(e);
        }

        private void OnAnyFilterChanged(DependencyPropertyChangedEventArgs e)
        {
            var dict = new Dictionary<string, WarningLevels>{
				{"NormalFilter", WarningLevels.Normal},
				{"MiddleFilter", WarningLevels.Middle},
				{"HighFilter", WarningLevels.High}
			};
            var currentWarningLevel = dict[e.Property.Name];

            filterDictionary[currentWarningLevel] = (bool)e.NewValue;
            if (SelectedItem != null && SelectedItem.WarningLevel == currentWarningLevel)
            {
                SelectedItem = null;
            }
            if ((bool)e.NewValue != (bool)e.OldValue)
                ApplyFilters(currentWarningLevel);
		}

		/// <summary>
		/// Static method for callback of source to call a method of current control
		/// </summary>
		private static void OnItemSourceChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
		{
			(sender as EventVisualHost).OnItemSourceChanged(e);
		}
		private void OnItemSourceChanged(DependencyPropertyChangedEventArgs e)
		{
			if(e.OldValue != null)
			{
				var models = e.OldValue as ObservableNotifiableCollection<EventViewModel>;
				models.CollectionCleared -= OnCollectionCleared;
				models.CollectionChanged -= OnCollectionChanged;
			}

			if(e.NewValue != null)
			{
				var models = e.NewValue as ObservableNotifiableCollection<EventViewModel>;
				models.CollectionCleared += OnCollectionCleared;
				models.CollectionChanged += OnCollectionChanged;
				Initialize();
			}
			else
			{
				Clear();
			}
		}
		/// <summary>
		/// Static method for callback of items to call a method of current control
		/// </summary>
		private static void OnSelectedItemChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
		{
			(sender as EventVisualHost).OnSelectedItemChanged(e);
		}

		private void OnSelectedItemChanged(DependencyPropertyChangedEventArgs e)
		{
			var oldModel = e.OldValue as EventViewModel;
			if(SelectedItem == null)
			{
				oldModel.ToggleVisual(visualDictionary[(oldModel)], false);
			}
			else
			{
				if(oldModel != null)
				{
					oldModel.ToggleVisual(visualDictionary[(oldModel)], false);
				}
				SelectedItem.ToggleVisual(visualDictionary[(SelectedItem)], true);
			}
		}
		#endregion

		private void SetOffset(DrawingVisual visual, int index)
		{
			visual.Offset = new Vector(index * (EventVisualFactory.VisualSize + GapWidth), 0);
		}
		private void SetOffset(DrawingVisual visual, int index, bool visible)
		{
			if(visible)
			{
				SetOffset(visual, index);
			}
			else
			{
				if(visual.Offset.X >= 0)
				{
					visual.Offset = new Vector(-visual.Offset.X - EventVisualFactory.VisualSize, 0);
				}
			}
		}
        private void ApplyFilters(WarningLevels currentWarningLevel)
        {
            countOfVisibleItems = 0;
            if (ItemsSource != null)
            {
                foreach (var item in ItemsSource)
                {
					var model = item as EventViewModel;
					if(model != null)
                    {
						var visual = FindVisualForModel(model);
						var warningLevelVisibility = filterDictionary[model.WarningLevel];
						SetOffset(visual, countOfVisibleItems, warningLevelVisibility);
                        if (warningLevelVisibility)
                        {
                            countOfVisibleItems++;
                        }
                    }
                }
            }
			Width = countOfVisibleItems * (EventVisualFactory.VisualSize + GapWidth);
        }
        
		#region Selection realisation
		void EventVisualHost_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var visual = GetVisual(e.GetPosition(this));
            if (visual == null)
            {
                SelectedItem = null;
                return;
            }
            var model = visualDictionary.FirstOrDefault((x) => x.Value == visual).Key;
            if (SelectedItem != null && SelectedItem == model)
            {
                SelectedItem = null;
            }
            else
            {
                SelectedItem = model;
            }
        }

		private DrawingVisual FindVisualForModel(EventViewModel model)
		{
			DrawingVisual visual = null;
			if(visualDictionary.ContainsKey(model))
				visual = visualDictionary[model];
			return visual;
		}
		#endregion
        
		#region Adding and removing visuals
		private void CreateVisualFromModel(EventViewModel model, DrawingVisual canvas)
        {
            model.DrawVisual(canvas);
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
                        RemoveVisual(visual);
                        visualDictionary.Remove(model);
                    }
                }
            }
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            if (args.OldItems != null)
                RemoveVisualChildren(args.OldItems);

            if (args.NewItems != null)
            {
                AddVisualChildrens(args.NewItems);
            }
        }
		private void OnCollectionCleared(object sender, EventArgs e)
		{
			Clear();
		}

		private void ExtendWidth()
		{
			if(double.IsNaN(Width))
				Width = countOfVisibleItems * (EventVisualFactory.VisualSize + GapWidth);
			if(Width <= countOfVisibleItems * (EventVisualFactory.VisualSize + GapWidth))
				Width += 1000;
		}

        private void AddVisualChildrens(IEnumerable models)
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
						CreateVisualFromModel(model, visual);
						var warningLevelVisibility = filterDictionary[model.WarningLevel];
						SetOffset(visual, countOfVisibleItems, warningLevelVisibility);
						if(warningLevelVisibility)
						{
							countOfVisibleItems++;
						}
                        AddVisual(visual);
                        visualDictionary.Add(model, visual);
                    }
                }
            }
            ExtendWidth();
        }
		#endregion
    }
}
