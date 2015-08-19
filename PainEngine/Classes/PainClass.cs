using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PainLang;
using System.Runtime.Serialization;

namespace PainLang.PainEngine.Classes
{
    [DataContract(IsReference = true)]
    public class PainClass : PainMethod
    {
        /*public String ClassName { get; set; }

        public List<String> Parameters { get; set; }*/

        //////////////////////////////////////////////////

        public PainClass()
            : base()
        {
            /*ClassName = "";
            Parameters = new List<String>();
            ContextType = PainLocalContextType.LOCAL;*/
            this.ParentObject = null;
        }

        //////////////////////////////////////////////////

    }
}
