using PainLang.PainEngine.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PainLang.PainEventArgs
{
    public class PainProgramChangedEventArgs : EventArgs
    {
        public PainContext Context { get; set; }

        public PainProgram Program { get; set; }

        public PainState State { get; set; }

        public IList<PainMethodParam> Parameters { get; set; }

        ////////////////////////////////////////

        public void Clean()
        {
            Context = null;
            Program = null;
            State = null;
            Parameters = null;
        }
    }

    public class PainMethodParam
    {
        public String Name { get; set; }

        public Object Value { get; set; }

        public PainMethodParam Clone()
        {
            return (PainMethodParam)this.MemberwiseClone();
        }
    }
}
