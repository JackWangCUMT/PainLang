using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using PainLang;
using PainLang.PainEngine.Classes;
using PainLang.Helpers;

namespace PainLang.OnpEngine.InternalExtenders
{
    public static class ExtenderSetValue
    {
        public static readonly String Name = "__EXT_SET";

        public static readonly Char[] NameChars = Name.ToCharArray();

        ////////////////////////////////////////////////////////////////////////

        public static Object Execute(PainContext EvaluateContext, Object obj, IList<Object> Parameters)
        {
            Boolean isValueSet = false;

            String propertyPath = UniConvert.ToUniString(Parameters != null && Parameters.Count > 0 ? Parameters[0] : null);
            Object value = (Parameters != null && Parameters.Count > 1 ? Parameters[1] : null);

            if (obj is IDictionary)
            {
                IDictionary dict = obj as IDictionary;
                dict[propertyPath] = value;
                isValueSet = true;
            }

            if (obj is PainObject)
            {
                propertyPath = propertyPath.ToUpper();
                PainObject painObj = obj as PainObject;
                painObj[propertyPath] = value;
                return null;
            }

            if (!isValueSet)
                isValueSet = RefSensitiveHelper.I.SetValue(obj, propertyPath, value);

            if (!isValueSet)
                isValueSet = RefUnsensitiveHelper.I.SetValue(obj, propertyPath, value);

            if (isValueSet)
                return value;

            return null;
        }
    }
}