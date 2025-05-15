namespace FinalFactory.Networking
{
    public interface INetConnection
    {
        int ConnectionID { get; }
        void Disconnect();
    }
}