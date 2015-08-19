﻿#define NOT_CLASSIC_REFLECTION

using System;
using System.Collections;
using System.Reflection;
//using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;


//

namespace PainLang.Helpers
{
    public static class MyReflectionHelper
    {
        private static object _lck = new object();

        private static Dictionary<Type, Dictionary<String, PropertyInfo>> _propertyUppercaseCache = new Dictionary<Type, Dictionary<string, PropertyInfo>>();

        private static Dictionary<Type, Dictionary<String, PropertyInfo>> _propertyCache = new Dictionary<Type, Dictionary<string, PropertyInfo>>();

        private static Dictionary<Type, Dictionary<String, Dictionary<Int32, MethodInfo>>> _methodsCache = new Dictionary<Type, Dictionary<string, Dictionary<Int32, MethodInfo>>>();

        private static Dictionary<Type, Dictionary<String, Dictionary<Int32, MethodInfo>>> _methodsUppercaseCache = new Dictionary<Type, Dictionary<string, Dictionary<Int32, MethodInfo>>>();

        ////////////////////////////////////////

        public static Object GetValue(this Object Obj, String PropertyName)
        {
            if (Obj != null)
            {
                Type type = Obj.GetType();
                CacheProperties(type);

                // szukanie property
                PropertyInfo propertyInfo = null;

                if (_propertyCache[type].ContainsKey(PropertyName))
                {
                    propertyInfo = _propertyCache[type][PropertyName];
                }
                else if (_propertyUppercaseCache[type].ContainsKey(PropertyName.ToUpper()))
                {
                    propertyInfo = _propertyUppercaseCache[type][PropertyName.ToUpper()];
                }

                if (propertyInfo != null)
                {
                    return propertyInfo.GetValue(Obj, null);
                }
            }
            return null;
        }

        public static void BindToEvent(Object Object, String EventName, Action<object, object> Handler)
        {
            EventInfo eventInfo = Object.GetType().GetEvent(EventName);
            Delegate convertedHandler = ConvertDelegate(Handler, eventInfo.EventHandlerType);
            eventInfo.AddEventHandler(Object, convertedHandler);
        }

        public static Delegate ConvertDelegate(Delegate originalDelegate, Type targetDelegateType)
        {
            return Delegate.CreateDelegate(
                targetDelegateType,
                originalDelegate.Target,
                originalDelegate.Method);
        }

        public static void SetValue(this Object Obj, String PropertyName, Object Value)
        {
            if (Obj != null)
            {
                Object value = Value;
                Type type = Obj.GetType();
                CacheProperties(type);

                // szukanie property
                PropertyInfo propertyInfo = null;
                if (_propertyCache[type].ContainsKey(PropertyName))
                {
                    propertyInfo = _propertyCache[type][PropertyName];
                }
                else if (_propertyUppercaseCache[type].ContainsKey(PropertyName.ToUpper()))
                {
                    propertyInfo = _propertyUppercaseCache[type][PropertyName.ToUpper()];
                }

                if (propertyInfo != null)
                {
                    if (propertyInfo.PropertyType != Value.GetType())
                    {
                        value = MyTypeHelper.ConvertTo(value, propertyInfo.PropertyType);
                    }
                    propertyInfo.SetValue(Obj, value, null);
                }
            }
        }

        ////////////////////////////////////////

        public static DynamicCallResult CallMethod(this Object Obj, String MethodName, IList<Object> Parameters)
        {
            MethodInfo methodInfo = MyReflectionHelper.GetMethod(
                Obj,
                MethodName,
                Parameters == null ? 0 : Parameters.Count);

            if (methodInfo != null)
            {
                IList<ParameterInfo> parametersInfo = methodInfo.GetParameters();

                List<Object> finalParameters = new List<Object>();
                Int32 index = -1;
                foreach (ParameterInfo parameterInfo in parametersInfo)
                {
                    index++;

                    Object val = MyTypeHelper.ConvertTo(
                        Parameters[index],
                        parameterInfo.ParameterType);

                    finalParameters.Add(val);
                }

                return new DynamicCallResult()
                {
                    Value = Obj is Type ?
                        methodInfo.Invoke(null, finalParameters.ToArray()) :
                        methodInfo.Invoke(Obj, finalParameters.ToArray())
                };
            }
            return null;
        }

