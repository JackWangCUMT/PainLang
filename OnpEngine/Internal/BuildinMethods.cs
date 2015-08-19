using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PainLang;
using System.Threading;
using System.Collections;
using PainLang.OnpEngine.Models;
using PainLang.Helpers;
using PainLang.OnpEngine.InternalMethods;
using PainLang.OnpEngine.Logic;
using PainLang.PainEngine.Compiler;
using PainLang.PainEngine.Classes;

namespace PainLang.OnpEngine.Internal
{
    public static class BuildinMethods
    {
        private static Dictionary<String, ExpressionMethod> methodsByNames;

        private static Dictionary<Guid, ExpressionMethod> methodsByIds;

        private static Object lck = new Object();

        ////////////////////////////////////////////////////////////////////


        public static ExpressionMethodInfo FindByName(String Name)
        {
            Init();
            ExpressionMethod expressionMethod = null;
            if (methodsByNames.TryGetValue(Name, out expressionMethod))
                return new ExpressionMethodInfo() { ID = expressionMethod.ID };
            return null;
        }

        public static void Add(ExpressionMethod Method)
        {
            Init();
            lock (lck)
            {
                if (!methodsByIds.ContainsKey(Method.ID))
                {
                    foreach (String name in Method.OperationNames)
                        methodsByNames[name.ToUpper()] = Method;
                    methodsByIds[Method.ID] = Method;
                }
            }
        }

        public static ExpressionMethod GetByID(Guid ID)
        {
            Init();
            ExpressionMethod expressionMethod = null;
            if (methodsByIds.TryGetValue(ID, out expressionMethod))
                return expressionMethod;
            return null;
        }

        public static void Init()
        {
            if (methodsByIds == null)
                lock (lck)
                    if (methodsByIds == null)
                    {
                        methodsByIds = new Dictionary<Guid, ExpressionMethod>();
                        methodsByNames = new Dictionary<String, ExpressionMethod>();
                        foreach (ExpressionMethod operation in BuildMethods())
                        {
                            foreach (String name in operation.OperationNames)
                                methodsByNames[name.ToUpper()] = operation;
                            methodsByIds[operation.ID] = operation;
                        }
                    }
        }

        ////////////////////////////////////////////////////////////////////

