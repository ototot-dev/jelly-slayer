using UnityEngine.UIElements;

namespace FinalFactory.UIElements
{
    public class AdvancedVisualElement : VisualElement
    {
        public AdvancedVisualElement()
        {
            RegisterCallback<AttachToPanelEvent>(OnAttachedToPanel);
            RegisterCallback<DetachFromPanelEvent>(OnDetachedFromPanel);
        }

        public bool IsActiveAndEnabled => visible && enabledInHierarchy && IsAttachedToPanel;

        public bool IsRuntime
        {
            get
            {
#if UNITY_EDITOR
                return UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode;
#else
                return true;
#endif
            }
        }

        public bool IsAttachedToPanel => panel != null;
        
        protected virtual void OnAttachedToPanel(AttachToPanelEvent evt)
        {
            
        }

        protected virtual void OnDetachedFromPanel(DetachFromPanelEvent evt)
        {
            
        }
    }
}