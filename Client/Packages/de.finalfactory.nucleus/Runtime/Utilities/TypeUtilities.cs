using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using FinalFactory.Tooling;
using JetBrains.Annotations;

namespace FinalFactory.Utilities
{
    [PublicAPI]
    [DebuggerStepThrough]
    public static class TypeUtilities
    {
        private const BindingFlags AccessFlags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;
        public static void SetValue(this PropertyInfo info, object obj, object value, object[] index)
        {
            info.SetValue(obj, value, BindingFlagsExt.Default, null, index);
        }
        public static void SetValue(this PropertyInfo info, object obj, object value, BindingFlagsExt invokeAttr, Binder binder, object[] index = null, CultureInfo culture = null)
        {
            MethodInfo setMethod = info.GetSetMethodFlatten(true);
            if (setMethod == null)
            {
                if (invokeAttr.HasFlag(BindingFlagsExt.DeepSearch))
                {
                    var type = info.DeclaringType;
                    while (type != null && type != typeof(object))
                    {
                        var field = type.GetRuntimeFields().FirstOrDefault(a => Regex.IsMatch( a.Name, $"\\A<{info.Name }>k__BackingField\\Z" ));
                        if (field != null)
                        {
                            field.SetValue(obj, value, (BindingFlags) invokeAttr, binder, culture);
                            return;
                        }
                        type = type.BaseType;
                    }
                }
                throw new ArgumentException("Set Method not found for '" + info.Name + "'");
            }
           
            object[] parameters;
            if (index == null || index.Length == 0)
            {
                parameters = new object[1]{ value };
            }
            else
            {
                int length = index.Length;
                parameters = new object[length + 1];
                index.CopyTo(parameters, 0);
                parameters[length] = value;
            }
            setMethod.Invoke(obj, (BindingFlags) invokeAttr, binder, parameters, culture);
        }
        
        [MethodImpl(GlobalConst.ImplOptions)]
        public static MethodInfo GetSetMethodFlatten(this PropertyInfo info, bool nonPublic)
        {
            var setter = info.GetSetMethod(nonPublic);
            if (setter == null && info.DeclaringType != null)
            {
                setter = info.DeclaringType.GetProperty(info.Name, BindingFlags.DeclaredOnly)
                    ?.GetSetMethod(nonPublic);
                if (setter == null)
                {
                    setter = info.DeclaringType.BaseType?.GetProperty(info.Name, BindingFlags.DeclaredOnly)
                        ?.GetSetMethod(nonPublic);
                }
            }
            return setter;
        }
        
        public static object GetValue(this MemberInfo member, object obj)
        {
            switch (member)
            {
                case FieldInfo info:
                    return info.GetValue(obj);
                case PropertyInfo info:
                    return info.GetValue(obj);
                default:
                    throw new ArgumentException("Can't get the value of a " + member.GetType().Name);
            }
        }

        public static void SetValue(this MemberInfo member, object obj, object value, BindingFlagsExt invokeAttr, object[] index = null)
        {
            switch (member)
            {
                case FieldInfo info:
                    info.SetValue(obj, value);
                    break;
                case PropertyInfo info:
                    info.SetValue(obj, value, invokeAttr, null, index);
                    break;
                default:
                    throw new ArgumentException("Can't set the value of a " + member.GetType().Name);
            }
        }
        
        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool HasAttribute<T>(this ICustomAttributeProvider member) where T : Attribute => member.GetCustomAttributes(typeof(T), true).Length > 0;
        
        public static object Call(this object obj, string methodName, params object[] parameters)
        {
            var method = obj.GetType().GetMethod(methodName, AccessFlags);
            Check.NotNull(method, $"Method[{methodName}] not found in {obj}");
            return method.Invoke(obj, parameters);
        }
        
        public static T Call<T>(this object obj, string methodName, params object[] parameters)
        {
            return (T) Call(obj, methodName, parameters);
        }
        
        /// <summary>
        /// Creates a delegate of the specified type from the specified method.
        /// </summary>
        /// <param name="methodInfo"></param>
        /// <typeparam name="T">This type must be a delegate type. The delegate must match the signature of the method.</typeparam>
        /// <returns></returns>
        [DebuggerStepThrough]
        [MethodImpl(GlobalConst.ImplOptions)]
        public static T CreateDelegate<T>(this MethodInfo methodInfo) where T : Delegate => (T) Delegate.CreateDelegate(typeof(T), methodInfo);
    }
}