        public static MethodInfo GetMethod(this Object Item, String MethodName, Int32 ParameterCount = -1)
        {
            if (Item != null)
            {
                return GetMethod(
                    Item is Type ? (Type)Item : (Type)Item.GetType(),
                    MethodName,
                    ParameterCount);
            }
            return null;
        }

        public static MethodInfo GetMethod(this Type Type, String MethodName, Int32 ParameterCount = -1)
        {
            if (ParameterCount < 0)
                ParameterCount = -1;

            if (Type == null)
                return null;

            lock (_methodsUppercaseCache)
                if (!_methodsUppercaseCache.ContainsKey(Type))
                // lock (_methodsUppercaseCache)
                //   if (!_methodsUppercaseCache.ContainsKey(Type))
                {
                    _methodsCache[Type] = new Dictionary<string, Dictionary<int, MethodInfo>>();
                    _methodsUppercaseCache[Type] = new Dictionary<string, Dictionary<int, MethodInfo>>();

                    IList<MethodInfo> methods = Type.
                        GetMethods();

                    Int32 index = -1;
                    foreach (MethodInfo method in methods.OrderBy(m => m.GetParameters().Length))
                    {
                        index++;

                        if (!_methodsCache[Type].ContainsKey(method.Name))
                            _methodsCache[Type][method.Name] = new Dictionary<int, MethodInfo>();

                        if (!_methodsUppercaseCache[Type].ContainsKey(method.Name.ToUpper()))
                            _methodsUppercaseCache[Type][method.Name.ToUpper()] = new Dictionary<int, MethodInfo>();

                        if (index == 0)
                        {
                            _methodsCache[Type][method.Name][-1] = method;
                            _methodsUppercaseCache[Type][method.Name.ToUpper()][-1] = method;
                        }

                        Int32 parametersCount = method.GetParameters().Length;
                        if (!_methodsCache[Type][method.Name].ContainsKey(parametersCount))
                        {
                            _methodsCache[Type][method.Name][parametersCount] = method;
                            _methodsUppercaseCache[Type][method.Name.ToUpper()][parametersCount] = method;
                        }
                    }
                }

            Dictionary<string, Dictionary<int, MethodInfo>> dict = null;
            Dictionary<int, MethodInfo> innerDict = null;
            MethodInfo result = null;

            ////////////////////////////////////////////

            _methodsCache.TryGetValue(Type, out dict);

            if (dict != null)
                dict.TryGetValue(MethodName, out innerDict);

            if (innerDict != null)
                innerDict.TryGetValue(ParameterCount, out result);

            if (result != null)
                return result;

            ////////////////////////////////////////////

            _methodsUppercaseCache.TryGetValue(Type, out dict);

            if (dict != null)
                dict.TryGetValue(MethodName.ToUpper(), out innerDict);

            if (innerDict != null)
                innerDict.TryGetValue(ParameterCount, out result);

            if (result != null)
                return result;

            return null;
        }

        ////////////////////////////////////////

        /*public static Object Invoke(this Object Item, String MethodName, params Object[] Params)
        {
            if (Item != null)
            {
                var lMethod = Item.GetType().GetMethod(MethodName);
                if (lMethod != null)
                {
                    return lMethod.Invoke(Item, Params);
                }
            }
            return null;
        }*/

        public static Boolean ContainsProperty(Object Item, String PropertyName)
        {
            if (RefSensitiveHelper.I.GetGetter(Item, PropertyName) != null)
                return true;
            return RefUnsensitiveHelper.I.GetGetter(Item, PropertyName) != null;
        }

        public static void CopyTo(Object Source, Object Dest)
        {
            if (Source == null || Dest == null)
                return;

            foreach (String property in RefSensitiveHelper.I.GetProperties(Source))
                RefSensitiveHelper.I.SetValue(
                    Dest,
                    property,
                    RefSensitiveHelper.I.GetValue(Source, property));
        }

        ////////////////////////////////////////

        private static void CacheProperties(Type Type)
        {
            if (!_propertyCache.ContainsKey(Type))
            {
                lock (_lck)
                {
                    if (!_propertyCache.ContainsKey(Type))
                    {
                        _propertyCache[Type] = Type.GetProperties().ToDictionary(i => i.Name, i => i);
                        _propertyUppercaseCache[Type] = Type.GetProperties().ToDictionary(i => i.Name.ToUpper(), i => i);
                    }
                }
            }
        }
    }

    public class DynamicCallResult
    {
        public Object Value;
    }
}
