using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Foundation
{
    public class MultiParameter
    {
        public object[] Parameters { get; set; }
        public MultiParameter(object[] parameters)
        {
            this.Parameters = parameters;
        }
    }
}
