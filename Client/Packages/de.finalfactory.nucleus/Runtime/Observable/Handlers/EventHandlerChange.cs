namespace FinalFactory.Observable.Handlers
{
    public delegate void EventHandlerChange<in TData>(object sender, TData oldValue, TData newValue);

    public delegate void EventHandlerChange<in TOrigin, in TData>(TOrigin sender, TData value);
}