using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using Unity.Linq;
using UnityEngine;

namespace Game
{

    /// <summary>
    /// 
    /// </summary>
    public static class __Logger
    {
        
        /// <summary>
        /// 
        /// </summary>
        public static bool verboseEnabled;
        
        /// <summary>
        /// 
        /// </summary>
        static string Tag(object[] v, int index)
        {
            return $"{v[2 * index]}: {v[2 * index + 1]}";
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="reason"></param>
        /// <param name="vars"></param>
        public static void Verbose(GameObject context, params object[] vars) 
        {
            if (!verboseEnabled)
                return;

            if (vars == null || vars.Length == 0)
                Debug.Log($"?? '{context.name}'");
            else if (vars.Length / 2 == 1)
                Debug.Log($"?? '{context.name}' => {Tag(vars, 0)}");
            else if (vars.Length / 2 == 2)
                Debug.Log($"?? '{context.name}' => {Tag(vars, 0)}, {Tag(vars, 1)}");
            else if (vars.Length / 2 == 3)
                Debug.Log($"?? '{context.name}' => {Tag(vars, 0)}, {Tag(vars, 1)}, {Tag(vars, 2)}");
            else if (vars.Length / 2 == 4)
                Debug.Log($"?? '{context.name}' => {Tag(vars, 0)}, {Tag(vars, 1)}, {Tag(vars, 2)}, {Tag(vars, 3)}");
            else if (vars.Length / 2 == 5)
                Debug.Log($"?? '{context.name}' => {Tag(vars, 0)}, {Tag(vars, 1)}, {Tag(vars, 2)}, {Tag(vars, 3)}, {Tag(vars, 4)}");
            else if (vars.Length / 2 == 6)
                Debug.Log($"?? '{context.name}' => {Tag(vars, 0)}, {Tag(vars, 1)}, {Tag(vars, 2)}, {Tag(vars, 3)}, {Tag(vars, 4)}, {Tag(vars, 5)}");
            else if (vars.Length / 2 == 7)
                Debug.Log($"?? '{context.name}' => {Tag(vars, 0)}, {Tag(vars, 1)}, {Tag(vars, 2)}, {Tag(vars, 3)}, {Tag(vars, 4)}, {Tag(vars, 5)}, {Tag(vars, 6)}");
            else if (vars.Length / 2 == 8)
                Debug.Log($"?? '{context.name}' => {Tag(vars, 0)}, {Tag(vars, 1)}, {Tag(vars, 2)}, {Tag(vars, 3)}, {Tag(vars, 4)}, {Tag(vars, 5)}, {Tag(vars, 6)}, {Tag(vars, 7)}");
            else if (vars.Length / 2 == 9)
                Debug.Log($"?? '{context.name}' => {Tag(vars, 0)}, {Tag(vars, 1)}, {Tag(vars, 2)}, {Tag(vars, 3)}, {Tag(vars, 4)}, {Tag(vars, 5)}, {Tag(vars, 6)}, {Tag(vars, 7)}, {Tag(vars, 8)}");
            else
                Debug.Assert(false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="reason"></param>
        /// <param name="vars"></param>
        public static void VerboseR(GameObject context, string reason, params object[] vars) 
        {
            if (!verboseEnabled)
                return;
                
            if (vars == null || vars.Length == 0)
                Debug.Log($"?? '{context.name}': {reason}");
            else if (vars.Length / 2 == 1)
                Debug.Log($"?? '{context.name}': {reason} => {Tag(vars, 0)}");
            else if (vars.Length / 2 == 2)
                Debug.Log($"?? '{context.name}': {reason} => {Tag(vars, 0)}, {Tag(vars, 1)}");
            else if (vars.Length / 2 == 3)
                Debug.Log($"?? '{context.name}': {reason} => {Tag(vars, 0)}, {Tag(vars, 1)}, {Tag(vars, 2)}");
            else if (vars.Length / 2 == 4)
                Debug.Log($"?? '{context.name}': {reason} => {Tag(vars, 0)}, {Tag(vars, 1)}, {Tag(vars, 2)}, {Tag(vars, 3)}");
            else if (vars.Length / 2 == 5)
                Debug.Log($"?? '{context.name}': {reason} => {Tag(vars, 0)}, {Tag(vars, 1)}, {Tag(vars, 2)}, {Tag(vars, 3)}, {Tag(vars, 4)}");
            else if (vars.Length / 2 == 6)
                Debug.Log($"?? '{context.name}': {reason} => {Tag(vars, 0)}, {Tag(vars, 1)}, {Tag(vars, 2)}, {Tag(vars, 3)}, {Tag(vars, 4)}, {Tag(vars, 5)}");
            else if (vars.Length / 2 == 7)
                Debug.Log($"?? '{context.name}': {reason} => {Tag(vars, 0)}, {Tag(vars, 1)}, {Tag(vars, 2)}, {Tag(vars, 3)}, {Tag(vars, 4)}, {Tag(vars, 5)}, {Tag(vars, 6)}");
            else if (vars.Length / 2 == 8)
                Debug.Log($"?? '{context.name}': {reason} => {Tag(vars, 0)}, {Tag(vars, 1)}, {Tag(vars, 2)}, {Tag(vars, 3)}, {Tag(vars, 4)}, {Tag(vars, 5)}, {Tag(vars, 6)}, {Tag(vars, 7)}");
            else if (vars.Length / 2 == 9)
                Debug.Log($"?? '{context.name}': {reason} => {Tag(vars, 0)}, {Tag(vars, 1)}, {Tag(vars, 2)}, {Tag(vars, 3)}, {Tag(vars, 4)}, {Tag(vars, 5)}, {Tag(vars, 6)}, {Tag(vars, 7)}, {Tag(vars, 8)}");
            else
                Debug.Assert(false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="vars"></param>
        public static void Log(GameObject context, params object[] vars) 
        {
            if (vars == null || vars.Length == 0)
                Debug.Log($"@@ '{context.name}'");
            else if (vars.Length / 2 == 1)
                Debug.Log($"@@ '{context.name}' => {Tag(vars, 0)}");
            else if (vars.Length / 2 == 2)
                Debug.Log($"@@ '{context.name}' => {Tag(vars, 0)}, {Tag(vars, 1)}");
            else if (vars.Length / 2 == 3)
                Debug.Log($"@@ '{context.name}' => {Tag(vars, 0)}, {Tag(vars, 1)}, {Tag(vars, 2)}");
            else if (vars.Length / 2 == 4)
                Debug.Log($"@@ '{context.name}' => {Tag(vars, 0)}, {Tag(vars, 1)}, {Tag(vars, 2)}, {Tag(vars, 3)}");
            else if (vars.Length / 2 == 5)
                Debug.Log($"@@ '{context.name}' => {Tag(vars, 0)}, {Tag(vars, 1)}, {Tag(vars, 2)}, {Tag(vars, 3)}, {Tag(vars, 4)}");
            else if (vars.Length / 2 == 6)
                Debug.Log($"@@ '{context.name}' => {Tag(vars, 0)}, {Tag(vars, 1)}, {Tag(vars, 2)}, {Tag(vars, 3)}, {Tag(vars, 4)}, {Tag(vars, 5)}");
            else if (vars.Length / 2 == 7)
                Debug.Log($"@@ '{context.name}' => {Tag(vars, 0)}, {Tag(vars, 1)}, {Tag(vars, 2)}, {Tag(vars, 3)}, {Tag(vars, 4)}, {Tag(vars, 5)}, {Tag(vars, 6)}");
            else if (vars.Length / 2 == 8)
                Debug.Log($"@@ '{context.name}' => {Tag(vars, 0)}, {Tag(vars, 1)}, {Tag(vars, 2)}, {Tag(vars, 3)}, {Tag(vars, 4)}, {Tag(vars, 5)}, {Tag(vars, 6)}, {Tag(vars, 7)}");
            else if (vars.Length / 2 == 9)
                Debug.Log($"@@ '{context.name}' => {Tag(vars, 0)}, {Tag(vars, 1)}, {Tag(vars, 2)}, {Tag(vars, 3)}, {Tag(vars, 4)}, {Tag(vars, 5)}, {Tag(vars, 6)}, {Tag(vars, 7)}, {Tag(vars, 8)}");
            else
                Debug.Assert(false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="reason"></param>
        /// <param name="vars"></param>
        public static void LogR1(GameObject context, string reason, params object[] vars) 
        {
            if (vars == null || vars.Length == 0)
                Debug.Log($"@@ '{context.name}': {reason}");
            else if (vars.Length / 2== 1)
                Debug.Log($"@@ '{context.name}': {reason} => {Tag(vars, 0)}");
            else if (vars.Length / 2 == 2)
                Debug.Log($"@@ '{context.name}': {reason} => {Tag(vars, 0)}, {Tag(vars, 1)}");
            else if (vars.Length / 2 == 3)
                Debug.Log($"@@ '{context.name}': {reason} => {Tag(vars, 0)}, {Tag(vars, 1)}, {Tag(vars, 2)}");
            else if (vars.Length / 2 == 4)
                Debug.Log($"@@ '{context.name}': {reason} => {Tag(vars, 0)}, {Tag(vars, 1)}, {Tag(vars, 2)}, {Tag(vars, 3)}");
            else if (vars.Length / 2 == 5)
                Debug.Log($"@@ '{context.name}': {reason} => {Tag(vars, 0)}, {Tag(vars, 1)}, {Tag(vars, 2)}, {Tag(vars, 3)}, {Tag(vars, 4)}");
            else if (vars.Length / 2 == 6)
                Debug.Log($"@@ '{context.name}': {reason} => {Tag(vars, 0)}, {Tag(vars, 1)}, {Tag(vars, 2)}, {Tag(vars, 3)}, {Tag(vars, 4)}, {Tag(vars, 5)}");
            else if (vars.Length / 2 == 7)
                Debug.Log($"@@ '{context.name}': {reason} => {Tag(vars, 0)}, {Tag(vars, 1)}, {Tag(vars, 2)}, {Tag(vars, 3)}, {Tag(vars, 4)}, {Tag(vars, 5)}, {Tag(vars, 6)}");
            else if (vars.Length / 2 == 8)
                Debug.Log($"@@ '{context.name}': {reason} => {Tag(vars, 0)}, {Tag(vars, 1)}, {Tag(vars, 2)}, {Tag(vars, 3)}, {Tag(vars, 4)}, {Tag(vars, 5)}, {Tag(vars, 6)}, {Tag(vars, 7)}");
            else if (vars.Length / 2 == 9)
                Debug.Log($"@@ '{context.name}': {reason} => {Tag(vars, 0)}, {Tag(vars, 1)}, {Tag(vars, 2)}, {Tag(vars, 3)}, {Tag(vars, 4)}, {Tag(vars, 5)}, {Tag(vars, 6)}, {Tag(vars, 7)}, {Tag(vars, 8)}");
            else
                Debug.Assert(false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="reason1"></param>
        /// <param name="reason2"></param>
        /// <param name="vars"></param>
        public static void LogR2(GameObject context, string reason1, string reason2, params object[] vars) 
        {
            if (vars == null || vars.Length == 0)
                Debug.Log($"@@ '{context.name}.{reason1}()': {reason2}");
            else if (vars.Length / 2== 1)
                Debug.Log($"@@ '{context.name}.{reason1}()': {reason2} => {Tag(vars, 0)}");
            else if (vars.Length / 2 == 2)
                Debug.Log($"@@ '{context.name}.{reason1}()': {reason2} => {Tag(vars, 0)}, {Tag(vars, 1)}");
            else if (vars.Length / 2 == 3)
                Debug.Log($"@@ '{context.name}.{reason1}()': {reason2} => {Tag(vars, 0)}, {Tag(vars, 1)}, {Tag(vars, 2)}");
            else if (vars.Length / 2 == 4)
                Debug.Log($"@@ '{context.name}.{reason1}()': {reason2} => {Tag(vars, 0)}, {Tag(vars, 1)}, {Tag(vars, 2)}, {Tag(vars, 3)}");
            else if (vars.Length / 2 == 5)
                Debug.Log($"@@ '{context.name}.{reason1}()': {reason2} => {Tag(vars, 0)}, {Tag(vars, 1)}, {Tag(vars, 2)}, {Tag(vars, 3)}, {Tag(vars, 4)}");
            else if (vars.Length / 2 == 6)
                Debug.Log($"@@ '{context.name}.{reason1}()': {reason2} => {Tag(vars, 0)}, {Tag(vars, 1)}, {Tag(vars, 2)}, {Tag(vars, 3)}, {Tag(vars, 4)}, {Tag(vars, 5)}");
            else if (vars.Length / 2 == 7)
                Debug.Log($"@@ '{context.name}.{reason1}()': {reason2} => {Tag(vars, 0)}, {Tag(vars, 1)}, {Tag(vars, 2)}, {Tag(vars, 3)}, {Tag(vars, 4)}, {Tag(vars, 5)}, {Tag(vars, 6)}");
            else if (vars.Length / 2 == 8)
                Debug.Log($"@@ '{context.name}.{reason1}()': {reason2} => {Tag(vars, 0)}, {Tag(vars, 1)}, {Tag(vars, 2)}, {Tag(vars, 3)}, {Tag(vars, 4)}, {Tag(vars, 5)}, {Tag(vars, 6)}, {Tag(vars, 7)}");
            else if (vars.Length / 2 == 9)
                Debug.Log($"@@ '{context.name}.{reason1}()': {reason2} => {Tag(vars, 0)}, {Tag(vars, 1)}, {Tag(vars, 2)}, {Tag(vars, 3)}, {Tag(vars, 4)}, {Tag(vars, 5)}, {Tag(vars, 6)}, {Tag(vars, 7)}, {Tag(vars, 8)}");
            else
                Debug.Assert(false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="reason"></param>
        /// <param name="vars"></param>
        public static void Warning(GameObject context, params object[] vars) 
        {
            if (vars == null || vars.Length == 0)
                Debug.LogWarning($"@@ '{context.name}'");
            else if (vars.Length / 2 == 1)
                Debug.LogWarning($"@@ '{context.name}' => {Tag(vars, 0)}");
            else if (vars.Length / 2 == 2)
                Debug.LogWarning($"@@ '{context.name}' => {Tag(vars, 0)}, {Tag(vars, 1)}");
            else if (vars.Length / 2 == 3)
                Debug.LogWarning($"@@ '{context.name}' => {Tag(vars, 0)}, {Tag(vars, 1)}, {Tag(vars, 2)}");
            else if (vars.Length / 2 == 4)
                Debug.LogWarning($"@@ '{context.name}' => {Tag(vars, 0)}, {Tag(vars, 1)}, {Tag(vars, 2)}, {Tag(vars, 3)}");
            else if (vars.Length / 2 == 5)
                Debug.LogWarning($"@@ '{context.name}' => {Tag(vars, 0)}, {Tag(vars, 1)}, {Tag(vars, 2)}, {Tag(vars, 3)}, {Tag(vars, 4)}");
            else if (vars.Length / 2 == 6)
                Debug.LogWarning($"@@ '{context.name}' => {Tag(vars, 0)}, {Tag(vars, 1)}, {Tag(vars, 2)}, {Tag(vars, 3)}, {Tag(vars, 4)}, {Tag(vars, 5)}");
            else if (vars.Length / 2 == 7)
                Debug.LogWarning($"@@ '{context.name}' => {Tag(vars, 0)}, {Tag(vars, 1)}, {Tag(vars, 2)}, {Tag(vars, 3)}, {Tag(vars, 4)}, {Tag(vars, 5)}, {Tag(vars, 6)}");
            else if (vars.Length / 2 == 8)
                Debug.LogWarning($"@@ '{context.name}' => {Tag(vars, 0)}, {Tag(vars, 1)}, {Tag(vars, 2)}, {Tag(vars, 3)}, {Tag(vars, 4)}, {Tag(vars, 5)}, {Tag(vars, 6)}, {Tag(vars, 7)}");
            else if (vars.Length / 2 == 9)
                Debug.LogWarning($"@@ '{context.name}' => {Tag(vars, 0)}, {Tag(vars, 1)}, {Tag(vars, 2)}, {Tag(vars, 3)}, {Tag(vars, 4)}, {Tag(vars, 5)}, {Tag(vars, 6)}, {Tag(vars, 7)}, {Tag(vars, 8)}");
            else
                Debug.Assert(false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="reason"></param>
        /// <param name="vars"></param>
        public static void WarningR1(GameObject context, string reason, params object[] vars) 
        {
            if (vars == null || vars.Length == 0)
                Debug.LogWarning($"@@ '{context.name}': {reason}");
            else if (vars.Length / 2 == 1)
                Debug.LogWarning($"@@ '{context.name}': {reason} => {Tag(vars, 0)}");
            else if (vars.Length / 2 == 2)
                Debug.LogWarning($"@@ '{context.name}': {reason} => {Tag(vars, 0)}, {Tag(vars, 1)}");
            else if (vars.Length / 2 == 3)
                Debug.LogWarning($"@@ '{context.name}': {reason} => {Tag(vars, 0)}, {Tag(vars, 1)}, {Tag(vars, 2)}");
            else if (vars.Length / 2 == 4)
                Debug.LogWarning($"@@ '{context.name}': {reason} => {Tag(vars, 0)}, {Tag(vars, 1)}, {Tag(vars, 2)}, {Tag(vars, 3)}");
            else if (vars.Length / 2 == 5)
                Debug.LogWarning($"@@ '{context.name}': {reason} => {Tag(vars, 0)}, {Tag(vars, 1)}, {Tag(vars, 2)}, {Tag(vars, 3)}, {Tag(vars, 4)}");
            else if (vars.Length / 2 == 6)
                Debug.LogWarning($"@@ '{context.name}': {reason} => {Tag(vars, 0)}, {Tag(vars, 1)}, {Tag(vars, 2)}, {Tag(vars, 3)}, {Tag(vars, 4)}, {Tag(vars, 5)}");
            else if (vars.Length / 2 == 7)
                Debug.LogWarning($"@@ '{context.name}': {reason} => {Tag(vars, 0)}, {Tag(vars, 1)}, {Tag(vars, 2)}, {Tag(vars, 3)}, {Tag(vars, 4)}, {Tag(vars, 5)}, {Tag(vars, 6)}");
            else if (vars.Length / 2 == 8)
                Debug.LogWarning($"@@ '{context.name}': {reason} => {Tag(vars, 0)}, {Tag(vars, 1)}, {Tag(vars, 2)}, {Tag(vars, 3)}, {Tag(vars, 4)}, {Tag(vars, 5)}, {Tag(vars, 6)}, {Tag(vars, 7)}");
            else if (vars.Length / 2 == 9)
                Debug.LogWarning($"@@ '{context.name}': {reason} => {Tag(vars, 0)}, {Tag(vars, 1)}, {Tag(vars, 2)}, {Tag(vars, 3)}, {Tag(vars, 4)}, {Tag(vars, 5)}, {Tag(vars, 6)}, {Tag(vars, 7)}, {Tag(vars, 8)}");
            else
                Debug.Assert(false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="reason1"></param>
        /// <param name="reason2"></param>
        /// <param name="vars"></param>
        public static void WarningR2(GameObject context, string reason1, string reason2, params object[] vars) 
        {
            if (vars == null || vars.Length == 0)
                Debug.LogWarning($"@@ '{context.name}.{reason1}()': {reason2}");
            else if (vars.Length / 2== 1)
                Debug.LogWarning($"@@ '{context.name}.{reason1}()': {reason2} => {Tag(vars, 0)}");
            else if (vars.Length / 2 == 2)
                Debug.LogWarning($"@@ '{context.name}.{reason1}()': {reason2} => {Tag(vars, 0)}, {Tag(vars, 1)}");
            else if (vars.Length / 2 == 3)
                Debug.LogWarning($"@@ '{context.name}.{reason1}()': {reason2} => {Tag(vars, 0)}, {Tag(vars, 1)}, {Tag(vars, 2)}");
            else if (vars.Length / 2 == 4)
                Debug.LogWarning($"@@ '{context.name}.{reason1}()': {reason2} => {Tag(vars, 0)}, {Tag(vars, 1)}, {Tag(vars, 2)}, {Tag(vars, 3)}");
            else if (vars.Length / 2 == 5)
                Debug.LogWarning($"@@ '{context.name}.{reason1}()': {reason2} => {Tag(vars, 0)}, {Tag(vars, 1)}, {Tag(vars, 2)}, {Tag(vars, 3)}, {Tag(vars, 4)}");
            else if (vars.Length / 2 == 6)
                Debug.LogWarning($"@@ '{context.name}.{reason1}()': {reason2} => {Tag(vars, 0)}, {Tag(vars, 1)}, {Tag(vars, 2)}, {Tag(vars, 3)}, {Tag(vars, 4)}, {Tag(vars, 5)}");
            else if (vars.Length / 2 == 7)
                Debug.LogWarning($"@@ '{context.name}.{reason1}()': {reason2} => {Tag(vars, 0)}, {Tag(vars, 1)}, {Tag(vars, 2)}, {Tag(vars, 3)}, {Tag(vars, 4)}, {Tag(vars, 5)}, {Tag(vars, 6)}");
            else if (vars.Length / 2 == 8)
                Debug.LogWarning($"@@ '{context.name}.{reason1}()': {reason2} => {Tag(vars, 0)}, {Tag(vars, 1)}, {Tag(vars, 2)}, {Tag(vars, 3)}, {Tag(vars, 4)}, {Tag(vars, 5)}, {Tag(vars, 6)}, {Tag(vars, 7)}");
            else if (vars.Length / 2 == 9)
                Debug.LogWarning($"@@ '{context.name}.{reason1}()': {reason2} => {Tag(vars, 0)}, {Tag(vars, 1)}, {Tag(vars, 2)}, {Tag(vars, 3)}, {Tag(vars, 4)}, {Tag(vars, 5)}, {Tag(vars, 6)}, {Tag(vars, 7)}, {Tag(vars, 8)}");
            else
                Debug.Assert(false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="reason"></param>
        /// <param name="vars"></param>
        public static void Error(GameObject context, params object[] vars) 
        {
            if (vars == null || vars.Length == 0)
                Debug.LogError($"@@ '{context.name}'");
            else if (vars.Length / 2 == 1)
                Debug.LogError($"@@ '{context.name}' => {Tag(vars, 0)}");
            else if (vars.Length / 2 == 2)
                Debug.LogError($"@@ '{context.name}' => {Tag(vars, 0)}, {Tag(vars, 1)}");
            else if (vars.Length / 2 == 3)
                Debug.LogError($"@@ '{context.name}' => {Tag(vars, 0)}, {Tag(vars, 1)}, {Tag(vars, 2)}");
            else if (vars.Length / 2 == 4)
                Debug.LogError($"@@ '{context.name}' => {Tag(vars, 0)}, {Tag(vars, 1)}, {Tag(vars, 2)}, {Tag(vars, 3)}");
            else if (vars.Length / 2 == 5)
                Debug.LogError($"@@ '{context.name}' => {Tag(vars, 0)}, {Tag(vars, 1)}, {Tag(vars, 2)}, {Tag(vars, 3)}, {Tag(vars, 4)}");
            else if (vars.Length / 2 == 6)
                Debug.LogError($"@@ '{context.name}' => {Tag(vars, 0)}, {Tag(vars, 1)}, {Tag(vars, 2)}, {Tag(vars, 3)}, {Tag(vars, 4)}, {Tag(vars, 5)}");
            else if (vars.Length / 2 == 7)
                Debug.LogError($"@@ '{context.name}' => {Tag(vars, 0)}, {Tag(vars, 1)}, {Tag(vars, 2)}, {Tag(vars, 3)}, {Tag(vars, 4)}, {Tag(vars, 5)}, {Tag(vars, 6)}");
            else if (vars.Length / 2 == 8)
                Debug.LogError($"@@ '{context.name}' => {Tag(vars, 0)}, {Tag(vars, 1)}, {Tag(vars, 2)}, {Tag(vars, 3)}, {Tag(vars, 4)}, {Tag(vars, 5)}, {Tag(vars, 6)}, {Tag(vars, 7)}");
            else if (vars.Length / 2 == 9)
                Debug.LogError($"@@ '{context.name}' => {Tag(vars, 0)}, {Tag(vars, 1)}, {Tag(vars, 2)}, {Tag(vars, 3)}, {Tag(vars, 4)}, {Tag(vars, 5)}, {Tag(vars, 6)}, {Tag(vars, 7)}, {Tag(vars, 8)}");
            else
                Debug.Assert(false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="reason"></param>
        /// <param name="vars"></param>
        public static void ErrorR1(GameObject context, string reason, params object[] vars) 
        {
            if (vars == null || vars.Length == 0)
                Debug.LogError($"@@ '{context.name}': {reason}");
            else if (vars.Length / 2 == 1)
                Debug.LogError($"@@ '{context.name}': {reason} => {Tag(vars, 0)}");
            else if (vars.Length / 2 == 2)
                Debug.LogError($"@@ '{context.name}': {reason} => {Tag(vars, 0)}, {Tag(vars, 1)}");
            else if (vars.Length / 2 == 3)
                Debug.LogError($"@@ '{context.name}': {reason} => {Tag(vars, 0)}, {Tag(vars, 1)}, {Tag(vars, 2)}");
            else if (vars.Length / 2 == 4)
                Debug.LogError($"@@ '{context.name}': {reason} => {Tag(vars, 0)}, {Tag(vars, 1)}, {Tag(vars, 2)}, {Tag(vars, 3)}");
            else if (vars.Length / 2 == 5)
                Debug.LogError($"@@ '{context.name}': {reason} => {Tag(vars, 0)}, {Tag(vars, 1)}, {Tag(vars, 2)}, {Tag(vars, 3)}, {Tag(vars, 4)}");
            else if (vars.Length / 2 == 6)
                Debug.LogError($"@@ '{context.name}': {reason} => {Tag(vars, 0)}, {Tag(vars, 1)}, {Tag(vars, 2)}, {Tag(vars, 3)}, {Tag(vars, 4)}, {Tag(vars, 5)}");
            else if (vars.Length / 2 == 7)
                Debug.LogError($"@@ '{context.name}': {reason} => {Tag(vars, 0)}, {Tag(vars, 1)}, {Tag(vars, 2)}, {Tag(vars, 3)}, {Tag(vars, 4)}, {Tag(vars, 5)}, {Tag(vars, 6)}");
            else if (vars.Length / 2 == 8)
                Debug.LogError($"@@ '{context.name}': {reason} => {Tag(vars, 0)}, {Tag(vars, 1)}, {Tag(vars, 2)}, {Tag(vars, 3)}, {Tag(vars, 4)}, {Tag(vars, 5)}, {Tag(vars, 6)}, {Tag(vars, 7)}");
            else if (vars.Length / 2 == 9)
                Debug.LogError($"@@ '{context.name}': {reason} => {Tag(vars, 0)}, {Tag(vars, 1)}, {Tag(vars, 2)}, {Tag(vars, 3)}, {Tag(vars, 4)}, {Tag(vars, 5)}, {Tag(vars, 6)}, {Tag(vars, 7)}, {Tag(vars, 8)}");
            else
                Debug.Assert(false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="reason1"></param>
        /// <param name="reason2"></param>
        /// <param name="vars"></param>
        public static void ErrorR2(GameObject context, string reason1, string reason2, params object[] vars) 
        {
            if (vars == null || vars.Length == 0)
                Debug.LogError($"@@ '{context.name}.{reason1}()': {reason2}");
            else if (vars.Length / 2== 1)
                Debug.LogError($"@@ '{context.name}.{reason1}()': {reason2} => {Tag(vars, 0)}");
            else if (vars.Length / 2 == 2)
                Debug.LogError($"@@ '{context.name}.{reason1}()': {reason2} => {Tag(vars, 0)}, {Tag(vars, 1)}");
            else if (vars.Length / 2 == 3)
                Debug.LogError($"@@ '{context.name}.{reason1}()': {reason2} => {Tag(vars, 0)}, {Tag(vars, 1)}, {Tag(vars, 2)}");
            else if (vars.Length / 2 == 4)
                Debug.LogError($"@@ '{context.name}.{reason1}()': {reason2} => {Tag(vars, 0)}, {Tag(vars, 1)}, {Tag(vars, 2)}, {Tag(vars, 3)}");
            else if (vars.Length / 2 == 5)
                Debug.LogError($"@@ '{context.name}.{reason1}()': {reason2} => {Tag(vars, 0)}, {Tag(vars, 1)}, {Tag(vars, 2)}, {Tag(vars, 3)}, {Tag(vars, 4)}");
            else if (vars.Length / 2 == 6)
                Debug.LogError($"@@ '{context.name}.{reason1}()': {reason2} => {Tag(vars, 0)}, {Tag(vars, 1)}, {Tag(vars, 2)}, {Tag(vars, 3)}, {Tag(vars, 4)}, {Tag(vars, 5)}");
            else if (vars.Length / 2 == 7)
                Debug.LogError($"@@ '{context.name}.{reason1}()': {reason2} => {Tag(vars, 0)}, {Tag(vars, 1)}, {Tag(vars, 2)}, {Tag(vars, 3)}, {Tag(vars, 4)}, {Tag(vars, 5)}, {Tag(vars, 6)}");
            else if (vars.Length / 2 == 8)
                Debug.LogError($"@@ '{context.name}.{reason1}()': {reason2} => {Tag(vars, 0)}, {Tag(vars, 1)}, {Tag(vars, 2)}, {Tag(vars, 3)}, {Tag(vars, 4)}, {Tag(vars, 5)}, {Tag(vars, 6)}, {Tag(vars, 7)}");
            else if (vars.Length / 2 == 9)
                Debug.LogError($"@@ '{context.name}.{reason1}()': {reason2} => {Tag(vars, 0)}, {Tag(vars, 1)}, {Tag(vars, 2)}, {Tag(vars, 3)}, {Tag(vars, 4)}, {Tag(vars, 5)}, {Tag(vars, 6)}, {Tag(vars, 7)}, {Tag(vars, 8)}");
            else
                Debug.Assert(false);
        }
        
    }

}
