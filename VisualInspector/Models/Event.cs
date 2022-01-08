using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace VisualInspector.Models
{
    public enum WarningLevels
    {
        Normal, Middle, High
    }

    public class Event
    {

        public WarningLevels WarningLevel { get; set; }

        public int Lock { get; set; }
        public int Sensor { get; set; }
        public int Room { get; set; }
        public DateTime DateTime { get; set; }
        public int AccessLevel { get; set; }
    }
}
