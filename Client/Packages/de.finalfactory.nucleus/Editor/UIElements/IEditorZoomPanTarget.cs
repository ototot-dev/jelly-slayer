using UnityEngine;

namespace FinalFactory.Editor.UIElements
{
    public interface IEditorZoomPanTarget
    {
        float ZoomScale { get; set; }
        Vector2 ContentOffset { get; set; }
    }
}