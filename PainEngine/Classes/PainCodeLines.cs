using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PainLang;
#if PCL
using System.Collections.ObjectModel2;
#else
using System.Collections.ObjectModel;
#endif

namespace PainLang.PainEngine.Classes
{
    public class PainCodeLines : ObservableCollection<PainCodeLine>
    {
        public PainCodeLines()
        {

        }

        public PainCodeLines(IEnumerable<PainCodeLine> Items)
        {
            if (Items == null)
                return;

            foreach (PainCodeLine item in Items)
                this.Add(item);
        }

        ////////////////////////////////////////

        public PainCodeLine Get_by_ID(Guid ID)
        {
            return this.FirstOrDefault(i => i.ID == ID);
        }
    }
}
