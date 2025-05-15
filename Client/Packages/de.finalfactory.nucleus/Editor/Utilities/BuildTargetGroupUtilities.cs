using System;
using System.Collections.Generic;
using System.Linq;
using FinalFactory.Helpers;
using FinalFactory.Tooling;
using FinalFactory.Utilities;
using JetBrains.Annotations;
using UnityEditor;

namespace FinalFactory.Editor.Utilities
{
    /// <summary>
    /// Utility class for managing build target groups in Unity.
    /// </summary>
    [PublicAPI]
    public static class BuildTargetGroupUtilities
    {
        /// <summary>
        /// Gets all valid build target groups.
        /// </summary>
        public static IEnumerable<BuildTargetGroup> AllValidBuildGroups => EnumHelpers.GetValues<BuildTargetGroup>().Where(IsValid);

        /// <summary>
        /// Checks if a build target group is valid.
        /// </summary>
        /// <param name="group">The build target group to check.</param>
        /// <returns>True if the build target group is valid, false otherwise.</returns>
        public static bool IsValid(this BuildTargetGroup group)
        {
            return group != BuildTargetGroup.Unknown &&
                   !typeof(BuildTargetGroup).GetField(group.ToString()).HasAttribute<ObsoleteAttribute>();
        }
    }
}