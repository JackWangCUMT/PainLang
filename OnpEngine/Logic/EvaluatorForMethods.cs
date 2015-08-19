using PainLang;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using PainLang.Exceptions;
using PainLang.PainEngine.Classes;
using PainLang.OnpEngine.Models;
using PainLang.Helpers;
using PainLang.OnpEngine.Internal;

namespace PainLang.OnpEngine.Logic
{
    public static class EvaluatorForMethods
    {
        public static Boolean EvaluateMethod(
            Object Object,
            Object MethodObject,
            IList<Object> Parameters,
            PainContext PainContext)
        {
            if (MethodObject is PainMethod)
            {
                if (Parameters == null)
                    Parameters = new Object[0];

                PainMethod method = (PainMethod)MethodObject;
                PainContextType contextType = PainContextType.METHOD;
                
                // jesli tworzenie klasy (wolanie konstruktora)
                if (MethodObject is PainClass)
                    contextType = PainContextType.CLASS;

                PainState newContext = PainContext.
                    PushContext(method, contextType, Parameters);

                newContext.Object.ParentObject = method.ParentObject;

                return true;
            }
            else if (MethodObject is PainProgram)
            {
                PainProgram program = (PainProgram)MethodObject;

                IDictionary<String, Object> currentValues = (PainContext == null || PainContext.CurrentState == null || PainContext.CurrentState.Object == null ?
                    null :
                    PainContext.
                    CurrentState.
                    Object.
                    DynamicValues);

                IDictionary<String, Object> currentStaticValues = (PainContext == null || PainContext.CurrentState == null || PainContext.CurrentState.Object == null ?
                    null :
                    PainContext.
                    CurrentState.
                    Object.
                    StaticValues);

                PainState newState = PainContext.PushContext(
                    program,
                    PainContextType.METHOD,
                    null);

                if (currentValues != null)
                    foreach (String key in currentValues.Keys)
                        newState.Object.DynamicValues[key] = currentValues[key];
                newState.Object.StaticValues = currentStaticValues;

                return true;
            }
            else
            {
                ExpressionMethodResult methodResult = EvaluateInlineMethod(
                    Object,
                    MethodObject,
                    Parameters,
                    PainContext);

                if (methodResult != null &&
                    methodResult.NewContextCreated)
                {
                    return true;
                }
                else
                {
                    var v = methodResult == null ? null : methodResult.Value;
                    PainContext.CurrentExpressionState.PushValue(v);
                    return false;
                }
            }
        }

        private static ExpressionMethodResult EvaluateInlineMethod(
            Object Object,
            Object Method,
            IList<Object> MethodParameters,
            PainContext PainContext)
        {
            if (Method is OnpMethodInfo)
            {
                OnpMethodInfo methodInfo = Method as OnpMethodInfo;

                DynamicCallResult callResult = MyReflectionHelper.CallMethod(
                    methodInfo.Obj,
                    methodInfo.Name,
                    MethodParameters);

                if (callResult != null)
                    return new ExpressionMethodResult(callResult.Value);
            }
            else if (Method is ExpressionMethod)
            {
                ExpressionMethod onpMethod = Method as ExpressionMethod;
                ExpressionMethodResult result = null;

                if (Object is EmptyObject)
                {
                    result = onpMethod.
                        CalculateValueDelegate(
                            PainContext,
                            MethodParameters);
                }
                else
                {
                    result = onpMethod.
                        CalculateValueDelegate(
                            PainContext,
                            new[] { Object }.Union(MethodParameters).ToArray());
                }

                return result == null ?
                    new ExpressionMethodResult(null) :
                    result;
            }
            else if (Method is ExpressionMethodInfo)
            {
                ExpressionMethodInfo onpMethodInfo = Method as ExpressionMethodInfo;
                ExpressionMethod onpMethod = BuildinMethods.GetByID(onpMethodInfo.ID);
                ExpressionMethodResult result = null;

                if (onpMethod == null)
                    return new ExpressionMethodResult(result);

                if (Object is EmptyObject)
                {
                    result = onpMethod.
                        CalculateValueDelegate(
                            PainContext,
                            MethodParameters);
                }
                else
                {
                    result = onpMethod.
                        CalculateValueDelegate(
                            PainContext,
                            new[] { Object }.Union(MethodParameters).ToArray());
                }

                return result == null ?
                    new ExpressionMethodResult(null) :
                    result;
            }
            else if (Method is ExpressionExtender)
            {
                ExpressionExtender onpExtender = Method as ExpressionExtender;

                return new ExpressionMethodResult(
                    onpExtender.
                        CalculateValueDelegate(
                            PainContext,
                            Object,
                            MethodParameters));
            }
            else if (Method is ExpressionExtenderInfo)
            {
                ExpressionExtenderInfo onpExtenderInfo = Method as ExpressionExtenderInfo;
                ExpressionExtender onpExtender = BuildinExtenders.GetByID(onpExtenderInfo.ID);

                if (onpExtender == null)
                    return new ExpressionMethodResult(null);

                return new ExpressionMethodResult(
                    onpExtender.
                        CalculateValueDelegate(
                            PainContext,
                            Object,
                            MethodParameters));
            }

            if (Method == null)
            {
                if (Object == null)
                {
                    throw new PainMethodNotFoundException("Cannot find a method to call");
                }
                else
                {
                    throw new PainMethodNotFoundException("Cannot find a method to call in object " + Object.GetType().Name + "");
                }
            }
            throw new PainUnsupportedMethodTypeException("Unsupported method type " + Method.GetType() + "!");
        }

        /*public static Boolean EvaluateValue(
            Object VariableValue,
            PainContext PainContext)
        {
            if (VariableValue == null || VariableValue is ExpressionValue)
            {
                Object value = VariableValue == null ? null : ((ExpressionValue)VariableValue).Value;

                value = InternalTypeConverter.ToInner(
                    value);

                PainContext.CurrentExpressionState.PushValue(value);
                return false;
            }
            else if (VariableValue is Expression)
            {
                Expression expression = (Expression)VariableValue;

                ExpressionState newExpressionState = new ExpressionState();
                newExpressionState.Expression = expression;

                PainContext.CurrentExpressionContext.Stack.Add(newExpressionState);
                return false;
            }
            else
            {
                PainContext.CurrentExpressionState.PushValue(VariableValue);
                return false;
            }
        }*/

    }

    public class OnpMethodInfo
    {
        public Object Obj { get; set; }

        public String Name { get; set; }
    }

}