        private static IEnumerable<ExpressionMethod> BuildMethods()
        {
            yield return new ExpressionMethod()
            {
                OperationNames = new[] { MethodSetValue.Name },
                CalculateValueDelegate = MethodSetValue.Execute
            };
            yield return new ExpressionMethod()
            {
                OperationNames = new[] { "check" },
                CalculateValueDelegate = (PainContext, Parameters) =>
                {
                    Object condition = Parameters != null && Parameters.Count > 0 ? Parameters[0] : null;
                    Object ifTrue = Parameters != null && Parameters.Count > 1 ? Parameters[1] : null;
                    Object ifFalse = Parameters != null && Parameters.Count > 2 ? Parameters[2] : null;

                    if (condition != null)
                    {
                        Type conditionType = condition.GetType();

                        if ((conditionType.IsNumeric() && Convert.ToDecimal(condition) > 0) ||
                            (conditionType == typeof(String) && Convert.ToString(condition).Length > 0))
                        {
                            return new ExpressionMethodResult(ifTrue);
                        }
                        else
                        {
                            return new ExpressionMethodResult(ifFalse);
                        }
                    }

                    return new ExpressionMethodResult(null);
                }
            };
            yield return new ExpressionMethod()
            {
                OperationNames = new[] { "getdatetime" },
                CalculateValueDelegate = (PainContext, Parameters) =>
                {
                    return new ExpressionMethodResult(DateTime.Now);
                }
            };
            yield return new ExpressionMethod()
            {
                OperationNames = new[] { "getdate" },
                CalculateValueDelegate = (PainContext, Parameters) =>
                {
                    return new ExpressionMethodResult(DateTime.Now.Date);
                }
            };
            yield return new ExpressionMethod()
            {
                OperationNames = new[] { "return" },
                CalculateValueDelegate = (PainContext, Parameters) =>
                {
                    return new ExpressionMethodResult(Parameters.FirstOrDefault());
                }
            };
            yield return new ExpressionMethod()
            {
                OperationNames = new[] { "substring" },
                CalculateValueDelegate = (PainContext, Parameters) =>
                {
                    if (Parameters.Count > 0)
                    {
                        String str = UniConvert.ToUniString(Parameters[0] ?? "");
                        if (Parameters.Count > 2)
                        {
                            Int32 start = Convert.ToInt32(Parameters[1]);
                            Int32 len = Convert.ToInt32(Parameters[2]);
                            if (start < str.Length)
                            {
                                if (start + len <= str.Length)
                                {
                                    return new ExpressionMethodResult(str.Substring(start, len));
                                }
                                else
                                {
                                    return new ExpressionMethodResult(str.Substring(start));
                                }
                            }
                            return new ExpressionMethodResult("");
                        }
                        else if (Parameters.Count == 2)
                        {
                            Int32 start = Convert.ToInt32(Parameters[1]);
                            if (start < str.Length)
                            {
                                return new ExpressionMethodResult(str.Substring(start));
                            }
                            return new ExpressionMethodResult("");
                        }
                        return new ExpressionMethodResult(str);
                    }
                    return new ExpressionMethodResult("");
                }
            };
            yield return new ExpressionMethod()
            {
                OperationNames = new[] { "split" },
                CalculateValueDelegate = (PainContext, Parameters) =>
                {
                    if (Parameters.Count > 0)
                    {
                        List<String> list = new List<String>();
                        String str = UniConvert.ToUniString(Parameters[0] ?? "");
                        if (Parameters.Count > 1)
                        {
                            List<String> splitParams = new List<String>();
                            for (Int32 i = 1; i < Parameters.Count; i++)
                                splitParams.Add(UniConvert.ToString(Parameters[i] ?? ""));

                            foreach (String item in str.Split(splitParams.ToArray(), StringSplitOptions.None))
                                list.Add(item);
                        }
                        else
                        {
                            list.Add(str);
                        }
                        return new ExpressionMethodResult(list);
                    }
                    return new ExpressionMethodResult("");
                }
            };
            yield return new ExpressionMethod()
            {
                OperationNames = new[] { "str" },
                CalculateValueDelegate = (PainContext, Parameters) =>
                {
                    if (Parameters.Count > 0)
                    {
                        return new ExpressionMethodResult(
                            UniConvert.ToString(Parameters[0]));
                    }
                    return new ExpressionMethodResult(0);
                }
            };
            yield return new ExpressionMethod()
            {
                OperationNames = new[] { "ceil" },
                CalculateValueDelegate = (PainContext, Parameters) =>
                {
                    if (Parameters.Count > 0)
                    {
                        return new ExpressionMethodResult(
                            UniConvert.ToDecimal(Math.Ceiling(UniConvert.ToDouble(Parameters[0]))));
                    }
                    return new ExpressionMethodResult(0);
                }
            };
            yield return new ExpressionMethod()
            {
                OperationNames = new[] { "eval" },
                CalculateValueDelegate = (PainContext, Parameters) =>
                {
                    if (Parameters.Count > 0)
                    {
                        PainProgram program = new PainCompiler().Compile(
                            UniConvert.ToString(Parameters.First()));

                        EvaluatorForMethods.EvaluateMethod(
                            null,
                            program,
                            null,
                            PainContext);

                        return new ExpressionMethodResult()
                        {
                            NewContextCreated = true
                        };
                    }
                    return new ExpressionMethodResult(null);
                }
            };
            /*yield return new ExpressionMethod()
            {
                OperationNames = new[] { "bindtoevent" },
                CalculateValueDelegate = (PainContext, Parameters) =>
                {
                    if (Parameters.Count > 0)
                    {
                        Object obj = Parameters != null && Parameters.Count > 0 ? Parameters[0] : null;
                        String eventName = ((Parameters != null && Parameters.Count > 1 ? Parameters[1] : null) ?? "").ToString().Trim();
                        Object method = Parameters != null && Parameters.Count > 2 ? Parameters[2] : null;

                        throw new NotImplementedException();

                        MyReflectionHelper.BindToEvent(
                            obj,
                            eventName,
                            (s, o) =>
                            {

                            });
                    }
                    return new ExpressionMethodResult(null);
                }
            };*/
            /*yield return new ExpressionMethod()
            {
                OperationNames = new[] { "create" },
                CalculateValueDelegate = (PainContext, Parameters) =>
                {
                    if (Parameters.Count <= 0)
                        return new ExpressionMethodResult(null);

                    String typeName = UniConvert.ToString(Parameters[0]) ?? "";

                    foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        var type = assembly.GetType(typeName);
                        if (type == null)
                            continue;

                        if (type.IsStatic())
                        {
                            return null;
                        }
                        else
                        {
                            var r = Activator.CreateInstance(type);
                            return new ExpressionMethodResult(r);
                        }
                    }

                    return new ExpressionMethodResult(null);
                }
            };*/
            yield return new ExpressionMethod()
            {
                OperationNames = new[] { "type" },
                CalculateValueDelegate = (PainContext, Parameters) =>
                {
                    if (Parameters.Count <= 0)
                        return new ExpressionMethodResult(null);

                    String typeName = UniConvert.ToString(Parameters[0]) ?? "";
                    var type = MyAssemblyHelper.FindType(typeName);
                    return new ExpressionMethodResult(type);
                }
            };
#if !PCL
            yield return new ExpressionMethod()
            {
                OperationNames = new[] { "sleep" },
                CalculateValueDelegate = (PainContext, Parameters) =>
                {
                    if (Parameters.Count > 0)
                    {
                        Int32 milliseconds = UniConvert.ToInt32(Parameters[0]);
                        Thread.Sleep(milliseconds);
                        return new ExpressionMethodResult(milliseconds);
                    }
                    return new ExpressionMethodResult(0);
                }
            };
#endif
            yield return new ExpressionMethod()
            {
                OperationNames = new[] { "new" },
                CalculateValueDelegate = (PainContext, Parameters) =>
                {
                    if (Parameters.Count <= 0)
                        return new ExpressionMethodResult(null);

                    String typeName = UniConvert.ToString(Parameters[0]) ?? "";
                    var obj = MyAssemblyHelper.CreateType(typeName);
                    return new ExpressionMethodResult(obj);
                }
            };
            yield return new ExpressionMethod()
            {
                OperationNames = new[] { "typeof" },
                CalculateValueDelegate = (PainContext, Parameters) =>
                {
                    if (Parameters.Count > 0)
                    {
                        if (Parameters.First() != null)
                            return new ExpressionMethodResult(
                                Parameters.First().GetType());
                    }
                    return new ExpressionMethodResult(null);
                }
            };
            yield return new ExpressionMethod()
            {
                OperationNames = new[] { "istypeof" },
                CalculateValueDelegate = (PainContext, Parameters) =>
                {
                    Boolean result = false;
                    if (Parameters.Count > 1)
                    {
                        Object obj = Parameters[0];
                        Object type = Parameters[1];

                        if (obj != null && type != null)
                        {
                            if (type is Type)
                            {
                                result = obj.GetType().Is((Type)type);
                            }
                            else
                            {
                                String typeName = UniConvert.ToString(type);                                
                                result = obj.GetType().Is(typeName);
                            }
                        }
                    }
                    return new ExpressionMethodResult(result);
                }
            };
            yield return new ExpressionMethod()
            {
                OperationNames = new[] { "isnottypeof" },
                CalculateValueDelegate = (PainContext, Parameters) =>
                {
                    Boolean result = true;
                    if (Parameters.Count > 1)
                    {
                        Object obj = Parameters[0];
                        Object type = Parameters[1];

                        if (obj != null && type != null)
                        {
                            if (type is Type)
                            {
                                result = !obj.GetType().Is((Type)type);
                            }
                            else
                            {
                                String typeName = UniConvert.ToString(type);
                                result = !obj.GetType().Is(typeName);
                            }
                        }
                    }
                    return new ExpressionMethodResult(result);
                }
            };
            yield return new ExpressionMethod()
            {
                OperationNames = new[] { "mod" },
                CalculateValueDelegate = (PainContext, Parameters) =>
                {
                    if (Parameters.Count > 1)
                    {
                        return new ExpressionMethodResult(
                            UniConvert.ToDecimal(
                                UniConvert.ToDecimal(Parameters[0]) % UniConvert.ToDecimal(Parameters[1])));
                    }
                    return new ExpressionMethodResult(0);
                }
            };
            yield return new ExpressionMethod()
            {
                OperationNames = new[] { "float", "double" },
                CalculateValueDelegate = (PainContext, Parameters) =>
                {
                    if (Parameters.Count > 0)
                    {
                        return new ExpressionMethodResult(
                            UniConvert.ToDecimal(Parameters[0]));
                    }
                    return new ExpressionMethodResult(0);
                }
            };
            yield return new ExpressionMethod()
            {
                OperationNames = new[] { "int" },
                CalculateValueDelegate = (PainContext, Parameters) =>
                {
                    if (Parameters.Count > 0)
                    {
                        return new ExpressionMethodResult(
                            UniConvert.ToInt64(Parameters[0]));
                    }
                    return new ExpressionMethodResult(0);
                }
            };
            yield return new ExpressionMethod()
            {
                OperationNames = new[] { "floor" },
                CalculateValueDelegate = (PainContext, Parameters) =>
                {
                    if (Parameters.Count > 0)
                    {
                        return new ExpressionMethodResult(
                            UniConvert.ToDecimal(
                                Math.Floor(
                                    UniConvert.ToDouble(Parameters[0]))));
                    }
                    return new ExpressionMethodResult(0);
                }
            };
            yield return new ExpressionMethod()
            {
                OperationNames = new[] { "round" },
                CalculateValueDelegate = (PainContext, Parameters) =>
                {
                    if (Parameters.Count > 0)
                    {
                        if (Parameters.Count == 1)
                        {
                            return new ExpressionMethodResult(
                                UniConvert.ToDecimal(
                                    Math.Round(
                                        UniConvert.ToDouble(Parameters[0]))));
                        }
                        else
                        {
                            return new ExpressionMethodResult(
                                UniConvert.ToDecimal(
                                    Math.Round(
                                        UniConvert.ToDouble(Parameters[0]),
                                        Convert.ToInt32(Parameters[1]))));
                        }
                    }
                    return new ExpressionMethodResult(0);
                }
            };
            yield return new ExpressionMethod()
            {
                OperationNames = new[] { "coalesce" },
                CalculateValueDelegate = (PainContext, Parameters) =>
                {
                    if (Parameters.Count > 0)
                    {
                        var value = Parameters[0];
                        if (value == null)
                        {
                            for (var i = 1; i < Parameters.Count; i++)
                            {
                                if (Parameters[i] != null)
                                    return new ExpressionMethodResult(Parameters[i]);
                            }
                        }
                        return new ExpressionMethodResult(value);
                    }
                    return new ExpressionMethodResult(null);
                }
            };
            yield return new ExpressionMethod()
            {
                OperationNames = new[] { "not" },
                CalculateValueDelegate = (PainContext, Parameters) =>
                {
                    if (Parameters.Count > 0)
                    {
                        Object val = (Parameters[0] ?? "");
                        if (val != null)
                        {
                            Type type = val.GetType();
                            if (MyTypeHelper.IsNumeric(type))
                            {
                                Decimal numVal = UniConvert.ToDecimal(val);
                                return new ExpressionMethodResult(Math.Sign(numVal) == 0 ? true : false);
                            }
                            else if (type.Name == "String")
                            {
                                return new ExpressionMethodResult(
                                    String.IsNullOrEmpty(
                                        (String)val));
                            }
                            else
                            {
                                return new ExpressionMethodResult(false);
                            }
                        }
                        else
                        {
                            return new ExpressionMethodResult(true);
                        }
                    }
                    return new ExpressionMethodResult(null);
                }
            };
            yield return new ExpressionMethod()
            {
                OperationNames = new[] { "getyear" },
                CalculateValueDelegate = (PainContext, Parameters) =>
                {
                    if (Parameters.Count > 0)
                    {
                        Object val = Parameters[0];
                        if (val != null && (val.GetType() == typeof(DateTime) || val.GetType() == typeof(DateTime?)))
                            return new ExpressionMethodResult(((DateTime)val).Year);
                    }
                    return new ExpressionMethodResult(0);
                }
            };
            yield return new ExpressionMethod()
            {
                OperationNames = new[] { "len", "length" },
                CalculateValueDelegate = (PainContext, Parameters) =>
                {
                    if (Parameters.Count > 0)
                    {
                        String val = Convert.ToString(Parameters[0] ?? "");
                        return new ExpressionMethodResult(val.Length);
                    }
                    return new ExpressionMethodResult(0);
                }
            };
            yield return new ExpressionMethod()
            {
                OperationNames = new[] { "getmonth" },
                CalculateValueDelegate = (PainContext, Parameters) =>
                {
                    if (Parameters.Count > 0)
                    {
                        Object val = Parameters[0];
                        if (val != null && (val.GetType() == typeof(DateTime) || val.GetType() == typeof(DateTime?)))
                            return new ExpressionMethodResult(((DateTime)val).Month);
                    }
                    return new ExpressionMethodResult(0);
                }
            };
            yield return new ExpressionMethod()
            {
                OperationNames = new[] { "getday" },
                CalculateValueDelegate = (PainContext, Parameters) =>
                {
                    if (Parameters.Count > 0)
                    {
                        Object val = Parameters[0];
                        if (val != null && (val.GetType() == typeof(DateTime) || val.GetType() == typeof(DateTime?)))
                            return new ExpressionMethodResult(((DateTime)val).Day);
                    }
                    return new ExpressionMethodResult(0);
                }
            };
            yield return new ExpressionMethod()
            {
                OperationNames = new[] { "hours" },
                CalculateValueDelegate = (PainContext, Parameters) =>
                {
                    if (Parameters.Count > 0)
                    {
                        Object val = Parameters[0];
                        if (val != null && (val.GetType() == typeof(TimeSpan) || val.GetType() == typeof(TimeSpan?)))
                            return new ExpressionMethodResult(Convert.ToDecimal(((TimeSpan)val).TotalHours));
                        if (val != null && (val.GetType() == typeof(DateTime) || val.GetType() == typeof(DateTime?)))
                            return new ExpressionMethodResult(Convert.ToDecimal(TimeSpan.FromTicks(((DateTime)val).Ticks).TotalHours));
                    }
                    return new ExpressionMethodResult(0);
                }
            };
            yield return new ExpressionMethod()
            {
                OperationNames = new[] { "days" },
                CalculateValueDelegate = (PainContext, Parameters) =>
                {
                    if (Parameters.Count > 0)
                    {
                        Object val = Parameters[0];
                        if (val != null && (val.GetType() == typeof(TimeSpan) || val.GetType() == typeof(TimeSpan?)))
                            return new ExpressionMethodResult(Convert.ToDecimal(((TimeSpan)val).TotalDays));
                        if (val != null && (val.GetType() == typeof(DateTime) || val.GetType() == typeof(DateTime?)))
                            return new ExpressionMethodResult(Convert.ToDecimal(TimeSpan.FromTicks(((DateTime)val).Ticks).TotalDays));
                    }
                    return new ExpressionMethodResult(0);
                }
            };
            yield return new ExpressionMethod()
            {
                OperationNames = new[] { "minutes" },
                CalculateValueDelegate = (PainContext, Parameters) =>
                {
                    if (Parameters.Count > 0)
                    {
                        Object val = Parameters[0];
                        if (val != null && (val.GetType() == typeof(TimeSpan) || val.GetType() == typeof(TimeSpan?)))
                            return new ExpressionMethodResult(Convert.ToDecimal(((TimeSpan)val).TotalMinutes));
                        if (val != null && (val.GetType() == typeof(DateTime) || val.GetType() == typeof(DateTime?)))
                            return new ExpressionMethodResult(Convert.ToDecimal(TimeSpan.FromTicks(((DateTime)val).Ticks).TotalMinutes));
                    }
                    return new ExpressionMethodResult(0);
                }
            };
            yield return new ExpressionMethod()
            {
                OperationNames = new[] { "seconds" },
                CalculateValueDelegate = (PainContext, Parameters) =>
                {
                    if (Parameters.Count > 0)
                    {
                        Object val = Parameters[0];
                        if (val != null && (val.GetType() == typeof(TimeSpan) || val.GetType() == typeof(TimeSpan?)))
                            return new ExpressionMethodResult(Convert.ToDecimal(((TimeSpan)val).TotalSeconds));
                        if (val != null && (val.GetType() == typeof(DateTime) || val.GetType() == typeof(DateTime?)))
                            return new ExpressionMethodResult(Convert.ToDecimal(TimeSpan.FromTicks(((DateTime)val).Ticks).TotalSeconds));
                    }
                    return new ExpressionMethodResult(0);
                }
            };
            yield return new ExpressionMethod()
            {
                OperationNames = new[] { "todatetime" },
                CalculateValueDelegate = (PainContext, Parameters) =>
                {
                    DateTime date = new DateTime();
                    if (Parameters.Count > 0)
                    {
                        Object val = Parameters[0];
                        String str = UniConvert.ToUniString(val ?? "");
                        if (UniConvert.TryParseUniDateTime(str, out date)) { }
                    }
                    return new ExpressionMethodResult(date);
                }
            };
            yield return new ExpressionMethod()
            {
                OperationNames = new[] { "todate" },
                CalculateValueDelegate = (PainContext, Parameters) =>
                {
                    DateTime date = new DateTime();
                    if (Parameters.Count > 0)
                    {
                        Object val = Parameters[0];
                        String str = UniConvert.ToUniString(val ?? "");
                        if (UniConvert.TryParseUniDateTime(str, out date)) { }
                    }
                    return new ExpressionMethodResult(date.Date);
                }
            };
            yield return new ExpressionMethod()
            {
                OperationNames = new[] { "format" },
                CalculateValueDelegate = (PainContext, Parameters) =>
                {
                    String r = "";
                    if (Parameters.Count > 1)
                    {
                        Object val = Parameters[0];
                        String format = Convert.ToString(Parameters[1] ?? "");
                        if (val != null && (val is DateTime || val is DateTime?))
                        {
                            r = StringHelper.FormatDate((DateTime)val, format);
                        }
                    }
                    return new ExpressionMethodResult(r);
                }
            };
            yield return new ExpressionMethod()
            {
                OperationNames = new[] { "tostring" },
                CalculateValueDelegate = (PainContext, Parameters) =>
                {
                    if (Parameters.Count > 0)
                    {
                        var result = UniConvert.ToUniString(Parameters[0] ?? "");
                        if (Parameters.Count > 1)
                        {
                            var len = Convert.ToInt32(Parameters[1]);
                            var txt = (Convert.ToString(Parameters[2]) ?? "");
                            txt = txt.Replace("\"", "").Replace("'", "");

                            while (result.Length < len)
                            {
                                result = txt + result;
                            }
                        }
                        return new ExpressionMethodResult(result);
                    }
                    return new ExpressionMethodResult("");
                }
            };
            yield return new ExpressionMethod()
            {
                OperationNames = new[] { "sqrt" },
                CalculateValueDelegate = (PainContext, Parameters) =>
                {
                    try
                    {
                        if (Parameters.Count > 0)
                        {
                            Object val = Parameters[0];
                            Decimal result = UniConvert.ToDecimal(val);
                            return new ExpressionMethodResult((decimal)Math.Sqrt((double)result));
                        }
                    }
                    catch { }
                    return new ExpressionMethodResult(0);
                }
            };
            yield return new ExpressionMethod()
            {
                OperationNames = new[] { "adddays" },
                CalculateValueDelegate = (PainContext, Parameters) =>
                {
                    try
                    {
                        if (Parameters.Count > 0)
                        {
                            Object v = Parameters[0];
                            if (v != null && (v.GetType() == typeof(DateTime) || v.GetType() == typeof(DateTime?)))
                            {
                                DateTime date = (DateTime)v;
                                if (Parameters.Count > 0)
                                {
                                    var days = UniConvert.ToInt32(Parameters[1]);
                                    date = date.AddDays(days);
                                }
                                return new ExpressionMethodResult(date);
                            }
                        }
                        return new ExpressionMethodResult(
                            new DateTime());
                    }
                    catch
                    {
                        return new ExpressionMethodResult(0);
                    }
                }
            };
            yield return new ExpressionMethod()
            {
                OperationNames = new[] { "addyears" },
                CalculateValueDelegate = (PainContext, Parameters) =>
                {
                    try
                    {
                        if (Parameters.Count > 0)
                        {
                            Object v = Parameters[0];
                            if (v != null && (v.GetType() == typeof(DateTime) || v.GetType() == typeof(DateTime?)))
                            {
                                DateTime date = (DateTime)v;
                                if (Parameters.Count > 0)
                                {
                                    var years = UniConvert.ToInt32(Parameters[1]);
                                    date = date.AddYears(years);
                                }
                                return new ExpressionMethodResult(date);
                            }
                        }
                        return new ExpressionMethodResult(new DateTime());
                    }
                    catch
                    {
                        return new ExpressionMethodResult(0);
                    }
                }
            };
            yield return new ExpressionMethod()
            {
                OperationNames = new[] { "addmonths" },
                CalculateValueDelegate = (PainContext, Parameters) =>
                {
                    try
                    {
                        if (Parameters.Count > 0)
                        {
                            Object v = Parameters[0];
                            if (v != null && (v.GetType() == typeof(DateTime) || v.GetType() == typeof(DateTime?)))
                            {
                                DateTime date = (DateTime)v;
                                if (Parameters.Count > 0)
                                {
                                    var months = UniConvert.ToInt32(Parameters[1]);
                                    date = date.AddMonths(months);
                                }
                                return new ExpressionMethodResult(date);
                            }
                        }
                        return new ExpressionMethodResult(new DateTime());
                    }
                    catch
                    {
                        return new ExpressionMethodResult(0);
                    }
                }
            };
            yield return new ExpressionMethod()
            {
                OperationNames = new[] { "rand" },
                CalculateValueDelegate = (PainContext, Parameters) =>
                {
                    return new ExpressionMethodResult(
                        Convert.ToDecimal(
                            new Random(DateTime.Now.Millisecond).NextDouble()));
                }
            };
        }
    }
}