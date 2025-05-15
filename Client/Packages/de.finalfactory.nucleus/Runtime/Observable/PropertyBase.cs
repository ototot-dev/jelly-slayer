// // ***********************************************************************
// // Author           : Florian Schmidt
// // Created          : 24.11.2020 : 17:30
// // Website          : www.finalfactory.de
// //
// // Last Modified By : Florian Schmidt
// // Last Modified On : 24.11.2020 : 17:30
// // ***********************************************************************
// // <copyright file="PropertyBase.cs" company="Final Factory">
// //     Copyright (c) Final Factory. All rights reserved.
// // </copyright>
// // <summary></summary>
// // ***********************************************************************

using System;
using FinalFactory.Observable.Handlers;

namespace FinalFactory.Observable
{
    public abstract class PropertyBase<T> : ObservableBase<T>, IProperty<T>
    {
        private event EventHandler<object> ObjectChanged; 
        private event EventHandlerChange<object> ObjectValueChanged; 

        public virtual Type ValueType => typeof(T);

        object IReadOnlyProperty.Value => Value;
        public abstract T Value { get; set; }
        
        object IProperty.Value
        {
            get => Value;
            set => Value = (T) value;
        }
        
        event EventHandler<object> IReadOnlyProperty.Changed
        {
            add => ObjectChanged += value;
            remove => ObjectChanged -= value;
        }

        event EventHandlerChange<object> IReadOnlyProperty.ValueChanged
        {
            add => ObjectValueChanged += value;
            remove => ObjectValueChanged -= value;
        }
        
        void IReadOnlyProperty.AddEventHandler(EventHandler<object> action, bool call)
        {
            ObjectChanged += action;
            if (call)
            {
                action(this, GetValue());
            }
        }
        
        void IReadOnlyProperty.AddEventHandler(EventHandlerChange<object> action, bool call)
        {
            ObjectValueChanged += action;
            if (call)
            {
                action(this, default, GetValue());
            }
        }
        
        void IReadOnlyProperty.RemoveEventHandler(EventHandler<object> action) => ObjectChanged -= action;
        void IReadOnlyProperty.RemoveEventHandler(EventHandlerChange<object> action) => ObjectValueChanged -= action;
        
        protected override void OnChanged(T oldValue, T newValue)
        {
            base.OnChanged(oldValue, newValue);
            ObjectChanged?.Invoke(this, newValue);
            ObjectValueChanged?.Invoke(this, oldValue, newValue);
        }
    }
}