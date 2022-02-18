using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Controls;
using VisualInspector.ViewModels;

namespace VisualInspector.Infrastructure
{
    public class VisualHost : FrameworkElement
    {
        protected List<DrawingVisual> visuals;

        protected List<DrawingVisual> hits;

        protected Dictionary<EventViewModel, DrawingVisual> visualDictionary;

        protected Dictionary<int, DrawingVisual> visualIndexator;

        public VisualHost()
        {
            visuals = new List<DrawingVisual>();
            visualDictionary = new Dictionary<EventViewModel, DrawingVisual>();
            visualIndexator = new Dictionary<int, DrawingVisual>();
            hits = new List<DrawingVisual>();
        }

       

        protected override Visual GetVisualChild(int index)
        {
            if (index < 0 || index >= visuals.Count)
                throw new ArgumentOutOfRangeException();
            return visuals[index];
        }

        protected override int VisualChildrenCount
        {
            get { return visuals.Count; }
        }


        public void AddVisual(DrawingVisual visual)
        {
            visuals.Add(visual);
            base.AddLogicalChild(visual);
            base.AddVisualChild(visual);
        }

        public void RemoveVisual(DrawingVisual visual)
        {
            visuals.Remove(visual);
            base.RemoveVisualChild(visual);
            base.RemoveLogicalChild(visual);
        }

        public DrawingVisual GetVisual(Point hitPoint)
        {
            var hitResult = VisualTreeHelper.HitTest(this, hitPoint);
            return hitResult.VisualHit as DrawingVisual;
        }

        public List<DrawingVisual> GetVisuals(Geometry region)
        {
            hits.Clear();
            var parameters = new GeometryHitTestParameters(region);
            var callback = new HitTestResultCallback(this.HitTestCallback);
            VisualTreeHelper.HitTest(this, null, callback, parameters);
            return hits;
        }

        private HitTestResultBehavior HitTestCallback(HitTestResult result)
        {
            var geometryResult = (GeometryHitTestResult)result;
            var visual = result.VisualHit as DrawingVisual;
            if (visual != null &&
                geometryResult.IntersectionDetail == IntersectionDetail.FullyInside)
            {
                hits.Add(visual);
            }
            return HitTestResultBehavior.Continue;
        }

        
    }

}
