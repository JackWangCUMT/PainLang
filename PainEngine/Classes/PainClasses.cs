using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PainLang;

namespace PainLang.PainEngine.Classes
{
    public class PainClasses : List<PainClass>
    {
        public PainClasses()
        {

        }

        public PainClasses(IEnumerable<PainClass> Items)
        {
            if (Items == null)
                return;

            this.AddRange(Items);
        }

        ////////////////////////////////////////

        public PainClass By_ID(Guid ID)
        {
            return this.FirstOrDefault(i => i.ID == ID);
        }

        public PainClass By_Name(String Name)
        {
            return this.FirstOrDefault(i => i.Name == Name);
        }

        public void Remove_by_Name(String Name)
        {
            for (var i = 0; i < this.Count; i++)
            {
                if (this[i].Name == Name)
                {
                    this.RemoveAt(i);
                    i--;
                }
            }
        }
    }
}
