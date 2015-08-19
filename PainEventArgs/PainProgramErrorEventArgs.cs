using PainLang.PainEngine.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PainLang.PainEventArgs
{
    public class PainProgramErrorEventArgs : EventArgs
    {
        public PainContext Context { get; set; }

        public PainProgram Program { get; set; }

        public PainState State { get; set; }
        
        ////////////////////////////////////////

        public void Clean()
        {
            Context = null;
            Program = null;
            State = null;
        }
    }
}
