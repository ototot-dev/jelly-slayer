using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using FinalFactory.Observable.Handlers;

namespace FinalFactory.Observable
{
    public abstract class ObservableBase<T>
    {
        public event EventHandler<T> Changed;
        public event EventHandlerChange<T> ValueChanged;
        
        public virtual void AddEventHandler(EventHandler<T> action, bool call = true)
        {
            Changed += action;
            if (call)
            {
                action(this, GetValue());
            }
        }
        
        public virtual void AddEventHandler(EventHandlerChange<T> action, bool call = true)
        {
            ValueChanged += action;
            if (call)
            {
                action(this, default, GetValue());
            }
        }

        public virtual void RemoveEventHandler(EventHandler<T> action) => Changed -= action;

        public virtual void RemoveEventHandler(EventHandlerChange<T> action) => ValueChanged -= action;

        public abstract T GetValue();
        
        public void Refire() => OnChanged(GetValue());
        
        [MethodImpl(GlobalConst.ImplOptions)]
        public static implicit operator T(ObservableBase<T> val) => val.GetValue();

        protected virtual void OnChanged(T newValue) => OnChanged(this, default, newValue);
        protected virtual void OnChanged(T oldValue, T newValue) => OnChanged(this, oldValue, newValue);

        protected virtual void OnChanged(object sender, T oldValue, T newValue)
        {
            Changed?.Invoke(sender, newValue);
            ValueChanged?.Invoke(sender, oldValue, newValue);
        }
        
        [MethodImpl(GlobalConst.ImplOptions)]
        public override string ToString() => GetValue()?.ToString() ?? "NULL";
    }
}