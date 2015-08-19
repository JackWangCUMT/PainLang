using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PainLang;
using System.Runtime.Serialization;
using PainLang.PainEngine.Classes;

namespace PainLang.OnpEngine.Models
{
    [DataContract(IsReference = true)]
    public class ExpressionExtenderInfo
    {
        [DataMember]
        public Guid ID { get; set; }
    }

    public class ExpressionExtender
    {
        private Guid? id;

        //////////////////////////////////////////////////////////////////////

        public Guid ID
        {
            get
            {
                if (id == null)
                {
                    Int32 v = 0;

                    if (OperationNames != null)
                        foreach (String operationName in OperationNames)
                            v += operationName.GetHashCode();

                    id = new Guid(v, 0, 0, new byte[8]);
                }
                return id.Value;
            }
        }

        //////////////////////////////////////////////////////////////////////

        public String[] OperationNames { get; set; }

        public Func<PainContext, Object, IList<Object>, Object> CalculateValueDelegate { get; set; }

        //////////////////////////////////////////////////////////////////////

        public ExpressionExtender()
        {
            this.OperationNames = new String[0];
        }

        public ExpressionExtender(Func<PainContext, Object, IList<Object>, Object> CalculateValueDelegate)
        {
            this.OperationNames = new String[0];
            this.CalculateValueDelegate = CalculateValueDelegate;
        }

        //////////////////////////////////////////////////////////////////////

    }
}