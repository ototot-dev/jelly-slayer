
//###########################################
//This file is auto-generated. Do not modify!
//###########################################
// Because of the auto generation there are some redundant type casts.
// ReSharper disable ArrangeObjectCreationWhenTypeEvident
// ReSharper disable RedundantCast
// ReSharper disable RedundantUsingDirective
// ReSharper disable InconsistentNaming
using JetBrains.Annotations;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using FinalFactory.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;
namespace FinalFactory.UIElements
{
    // ReSharper disable once InconsistentNaming
    public static partial class CustomStylePropertyExtensions 
    {
        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool TryGetValue(this CustomStyleProperty<float> property, CustomStyleResolvedEvent evt,
            ref bool dirty,
            out float value) => TryGetValue(property, evt.customStyle, ref dirty, out value);

        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool TryGetValue(this CustomStyleProperty<float> property, ICustomStyle style, ref bool dirty,
            out float value)
        {
            value = default;
            bool hasValue = style.TryGetValue(property, out value);
            dirty |= hasValue;
            return hasValue;
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool TryGetValue(this CustomStyleProperty<float> property, CustomStyleResolvedEvent evt,
            out float value) => TryGetValue(property, evt.customStyle, out value);

        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool TryGetValue(this CustomStyleProperty<float> property, ICustomStyle style, out float value)
        {
            return style.TryGetValue(property, out value);
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static void UpdateValue(this CustomStyleProperty<float> property, CustomStyleResolvedEvent evt, ref bool dirty, ref float value)
        {
            dirty |= UpdateValue(property, evt.customStyle, ref value);
        }
        
        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool UpdateValue(this CustomStyleProperty<float> property, CustomStyleResolvedEvent evt, ref float value)
        {
            return UpdateValue(property, evt.customStyle, ref value);
        }
        
        [MethodImpl(GlobalConst.ImplOptions)]
        public static void UpdateValue(this CustomStyleProperty<float> property, ICustomStyle style, ref bool dirty, ref float value)
        {
            dirty |= UpdateValue(property, style, ref value);
        }
        
        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool UpdateValue(this CustomStyleProperty<float> property, ICustomStyle style, ref float value)
        {
            if (style.TryGetValue(property, out var v) && Math.Abs(value - v) > float.Epsilon)
            {
                value = v;
                return true;
            }
            return false;
        }
        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool TryGetValue(this CustomStyleProperty<int> property, CustomStyleResolvedEvent evt,
            ref bool dirty,
            out int value) => TryGetValue(property, evt.customStyle, ref dirty, out value);

        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool TryGetValue(this CustomStyleProperty<int> property, ICustomStyle style, ref bool dirty,
            out int value)
        {
            value = default;
            bool hasValue = style.TryGetValue(property, out value);
            dirty |= hasValue;
            return hasValue;
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool TryGetValue(this CustomStyleProperty<int> property, CustomStyleResolvedEvent evt,
            out int value) => TryGetValue(property, evt.customStyle, out value);

        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool TryGetValue(this CustomStyleProperty<int> property, ICustomStyle style, out int value)
        {
            return style.TryGetValue(property, out value);
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static void UpdateValue(this CustomStyleProperty<int> property, CustomStyleResolvedEvent evt, ref bool dirty, ref int value)
        {
            dirty |= UpdateValue(property, evt.customStyle, ref value);
        }
        
        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool UpdateValue(this CustomStyleProperty<int> property, CustomStyleResolvedEvent evt, ref int value)
        {
            return UpdateValue(property, evt.customStyle, ref value);
        }
        
        [MethodImpl(GlobalConst.ImplOptions)]
        public static void UpdateValue(this CustomStyleProperty<int> property, ICustomStyle style, ref bool dirty, ref int value)
        {
            dirty |= UpdateValue(property, style, ref value);
        }
        
        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool UpdateValue(this CustomStyleProperty<int> property, ICustomStyle style, ref int value)
        {
            if (style.TryGetValue(property, out var v) && value != v)
            {
                value = v;
                return true;
            }
            return false;
        }
        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool TryGetValue(this CustomStyleProperty<bool> property, CustomStyleResolvedEvent evt,
            ref bool dirty,
            out bool value) => TryGetValue(property, evt.customStyle, ref dirty, out value);

        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool TryGetValue(this CustomStyleProperty<bool> property, ICustomStyle style, ref bool dirty,
            out bool value)
        {
            value = default;
            bool hasValue = style.TryGetValue(property, out value);
            dirty |= hasValue;
            return hasValue;
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool TryGetValue(this CustomStyleProperty<bool> property, CustomStyleResolvedEvent evt,
            out bool value) => TryGetValue(property, evt.customStyle, out value);

        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool TryGetValue(this CustomStyleProperty<bool> property, ICustomStyle style, out bool value)
        {
            return style.TryGetValue(property, out value);
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static void UpdateValue(this CustomStyleProperty<bool> property, CustomStyleResolvedEvent evt, ref bool dirty, ref bool value)
        {
            dirty |= UpdateValue(property, evt.customStyle, ref value);
        }
        
        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool UpdateValue(this CustomStyleProperty<bool> property, CustomStyleResolvedEvent evt, ref bool value)
        {
            return UpdateValue(property, evt.customStyle, ref value);
        }
        
        [MethodImpl(GlobalConst.ImplOptions)]
        public static void UpdateValue(this CustomStyleProperty<bool> property, ICustomStyle style, ref bool dirty, ref bool value)
        {
            dirty |= UpdateValue(property, style, ref value);
        }
        
        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool UpdateValue(this CustomStyleProperty<bool> property, ICustomStyle style, ref bool value)
        {
            if (style.TryGetValue(property, out var v) && value != v)
            {
                value = v;
                return true;
            }
            return false;
        }
        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool TryGetValue(this CustomStyleProperty<Color> property, CustomStyleResolvedEvent evt,
            ref bool dirty,
            out Color value) => TryGetValue(property, evt.customStyle, ref dirty, out value);

        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool TryGetValue(this CustomStyleProperty<Color> property, ICustomStyle style, ref bool dirty,
            out Color value)
        {
            value = default;
            bool hasValue = style.TryGetValue(property, out value);
            dirty |= hasValue;
            return hasValue;
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool TryGetValue(this CustomStyleProperty<Color> property, CustomStyleResolvedEvent evt,
            out Color value) => TryGetValue(property, evt.customStyle, out value);

        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool TryGetValue(this CustomStyleProperty<Color> property, ICustomStyle style, out Color value)
        {
            return style.TryGetValue(property, out value);
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static void UpdateValue(this CustomStyleProperty<Color> property, CustomStyleResolvedEvent evt, ref bool dirty, ref Color value)
        {
            dirty |= UpdateValue(property, evt.customStyle, ref value);
        }
        
        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool UpdateValue(this CustomStyleProperty<Color> property, CustomStyleResolvedEvent evt, ref Color value)
        {
            return UpdateValue(property, evt.customStyle, ref value);
        }
        
        [MethodImpl(GlobalConst.ImplOptions)]
        public static void UpdateValue(this CustomStyleProperty<Color> property, ICustomStyle style, ref bool dirty, ref Color value)
        {
            dirty |= UpdateValue(property, style, ref value);
        }
        
        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool UpdateValue(this CustomStyleProperty<Color> property, ICustomStyle style, ref Color value)
        {
            if (style.TryGetValue(property, out var v) && value != v)
            {
                value = v;
                return true;
            }
            return false;
        }
        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool TryGetValue(this CustomStyleProperty<Texture2D> property, CustomStyleResolvedEvent evt,
            ref bool dirty,
            out Texture2D value) => TryGetValue(property, evt.customStyle, ref dirty, out value);

        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool TryGetValue(this CustomStyleProperty<Texture2D> property, ICustomStyle style, ref bool dirty,
            out Texture2D value)
        {
            value = default;
            bool hasValue = style.TryGetValue(property, out value);
            dirty |= hasValue;
            return hasValue;
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool TryGetValue(this CustomStyleProperty<Texture2D> property, CustomStyleResolvedEvent evt,
            out Texture2D value) => TryGetValue(property, evt.customStyle, out value);

        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool TryGetValue(this CustomStyleProperty<Texture2D> property, ICustomStyle style, out Texture2D value)
        {
            return style.TryGetValue(property, out value);
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static void UpdateValue(this CustomStyleProperty<Texture2D> property, CustomStyleResolvedEvent evt, ref bool dirty, ref Texture2D value)
        {
            dirty |= UpdateValue(property, evt.customStyle, ref value);
        }
        
        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool UpdateValue(this CustomStyleProperty<Texture2D> property, CustomStyleResolvedEvent evt, ref Texture2D value)
        {
            return UpdateValue(property, evt.customStyle, ref value);
        }
        
        [MethodImpl(GlobalConst.ImplOptions)]
        public static void UpdateValue(this CustomStyleProperty<Texture2D> property, ICustomStyle style, ref bool dirty, ref Texture2D value)
        {
            dirty |= UpdateValue(property, style, ref value);
        }
        
        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool UpdateValue(this CustomStyleProperty<Texture2D> property, ICustomStyle style, ref Texture2D value)
        {
            if (style.TryGetValue(property, out var v) && value != v)
            {
                value = v;
                return true;
            }
            return false;
        }
        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool TryGetValue(this CustomStyleProperty<Sprite> property, CustomStyleResolvedEvent evt,
            ref bool dirty,
            out Sprite value) => TryGetValue(property, evt.customStyle, ref dirty, out value);

        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool TryGetValue(this CustomStyleProperty<Sprite> property, ICustomStyle style, ref bool dirty,
            out Sprite value)
        {
            value = default;
            bool hasValue = style.TryGetValue(property, out value);
            dirty |= hasValue;
            return hasValue;
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool TryGetValue(this CustomStyleProperty<Sprite> property, CustomStyleResolvedEvent evt,
            out Sprite value) => TryGetValue(property, evt.customStyle, out value);

        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool TryGetValue(this CustomStyleProperty<Sprite> property, ICustomStyle style, out Sprite value)
        {
            return style.TryGetValue(property, out value);
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static void UpdateValue(this CustomStyleProperty<Sprite> property, CustomStyleResolvedEvent evt, ref bool dirty, ref Sprite value)
        {
            dirty |= UpdateValue(property, evt.customStyle, ref value);
        }
        
        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool UpdateValue(this CustomStyleProperty<Sprite> property, CustomStyleResolvedEvent evt, ref Sprite value)
        {
            return UpdateValue(property, evt.customStyle, ref value);
        }
        
        [MethodImpl(GlobalConst.ImplOptions)]
        public static void UpdateValue(this CustomStyleProperty<Sprite> property, ICustomStyle style, ref bool dirty, ref Sprite value)
        {
            dirty |= UpdateValue(property, style, ref value);
        }
        
        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool UpdateValue(this CustomStyleProperty<Sprite> property, ICustomStyle style, ref Sprite value)
        {
            if (style.TryGetValue(property, out var v) && value != v)
            {
                value = v;
                return true;
            }
            return false;
        }
        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool TryGetValue(this CustomStyleProperty<VectorImage> property, CustomStyleResolvedEvent evt,
            ref bool dirty,
            out VectorImage value) => TryGetValue(property, evt.customStyle, ref dirty, out value);

        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool TryGetValue(this CustomStyleProperty<VectorImage> property, ICustomStyle style, ref bool dirty,
            out VectorImage value)
        {
            value = default;
            bool hasValue = style.TryGetValue(property, out value);
            dirty |= hasValue;
            return hasValue;
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool TryGetValue(this CustomStyleProperty<VectorImage> property, CustomStyleResolvedEvent evt,
            out VectorImage value) => TryGetValue(property, evt.customStyle, out value);

        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool TryGetValue(this CustomStyleProperty<VectorImage> property, ICustomStyle style, out VectorImage value)
        {
            return style.TryGetValue(property, out value);
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static void UpdateValue(this CustomStyleProperty<VectorImage> property, CustomStyleResolvedEvent evt, ref bool dirty, ref VectorImage value)
        {
            dirty |= UpdateValue(property, evt.customStyle, ref value);
        }
        
        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool UpdateValue(this CustomStyleProperty<VectorImage> property, CustomStyleResolvedEvent evt, ref VectorImage value)
        {
            return UpdateValue(property, evt.customStyle, ref value);
        }
        
        [MethodImpl(GlobalConst.ImplOptions)]
        public static void UpdateValue(this CustomStyleProperty<VectorImage> property, ICustomStyle style, ref bool dirty, ref VectorImage value)
        {
            dirty |= UpdateValue(property, style, ref value);
        }
        
        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool UpdateValue(this CustomStyleProperty<VectorImage> property, ICustomStyle style, ref VectorImage value)
        {
            if (style.TryGetValue(property, out var v) && value != v)
            {
                value = v;
                return true;
            }
            return false;
        }
        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool TryGetValue(this CustomStyleProperty<string> property, CustomStyleResolvedEvent evt,
            ref bool dirty,
            out string value) => TryGetValue(property, evt.customStyle, ref dirty, out value);

        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool TryGetValue(this CustomStyleProperty<string> property, ICustomStyle style, ref bool dirty,
            out string value)
        {
            value = default;
            bool hasValue = style.TryGetValue(property, out value);
            dirty |= hasValue;
            return hasValue;
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool TryGetValue(this CustomStyleProperty<string> property, CustomStyleResolvedEvent evt,
            out string value) => TryGetValue(property, evt.customStyle, out value);

        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool TryGetValue(this CustomStyleProperty<string> property, ICustomStyle style, out string value)
        {
            return style.TryGetValue(property, out value);
        }

        [MethodImpl(GlobalConst.ImplOptions)]
        public static void UpdateValue(this CustomStyleProperty<string> property, CustomStyleResolvedEvent evt, ref bool dirty, ref string value)
        {
            dirty |= UpdateValue(property, evt.customStyle, ref value);
        }
        
        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool UpdateValue(this CustomStyleProperty<string> property, CustomStyleResolvedEvent evt, ref string value)
        {
            return UpdateValue(property, evt.customStyle, ref value);
        }
        
        [MethodImpl(GlobalConst.ImplOptions)]
        public static void UpdateValue(this CustomStyleProperty<string> property, ICustomStyle style, ref bool dirty, ref string value)
        {
            dirty |= UpdateValue(property, style, ref value);
        }
        
        [MethodImpl(GlobalConst.ImplOptions)]
        public static bool UpdateValue(this CustomStyleProperty<string> property, ICustomStyle style, ref string value)
        {
            if (style.TryGetValue(property, out var v) && value != v)
            {
                value = v;
                return true;
            }
            return false;
        }
    }
}