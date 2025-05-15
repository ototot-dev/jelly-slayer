namespace FinalFactory.Serialization
{
    public enum SendZone
    {
        /// <summary>
        /// Indicates a serialization call rather than a zone.
        /// </summary>
        Serialization,

        /// <summary>
        /// Broadcast to all subscribers, regardless of their current zone.
        /// </summary>
        Broadcast,

        /// <summary>
        /// For updates visible only upon close inspection, such as machine switches.
        /// </summary>
        Proximity,

        /// <summary>
        /// For high-frequency updates, such as dynamic world UI elements.
        /// </summary>
        DynamicDetail,

        /// <summary>
        /// When a player is directly interacting with an entity, for instance, operating machinery or accessing inventory.
        /// </summary>
        DirectInteraction,

        /// <summary>
        /// When the entity is inside an inventory, signaling updates related to item states.
        /// </summary>
        InventoryState,

        /// <summary>
        /// For logging and testing purposes, not intended for production use.
        /// </summary>
        Diagnostic,
    }
}