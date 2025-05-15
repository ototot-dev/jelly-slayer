using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEditor;

namespace FinalFactory.Editor.Utilities
{
    /// <summary>
    /// Utility class for managing scripting define symbols in Unity.
    /// </summary>
    [PublicAPI]
    public static class DefineUtility
    {
        /// <summary>
        /// Gets all valid build target groups.
        /// </summary>
        private static IEnumerable<BuildTargetGroup> AllGroups => BuildTargetGroupUtilities.AllValidBuildGroups;

        /// <summary>
        /// Checks if a define exists in any build target group.
        /// </summary>
        public static bool HasDefine(string define) => AllGroups.Any(group => HasDefine(define, group));

        /// <summary>
        /// Checks if a define exists in a specific build target group.
        /// </summary>
        public static bool HasDefine(string define, BuildTargetGroup group) => GetDefines(group).Contains(define);

        /// <summary>
        /// Toggles a define in all build target groups.
        /// </summary>
        public static bool ToggleDefine(string define, bool enable) => enable ? AddDefine(define) : RemoveDefine(define);

        /// <summary>
        /// Toggles a define in a specific build target group.
        /// </summary>
        public static bool ToggleDefine(string define, BuildTargetGroup group, bool enable) => enable ? AddDefine(define, group) : RemoveDefine(define, group);

        /// <summary>
        /// Adds a define to all build target groups.
        /// </summary>
        public static bool AddDefine(string define) => AllGroups.Aggregate(false, (current, group) => current | AddDefine(define, group));

        /// <summary>
        /// Adds a define to a specific build target group.
        /// </summary>
        public static bool AddDefine(string define, BuildTargetGroup group)
        {
            var defines = GetDefines(group);

            if (defines.Add(define))
            {
                SetDefines(group, defines);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Removes a define from all build target groups.
        /// </summary>
        public static bool RemoveDefine(string define) => AllGroups.Aggregate(false, (current, group) => current | RemoveDefine(define, group));

        /// <summary>
        /// Removes a define from a specific build target group.
        /// </summary>
        public static bool RemoveDefine(string define, BuildTargetGroup group)
        {
            var defines = GetDefines(group);

            if (defines.Remove(define))
            {
                SetDefines(group, defines);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Sets the defines for a specific build target group.
        /// </summary>
        private static void SetDefines(BuildTargetGroup group, HashSet<string> defines)
        {
            PlayerSettings.SetScriptingDefineSymbolsForGroup(group, string.Join(";", defines.ToArray()));
        }

        /// <summary>
        /// Gets the defines for a specific build target group.
        /// </summary>
        private static HashSet<string> GetDefines(BuildTargetGroup group)
        {
            return PlayerSettings.GetScriptingDefineSymbolsForGroup(group).Split(';').Select(d => d.Trim()).ToHashSet();
        }
    }
}