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
    public class ExpressionMethodInfo
    {
        [DataMember]
        public Guid ID { get; set; }
    }

    public class ExpressionMethod
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

        public Func<PainContext, IList<Object>, ExpressionMethodResult> CalculateValueDelegate { get; set; }

        //////////////////////////////////////////////////////////////////////

        public ExpressionMethod()
        {
            this.OperationNames = new String[0];
        }

        public ExpressionMethod(Func<PainContext, IList<Object>, ExpressionMethodResult> CalculateValueDelegate)
        {
            this.OperationNames = new String[0];
            this.CalculateValueDelegate = CalculateValueDelegate;
        }

        //////////////////////////////////////////////////////////////////////
    }

    public class ExpressionMethodResult
    {
        public Object Value;

        public Boolean NewContextCreated;

        public ExpressionMethodResult()
        {

        }

        public ExpressionMethodResult(Object Value)
        {
            this.Value = Value;
        }
    }
}