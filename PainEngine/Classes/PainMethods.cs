﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PainLang;

namespace PainLang.PainEngine.Classes
{
    public class PainMethods : List<PainMethod>
    {
        public PainMethods()
        {

        }

        public PainMethods(IEnumerable<PainMethod> Methods)
        {
            if (Methods == null)
                return;

            this.AddRange(Methods);
        }

        ////////////////////////////////////////

        public PainMethod By_ID(Guid ID)
        {
            return this.FirstOrDefault(i => i.ID == ID);
        }

        public PainMethod By_Name(String Name)
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
