using PainLang;
using PainLang.Classes;
using PainLang.Helpers;
using PainLang.OnpEngine.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace PainLang.PainEngine.Classes
{
    [DataContract(IsReference = true)]
    [KnownType(typeof(PainMethod))]
    [KnownType(typeof(PainObject))]
    [KnownType(typeof(PainProgram))]
    [KnownType(typeof(PainClass))]
    [KnownType(typeof(ExpressionExtenderInfo))]
    [KnownType(typeof(ExpressionMethodInfo))]
    [KnownType(typeof(Undefined))]
    [KnownType(typeof(EmptyObject))]
    public class PainObject
    {
        public object this[String PropertyName]
        {
            get
            {
                Object val = null;
                PropertyName = PropertyName.ToUpper();

                if (PropertyName == "THIS")
                    return ParentObject ?? this;

                if (StaticValues != null)
                {
                    if (StaticValues.TryGetValue(PropertyName, out val))
                        return val;
                }

                DynamicValues.TryGetValue(PropertyName, out val);
                return val;
            }
            set
            {
                PropertyName = PropertyName.ToUpper();

                if (PropertyName != "THIS")
                {
                    if (StaticValues != null && StaticValues.ContainsKey(PropertyName))
                    {
                        StaticValues = StaticValues;
                    }
                    else
                    {
                        DynamicValues[PropertyName] = value;
                    }
                }

                if (ValueChanged != null)
                    ValueChanged.Invoke(this, new EventArgs());
            }
        }

        [IgnoreDataMember]
        public IEnumerable<String> AllKeys
        {
            get
            {
                if (StaticValues != null)
                    return StaticValues.Keys.Union(DynamicValues.Keys);
                return DynamicValues.Keys;
            }
        }

        [IgnoreDataMember]
        public IEnumerable<Object> AllValues
        {
            get
            {
                if (StaticValues != null)
                    return StaticValues.Values.Union(DynamicValues.Values);
                return DynamicValues.Values;
            }
        }

        [IgnoreDataMember]
        public Int32 TotalCount
        {
            get
            {
                if (StaticValues != null)
                    return StaticValues.Count + DynamicValues.Count;
                return DynamicValues.Count;
            }
        }

        ////////////////////////////////////////////////

        [DataMember]
        public IDictionary<String, Object> DynamicValues { get; set; }

        [DataMember]
        public PainObject ParentObject { get; set; }

        [IgnoreDataMember]
        public IDictionary<String, Object> StaticValues { get; set; }

        ////////////////////////////////////////////////

        public event EventHandler ValueChanged;

        ////////////////////////////////////////////////

        public PainObject()
        {
            this.DynamicValues = new MyDictionary<String, Object>();
        }

        ////////////////////////////////////////////////

        public PainObject CloneBySerialize()
        {
            PainObject obj = this.SerializeToBytes().Deserialize<PainObject>();
            return obj;
        }

        public Boolean Contains(String PropertyName)
        {
            PropertyName = PropertyName.ToUpper();

            if (PropertyName == "THIS")
                return true;

            if (StaticValues != null && StaticValues.ContainsKey(PropertyName))
                return true;

            return DynamicValues.ContainsKey(PropertyName);
        }
    }
}
