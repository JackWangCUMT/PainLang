using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PainLang;
using System.Runtime.Serialization;

namespace PainLang.PainEngine.Classes
{
    [DataContract(IsReference = true)]
    public class PainMethod : PainProgram
    {
        [DataMember]
        public String Name { get; set; }

        [DataMember]
        public List<String> Parameters { get; set; }

        [DataMember]
        public PainObject ParentObject { get; set; }

        //////////////////////////////////////////////////

        public PainMethod()
        {
            Name = "";
            Parameters = new List<String>();
            ContextType = PainContextType.METHOD;
        }

        //////////////////////////////////////////////////

        public override Object Clone()
        {
            PainMethod item = base.Clone() as PainMethod;
            if (item.Parameters != null)
                item.Parameters = new List<String>(item.Parameters);
            return item;
        }
    }
}
