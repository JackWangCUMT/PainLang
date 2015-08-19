using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PainLang.Classes
{
    public class MyDictionary<TKey, TValue> :
#if SERVER_SIDE
        System.Collections.Concurrent.ConcurrentDictionary<TKey, TValue>, ICloneShallow
#else
        Dictionary<TKey, TValue>, ICloneShallow
#endif
    {
        public MyDictionary()
        {

        }

        public MyDictionary(IDictionary<TKey, TValue> Other)
            : base(Other)
        {

        }

        /*public MyDictionary<TKey, TValue> CloneShallow()
        {
            return (MyDictionary<TKey, TValue>)this.MemberwiseClone();
        }*/

        public Object CloneShallow()
        {
            return this.MemberwiseClone();
        }
    }

    public interface ICloneShallow
    {
        Object CloneShallow();
    }
}
