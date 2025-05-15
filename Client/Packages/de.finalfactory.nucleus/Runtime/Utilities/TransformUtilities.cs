// --------------------------------------------------------------------------------------------------------------------
//   © 2024  Final Factory Florian Schmidt.
//   TransformUtilities.cs is part of FinalFactory.Nucleus, distributed as part of this project.
//   Usage or distribution of this file is subject to the terms outlined in the LICENSE file located in the root of the project.
//   For specific licensing terms, refer to the LICENSE file.
// --------------------------------------------------------------------------------------------------------------------

using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace FinalFactory.Utilities
{
    [DebuggerStepThrough]
    public static class TransformUtilities
    {
        /// <summary>
        /// Returns the forward vector of a rotation.
        /// Equivalent to Transform.forward.
        /// </summary>
        /// <param name="rotation"></param>
        /// <returns></returns>
        [MethodImpl(GlobalConst.ImplOptions)]
        public static Vector3 Forward(this Quaternion rotation) => rotation * Vector3.forward;

        /// <summary>
        /// Returns the right vector of a rotation.
        /// Equivalent to Transform.right.
        /// </summary>
        /// <param name="rotation"></param>
        /// <returns></returns>
        [MethodImpl(GlobalConst.ImplOptions)]
        public static Vector3 Right(this Quaternion rotation) => rotation * Vector3.right;

        /// <summary>
        /// Returns the up vector of a rotation.
        /// Equivalent to Transform.up.
        /// </summary>
        /// <param name="rotation"></param>
        /// <returns></returns>
        [MethodImpl(GlobalConst.ImplOptions)]
        public static Vector3 Up(this Quaternion rotation) => rotation * Vector3.up;
    }
}