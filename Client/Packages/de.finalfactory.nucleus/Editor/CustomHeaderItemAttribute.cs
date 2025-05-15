using System;

namespace FinalFactory.Editor
{
    /// <summary>
    /// This attribute enables the ability to draw custom header items in the inspector.
    /// <example>
    /// <code>
    /// [ComponentHeaderItem]
    /// private static void DrawHeaderItem(UnityEngine.Object target, UnityEngine.Rect rect)
    /// {
    ///     rect.x -= 110;
    ///     rect.width = 110;
    ///     GUI.Label(rect, new GUIContent("Hello World"));
    /// }
    /// </code>
    /// </example>
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class CustomHeaderItemAttribute : Attribute { }
}
