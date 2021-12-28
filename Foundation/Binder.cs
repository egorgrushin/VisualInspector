using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Foundation
{
    public class Binder
    {
        private readonly Func<bool> ruleDelegate;
        private readonly string message;

        public Binder(Func<bool> ruleDelegate, string message)
        {
            this.ruleDelegate = ruleDelegate;
            this.message = message;

            IsDirty = true;
        }

        public string Error { get; set; }
        public bool HasError { get; set; }
        public bool IsDirty { get; set; }

        public void Update()
        {
            if (!IsDirty)
                return;
            Error = null;
            HasError = false;
            try
            {
                if (!ruleDelegate())
                {
                    Error = message;
                    HasError = true;
                }
            }
            catch (Exception)
            {
                Error = message;
                HasError = true;
            }
        } 
    }
}
