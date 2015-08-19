using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PainLang.PainEngine.Classes
{
    public class CodeLine : List<Char>
    {
        public CodeLine()
        {

        }

        public CodeLine(String Text)
        {
            this.AddRange(Text.ToCharArray());
        }

        public CodeLine(IEnumerable<Char> Chars)
        {
            this.AddRange(Chars);
        }

        public override string ToString()
        {
            return String.Join("", this.ToArray()); // base.ToString();
        }
    }
}
