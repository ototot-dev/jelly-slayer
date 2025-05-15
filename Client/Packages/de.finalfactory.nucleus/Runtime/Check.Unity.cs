using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace FinalFactory
{
    public static partial class Check
    {
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static void GetComponentSafe<T>(this Component component, out T result)
        {
            result = component.GetComponent<T>();
            NotNull(result, $"Component {typeof(T)} not found on {component.name}.");
        }
        
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static void GetComponentInParentSafe<T>(this Component component, out T result)
        {
            result = component.GetComponentInParent<T>();
            NotNull(result, $"Component {typeof(T)} not found on {component.transform.parent.name} search from {component.name}.");
        }
        
        [MethodImpl(GlobalConst.ImplOptions)]
        [DebuggerStepThrough]
        public static void GetComponentInChildrenSafe<T>(this Component component, out T result)
        {
            result = component.GetComponentInChildren<T>();
            NotNull(result, $"Component {typeof(T)} not found in {component.name} children.");
        }
    }
}