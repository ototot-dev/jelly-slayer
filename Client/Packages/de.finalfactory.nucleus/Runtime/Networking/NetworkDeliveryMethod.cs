namespace FinalFactory.Networking
{
    public enum NetworkDeliveryMethod : byte
    {
        /// <summary>
        /// Unreliable, unordered delivery
        /// </summary>
        Unreliable = 4,

        /// <summary>
        /// Unreliable delivery, but automatically dropping late messages
        /// </summary>
        UnreliableSequenced = 1,

        /// <summary>
        /// Reliable delivery, but unordered
        /// </summary>
        ReliableUnordered = 0,

        /// <summary>
        /// Reliable delivery, except for late messages which are dropped
        /// </summary>
        ReliableSequenced = 3,

        /// <summary>
        /// Reliable, ordered delivery
        /// </summary>
        ReliableOrdered = 2,
    }
}
