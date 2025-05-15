using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using FinalFactory.Tooling;
#if UNITY_MATHEMATICS
using Unity.Mathematics;
#endif
using UnityEngine;
using UnityEngine.UIElements;

namespace FinalFactory.UIElements
{
    public static class VisualElementExtensions
    {
        private static Type UIElementsType = typeof(Clickable);
        private static readonly Type PseudoStateType = UIElementsType.Assembly.GetType("UnityEngine.UIElements.PseudoStates");
        
        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool IsFocused(this VisualElement element)
        {
            if (element.panel == null)
            {
                return false;
            }
            return element.panel.focusController.focusedElement == element;
        }
        
        [MethodImpl(GlobalConst.ImplOptions)]
        public static void SetSize(this VisualElement element, Vector2 size)
        {
            element.style.width = size.x;
            element.style.height = size.y;
        }
#if UNITY_MATHEMATICS
        [MethodImpl(GlobalConst.ImplOptions)]
        public static void SetSize(this VisualElement element, float2 size)
        {
            element.style.width = size.x;
            element.style.height = size.y;
        }
#endif

        [MethodImpl(GlobalConst.ImplOptions)]
        public static Vector2 GetSize(this VisualElement element)
        {
            return element.layout.size;
        }

        /// <summary>
        /// Returns the position of the element relative to the parent with all applied styles.
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        [MethodImpl(GlobalConst.ImplOptions)]
        public static Vector2 GetPositionResolved(this VisualElement element)
        {
            return new Vector2(element.resolvedStyle.left, element.resolvedStyle.top);
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static Vector2 GetPosition(this VisualElement element)
        {
            return new Vector2(element.style.left.value.value, element.style.top.value.value);
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static void SetPosition(this VisualElement element, float x, float y)
        {
            element.style.left = x;
            element.style.top = y;
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static void SetPosition(this VisualElement element, Vector2 position)
        {
            element.style.left = position.x;
            element.style.top = position.y;
        }
#if UNITY_MATHEMATICS
        [MethodImpl(GlobalConst.ImplOptions)]
        public static void SetPosition(this VisualElement element, float2 position)
        {
            element.style.left = position.x;
            element.style.top = position.y;
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static void SetAnchoredPosition(this VisualElement element, float2 position)
        {
            element.style.left = new StyleLength(new Length(position.x, LengthUnit.Percent));
            element.style.top = new StyleLength(new Length(position.y, LengthUnit.Percent));
        }
#endif

        [MethodImpl(GlobalConst.ImplOptions)]
        public static void SetActive(this VisualElement element, bool isActive)
        {
            element.style.display = isActive ? DisplayStyle.Flex : DisplayStyle.None;
        }
        
        [MethodImpl(GlobalConst.ImplOptions)]
        public static void SetVisibility(this VisualElement element, bool visible) => element.visible = visible;
        
        [MethodImpl(GlobalConst.ImplOptions)]
        public static IEnumerable<VisualElement> ChildrenRecursive(this VisualElement element)
        {
            foreach (var child in element.Children())
            {
                yield return child;
                foreach (var grandChild in child.ChildrenRecursive())
                {
                    yield return grandChild;
                }
            }
        }
        
        [MethodImpl(GlobalConst.ImplOptions)]
        public static IEnumerable<VisualElement> ChildrenRecursive(this VisualElement element, int minDepth, int maxDepth = 999)
        {
            foreach (var child in element.Children())
            {
                if (minDepth <= 0)
                {
                    yield return child;
                }
                if (maxDepth > 0)
                {
                    foreach (var grandChild in child.ChildrenRecursive(minDepth - 1, maxDepth - 1))
                    {
                        yield return grandChild;
                    }
                }
            }
        }
        
        #region PsuedoStates
        
        /* PsuedoStates... why would we ever want to create our own custom elements?
         * Why would we ever want to expand the UIElements framework??
         * WHY WOULD WE EVER WANT TO CREATE OUR OWN CUSTOM UIElements????
         * Sorry but this is just a rant. I'm just so frustrated with Unitys keep all internal behaviour approach.
         * See the little helper methods... would be that so hard to just open up the API?
         * I mean, internal functions could be "dangerous" if you dont know what you are doing, that's true.
         * But why not just open up the API and mark them as "internal" and not protected them via internal?
         * That way we could just use them and if we break something, it's our fault.
         * Does anyone really read this?
         */
        
        [MethodImpl(GlobalConst.ImplOptions)]
        public static void AddPseudoState(this VisualElement element, PseudoStates state)
        {
            element.SetPseudoState(element.GetPseudoState() | state);
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static void RemovePseudoState(this VisualElement element, PseudoStates state)
        {
            element.SetPseudoState(element.GetPseudoState() & ~state);
        }
        
        [MethodImpl(GlobalConst.ImplOptions)]
        public static void TogglePseudoState(this VisualElement element, PseudoStates state, bool toggle)
        {
            if (toggle)
            {
                element.AddPseudoState(state);
            }
            else
            {
                element.RemovePseudoState(state);
            }
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static void SetPseudoState(this VisualElement element, PseudoStates state)
        {
            element.SetValue("pseudoStates", Enum.ToObject(PseudoStateType, (int) state));
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static PseudoStates GetPseudoState(this VisualElement element)
        {
            return (PseudoStates) (int) element.GetValue("pseudoStates");
        }
        
        #endregion
    }
}