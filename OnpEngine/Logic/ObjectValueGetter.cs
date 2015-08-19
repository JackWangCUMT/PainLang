using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Collections;
using PainLang;
using PainLang.PainEngine.Classes;
using PainLang.Helpers;
using PainLang.OnpEngine.Models;

namespace PainLang.OnpEngine.Logic
{
    public static class ObjectValueGetter
    {
        public static Object GetValueFromObject(
            Object Obj,
            String PropertyPath)
        {
            Boolean foundValue = false;
            return GetValueFromObject(Obj, PropertyPath, -1, out foundValue);
        }

        public static Object GetValueFromObject(
            Object Obj,
            String PropertyPath,
            Int32 ParametersCount,
            out Boolean FoundValue)
        {
            FoundValue = false;

            if (Obj == null)
            {
                FoundValue = true;
                return null;
            }

            if (Obj is EmptyObject)
                throw new NotImplementedException();

            if (Obj is IDictionary)
            {
                IDictionary dict = Obj as IDictionary;
                if (dict.Contains(PropertyPath))
                {
                    FoundValue = true;
                    return dict[PropertyPath];
                }
            }

            else if (Obj is PainObject)
            {
                PropertyPath = PropertyPath.ToUpper();
                PainObject painObj = Obj as PainObject;
                if (painObj.Contains(PropertyPath))
                {
                    FoundValue = true;
                    return painObj[PropertyPath];
                }
            }

            return RefUnsensitiveHelper.I.GetValueOrMethod(
                Obj,
                PropertyPath,
                ParametersCount,
                out FoundValue);
        }

        public static Boolean EvaluateValue(
            String FieldOrMethodName,
            PainContext PainContext)
        {
            return EvaluateValueOrMethod(
                null,
                FieldOrMethodName,
                -1,
                PainContext);
        }

        public static Boolean EvaluateValueOrMethod(
            Object Obj,
            String FieldOrMethodName,
            Int32 ParametersCount,
            PainContext PainContext)
        {
            Boolean seekInObject = (Obj != null && !(Obj is EmptyObject));

            if (seekInObject)
            {
                Boolean foundValue = false;

                Object value = GetValueFromObject(
                    Obj,
                    FieldOrMethodName,
                    ParametersCount,
                    out foundValue);

                if (foundValue)
                {
                    if (value is MethodInfo)
                    {
                        OnpMethodInfo methodInfo = new OnpMethodInfo();
                        methodInfo.Obj = Obj;
                        methodInfo.Name = FieldOrMethodName;
                        PainContext.CurrentExpressionState.PushValue(methodInfo);
                        return false;
                    }
                    else
                    {
                        PainContext.CurrentExpressionState.PushValue(value);
                        return false;
                    }
                }
            }

            ExpressionValue expressionValue = null;
            Expression expression = null;

            expression = PainContext.
                CurrentExpressionGroup.
                FindExpression(FieldOrMethodName, PainContext);

            if (expression == null)
            {
                expressionValue = PainContext.GetValue(
                    PainContext,
                    FieldOrMethodName,
                    seekInObject,
                    !seekInObject,
                    !seekInObject);
            }

            if (expression == null)
            {
                Object value = expressionValue == null ?
                    null :
                    expressionValue.Value;

                value = InternalTypeConverter.ToInner(
                    value);

                PainContext.CurrentExpressionState.PushValue(value);
                return false;
            }
            else 
            {
                ExpressionState newExpressionState = new ExpressionState();
                newExpressionState.Expression = expression;

                PainContext.CurrentExpressionContext.Stack.Add(newExpressionState);
                return false;
            }
        }

    }
}