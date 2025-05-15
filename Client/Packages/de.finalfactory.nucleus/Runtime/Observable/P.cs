using System;
using System.Diagnostics;
using FinalFactory.Annotations;
using JetBrains.Annotations;

namespace FinalFactory.Observable
{
    [PublicAPI]
    [Serializable]
    [DebuggerDisplay("{" + nameof(Value) + "}")]
    public class P<T> : PropertyBase<T>
    {
        [SerializeMember]
        private T _value;

        public override T Value
        {
            get => _value;
            set
            {
                var oldValue = _value;
                if (!Equals(oldValue, value))
                {
                    _value = value;
                    OnChanged(oldValue, value);
                }
            }
        }

        public P() { }

        public P(T value) => _value = value;
        public P(T value, EventHandler<T> onChanged)
        {
            _value = value;
            Changed += onChanged;
        }

        public P(EventHandler<T> onChanged) => Changed += onChanged;

        public override T GetValue() => Value;
        
        public P<TNew> Bind<TNew>(Func<T, TNew> c1, Func<TNew, T> c2)
        {
            var property = new P<TNew>(c1(Value));
            Changed += (sender, obj) =>  property.Value = c1(obj);
            property.Changed += (sender, obj)  => Value = c2(obj);
            return property;
        }
    }
}