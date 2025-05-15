using System;
using FinalFactory.Observable.Handlers;

namespace FinalFactory.Observable
{
    public interface IReadOnlyProperty
    {
        Type ValueType { get; }
        object Value { get;  }
        event EventHandler<object> Changed;
        event EventHandlerChange<object> ValueChanged;
        void AddEventHandler(EventHandler<object> action, bool call = true);
        void AddEventHandler(EventHandlerChange<object> action, bool call = true);
        
        void RemoveEventHandler(EventHandler<object> action);
        void RemoveEventHandler(EventHandlerChange<object> action);
    }

    public interface IReadOnlyProperty<T> : IReadOnlyProperty
    {
        new T Value { get;  }
        new event EventHandler<T> Changed;
        new event EventHandlerChange<T> ValueChanged;
        void AddEventHandler(EventHandler<T> action, bool call = true);
        void AddEventHandler(EventHandlerChange<T> action, bool call = true);
        void RemoveEventHandler(EventHandler<T> action);
        void RemoveEventHandler(EventHandlerChange<T> action);
    }
}