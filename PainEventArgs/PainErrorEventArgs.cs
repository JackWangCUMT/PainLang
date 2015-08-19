using PainLang.PainEngine.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PainLang.PainEventArgs
{
    public class PainErrorEventArgs : EventArgs
    {
        public PainContext Context { get; set; }

        public PainProgram Program { get; set; }

        public PainState State { get; set; }

        ////////////////////////////////////////

        public Exception Error { get; set; }

        public Boolean Handled { get; set; }
        
        ////////////////////////////////////////

        public void Clean()
        {
            Error = null;
            Context = null;
            Program = null;
            State = null;
        }
    }
}
