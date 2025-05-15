using UnityEngine.UIElements;

namespace FinalFactory.UIElements
{
    public class ComplexElement : VisualElement
    {
        protected const int LayoutUpdateCodeFull = 0;
        
        public bool IsLayoutDirty { get; private set; }
        public bool IsLayoutSuspended { get; private set; }
        public bool IsAttached { get; private set; }

        protected bool PerformDetachedLayoutUpdates { get; set; } = true;
        
        public ComplexElement()
        {
            RegisterCallback<AttachToPanelEvent>(OnAttachedToPanel);
            RegisterCallback<DetachFromPanelEvent>(OnDetachedFromPanel);
        }

        /// <summary>
        /// Suspend layout updates.
        /// </summary>
        public void SuspendLayout()
        {
            IsLayoutSuspended = true;
        }

        /// <summary>
        /// Resume layout update. Performs layout update if layout is dirty.
        /// </summary>
        public void ResumeLayout()
        {
            IsLayoutSuspended = false;
            if (IsLayoutDirty)
            {
                PerformLayout();
            }
        }
        
        /// <summary>
        /// Perform layout update now, even layout is suspended.
        /// Does not perform updates if this element is not attached to an panel until <see cref="PerformDetachedLayoutUpdates"/> is set to true.
        /// </summary>
        public void PerformLayout()
        {
            if (!IsAttached && !PerformDetachedLayoutUpdates)
            {
                IsLayoutDirty = true;
                return;
            }
            OnUpdateLayout(LayoutUpdateCodeFull);
        }

        protected virtual void OnAttachedToPanel(AttachToPanelEvent evt)
        {
            IsAttached = true;
            if (!PerformDetachedLayoutUpdates && IsLayoutDirty)
            {
                PerformLayout();
            }
        }
        protected virtual void OnDetachedFromPanel(DetachFromPanelEvent evt)
        {
            IsAttached = false;
        }
        
        protected void UpdateLayout(int updateCode)
        {
            if (IsLayoutSuspended)
            {
                IsLayoutDirty = true;
                return;
            }
            if (!IsAttached && !PerformDetachedLayoutUpdates)
            {
                IsLayoutDirty = true;
                return;
            }
            OnUpdateLayout(updateCode);
        }
        
        protected virtual void OnUpdateLayout(int updateCode)
        {
            IsLayoutDirty = false;
        }
    }
}