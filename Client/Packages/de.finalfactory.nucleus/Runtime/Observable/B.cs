// // ***********************************************************************
// // Author           : Florian Schmidt
// // Created          : 01.06.2020 : 14:38
// // Website          : www.finalfactory.de
// //
// // Last Modified By : Florian Schmidt
// // Last Modified On : 01.06.2020 : 14:38
// // ***********************************************************************
// // <copyright file="B.cs" company="Final Factory">
// //     Copyright (c) Final Factory. All rights reserved.
// // </copyright>
// // <summary></summary>
// // ***********************************************************************

using System;

namespace FinalFactory.Observable
{
    public class B<T> : PropertyBase<T>
    {
        protected Func<T> Getter;
        protected Action<T> Setter;
        
        public override T Value
        {
            get => Getter();
            set
            {
                var oldValue = Getter();
                if (SkipEqualsCheck || !Equals(oldValue, value))
                {
                    Setter(value);
                    OnChanged(oldValue, value);
                }
            }
        }
        
        public B(Func<T> getter, Action<T> setter)
        {
            Getter = getter;
            Setter = setter;
        }

        public override T GetValue() => Getter();
        
        public bool SkipEqualsCheck { get; set; }
        
        public B<T> WithSkipEqualsCheck(bool skipEqualsCheck)
        {
            SkipEqualsCheck = skipEqualsCheck;
            return this;
        }
    }
    
    public class B : B<object>
    {
        public B(Func<object> getter, Action<object> setter, Type valueType) : base(getter, setter)
        {
            ValueType = valueType;
        }
        
        public B(Func<object> getter, Action<object> setter) : this(getter, setter, typeof(object))
        {
        }

        public override Type ValueType { get; }
    }
}