// --------------------------------------------------------------------------------------------------------------------
//   © 2024  Final Factory Florian Schmidt.
//   CameraUtilities.cs is part of FinalFactory.Nucleus, distributed as part of this project.
//   Usage or distribution of this file is subject to the terms outlined in the LICENSE file located in the root of the project.
//   For specific licensing terms, refer to the LICENSE file.
// --------------------------------------------------------------------------------------------------------------------

using UnityEngine;

namespace FinalFactory.Utilities
{
    public static class CameraUtilities
    {
        /// <summary>
        /// Calculates a ray from the screen point.
        /// </summary>
        /// <param name="screenPoint">The screen point in pixels.</param>
        /// <param name="position">The camera position.</param>
        /// <param name="rotation">The camera rotation.</param>
        /// <param name="fieldOfView">The camera's field of view in degrees.</param>
        /// <param name="aspectRatio">The camera's aspect ratio.</param>
        /// <param name="nearClipPlane">The camera's near clipping plane distance.</param>
        /// <param name="farClipPlane">The camera's far clipping plane distance.</param>
        /// <returns>A ray pointing from the camera's target position through the screen point.</returns>
        public static Ray ScreenPointToRay(Vector2 screenPoint, Vector3 position, Quaternion rotation, float fieldOfView, 
            float aspectRatio, float nearClipPlane, float farClipPlane)
        {
            // Convert screen point to normalized device coordinates (-1 to 1)
            var viewportPoint = new Vector3((screenPoint.x / Screen.width) * 2f - 1f, (screenPoint.y / Screen.height) * 2f - 1f, 1f);

            // Calculate the projection and view matrices
            var projectionMatrix = Matrix4x4.Perspective(fieldOfView, aspectRatio, nearClipPlane, farClipPlane);
            var viewMatrix = Matrix4x4.TRS(position, rotation, Vector3.one).inverse;

            // Compute the inverse of the combined matrix
            var inverseMatrix = (projectionMatrix * viewMatrix).inverse;

            // Transform the viewport point to world space
            var worldPoint = inverseMatrix.MultiplyPoint(viewportPoint);

            // Calculate the ray direction
            var rayDirection = (worldPoint - position).normalized;

            return new Ray(position, rayDirection);
        }
        
        /// <summary>
        /// Calculates a ray from the screen point.
        /// </summary>
        /// <param name="screenPoint">The screen point in pixels.</param>
        /// <param name="position">The camera position.</param>
        /// <param name="rotation">The camera rotation.</param>
        /// <param name="orthographicSize">The camera's orthographic size.</param>
        /// <param name="aspectRatio">The camera's aspect ratio.</param>
        /// <returns></returns>
        public static Ray ScreenPointToRay(Vector2 screenPoint, Vector3 position, Quaternion rotation, float orthographicSize, 
            float aspectRatio)
        {
            // Convert screen point to viewport coordinates (0 to 1)
            var viewportPoint = new Vector3(screenPoint.x / Screen.width, screenPoint.y / Screen.height, 0f);

            // Calculate the offsets from the center based on viewport coordinates
            var offsetX = (viewportPoint.x - 0.5f) * 2f * orthographicSize * aspectRatio;
            var offsetY = (viewportPoint.y - 0.5f) * 2f * orthographicSize;

            // Calculate the ray origin in world space
            var rayOrigin = position + rotation.Right() * offsetX + rotation.Up() * offsetY;

            // The ray direction is the camera's forward vector
            var rayDirection = rotation.Forward();

            return new Ray(rayOrigin, rayDirection);
        }
    }
}