using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PainLang;
using System.Runtime.Serialization;

namespace PainLang.PainEngine.Classes
{
    [DataContract(IsReference = true)]
    public class PainProgram
    {
        [DataMember]
        public Guid ID { get; set; }

        //////////////////////////////////////////////

        [DataMember]
        public Int32 Depth { get; set; }

        [DataMember]
        public PainMethods Methods { get; set; }

        [DataMember]
        public PainClasses Classes { get; set; }

        [DataMember]
        public PainCodeLines Lines { get; set; }

        [DataMember]
        public PainContextType ContextType { get; set; }

        //////////////////////////////////////////////////

        [DataMember]
        public Object Tag { get; set; }

        [DataMember]
        public Object Tag1 { get; set; }

        [DataMember]
        public Object Tag2 { get; set; }

        //////////////////////////////////////////////////

        public PainProgram()
        {
            this.ID = Guid.NewGuid();
            this.Methods = new PainMethods();
            this.Classes = new PainClasses();
            this.Lines = new PainCodeLines();
            this.ContextType = PainContextType.GLOBAL;
        }

        //////////////////////////////////////////////////

        public virtual Object Clone()
        {
            PainProgram item = (PainProgram)this.MemberwiseClone();
            if (item.Lines != null)
                item.Lines = new PainCodeLines(item.Lines.Select(i => i.Clone()));
            if (item.Classes != null)
                item.Classes = new PainClasses(item.Classes.Select(i => i.Clone() as PainClass));
            if (item.Methods != null)
                item.Methods = new PainMethods(item.Methods.Select(i => i.Clone() as PainMethod));
            return item;
        }
    }
}
