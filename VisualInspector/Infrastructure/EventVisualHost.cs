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

namespace VisualInspector.Infrastructure
{
    public class EventVisualHost : VisualHost
    {
        private Dictionary<WarningLevels, bool> filterDictionary = new Dictionary<WarningLevels, bool>()
        {
            {WarningLevels.Normal, true},
            {WarningLevels.Middle, true},
            {WarningLevels.High, true}
        };

        private int countOfVisibleItems;
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


        private static void OnAnyFilterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as EventVisualHost).OnAnyFilterChanged(e);
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
            if (SelectedItem != null && SelectedItem.GetWarningLevel() == currentWarningLevel)
            {
                SelectedItem = null;
            }
            if ((bool)e.NewValue != (bool)e.OldValue)
                ApplyFilters(currentWarningLevel);
            //EraseAllVisuals();
            // Redraw();
        }

        private void ApplyFilters(WarningLevels currentWarningLevel)
        {
            countOfVisibleItems = 0;
            foreach (var item in ItemsSource)
            {
                var eventViewModel = item as EventViewModel;
                if (eventViewModel != null)
                {
                    var visual = FindVisualForModel(eventViewModel);
                    if (filterDictionary[eventViewModel.GetWarningLevel()])
                    {

                        SetOffset(visual, countOfVisibleItems);
                        countOfVisibleItems++;
                        // visual = CreateVisualFromModel(eventViewModel, visual, rect);
                    }
                    else
                    {
                        SetOffset(visual, -9999);
                    }
                }
            }
        }

        private void SetOffset(DrawingVisual visual, int offset)
        {
            visual.Offset = new Vector(offset * (itemSize.Width + gapWidth), 0);
        }


        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register("SelectedItem", typeof(EventViewModel), typeof(EventVisualHost), new FrameworkPropertyMetadata(null, OnSelectedItemChanged));

        private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as EventVisualHost).OnSelectedItemChanged(e);
        }

        private void OnSelectedItemChanged(DependencyPropertyChangedEventArgs e)
        {
            var oldModel = e.OldValue as EventViewModel;
            if (SelectedItem == null)
            {
                oldModel.ToggleVisual(visualDictionary[(oldModel)], false);
            }
            else
            {
                if (oldModel != null)
                {
                    oldModel.ToggleVisual(visualDictionary[(oldModel)], false);
                }
                SelectedItem.ToggleVisual(visualDictionary[(SelectedItem)], true);
            }
        }





        private Size itemSize;
        private int gapWidth;

        public EventVisualHost()
        {
            itemSize = new Size(16, 16);
            Width = 1000;
            gapWidth = 1;
            Height = itemSize.Height + gapWidth;
            MouseLeftButtonDown += EventVisualHost_MouseLeftButtonDown;
        }


        /// <summary>
        /// Selection realisation
        /// </summary>
        /// 
        protected override HitTestResult HitTestCore(PointHitTestParameters hitTestParameters)
        {
            return new PointHitTestResult(this, hitTestParameters.HitPoint);
        }
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
                        RemoveVisual(visual);
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
            model.ToggleVisual(visual, true);
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            if (args.OldItems != null)
                RemoveVisualChildren(args.OldItems);

            if (args.NewItems != null)
            {
                AddVisualChildrens(args.NewItems);
            }
            //Redraw();
        }

        private void ExtendWidth()
        {
            if (double.IsNaN(Width))
                Width = (visuals.Count - 1) * (itemSize.Width + gapWidth);
            if (Width <= (visuals.Count - 1) * (itemSize.Width + gapWidth))
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
                        CreateVisualFromModel(model, visual, new Rect(new Point(0, 0), itemSize));
                        if (filterDictionary[model.GetWarningLevel()])
                        {
                            SetOffset(visual, countOfVisibleItems);
                            countOfVisibleItems++;
                        }
                        else
                        {
                            SetOffset(visual, -9999);
                        }
                        AddVisual(visual);
                        visualDictionary.Add(model, visual);
                    }
                }
            }
        }


        private void OnCollectionCleared(object sender, EventArgs e)
        {
            Clear();
        }

        private void EraseVisual(DrawingVisual visual)
        {
            var dc = visual.RenderOpen();
            dc.Close();
        }
        private void EraseAllVisuals()
        {
            foreach (var item in visualIndexator.Values)
            {
                EraseVisual(item);
            }
        }


        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            var rect = new Rect(0, 0, Width + 100, Height + 100);
            drawingContext.DrawRectangle(Brushes.Transparent, null, rect);
        }





    }
}
