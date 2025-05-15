namespace FinalFactory.Logging
{
    public interface ILogReceiver
    {
        void Push(LogMessage message);
    }
}