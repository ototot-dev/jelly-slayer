using System.Diagnostics;
using System.Linq;
using System.Reflection;
using FinalFactory.Utilities;
using JetBrains.Annotations;

namespace FinalFactory.Tooling
{
    [PublicAPI]
    [DebuggerStepThrough]
    public static class ClassTools
    {
        
        private const BindingFlags AccessFlags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;
        public static object GetValue(this object obj, string memberName)
        {
            return obj.GetType().GetMember(memberName, AccessFlags).FirstOrDefault().GetValue(obj);
        }
        
        public static T GetValue<T>(this object obj, string memberName)
        {
            return (T) GetValue(obj, memberName);
        }
        
        public static void SetValue(this object obj, string memberName, object value)
        {
            obj.GetType().GetMember(memberName, AccessFlags).FirstOrDefault().SetValue(obj, value, BindingFlagsExt.Default);
        }
    }
}