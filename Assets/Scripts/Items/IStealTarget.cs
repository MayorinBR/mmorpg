namespace Project.Items
{
    /// <summary>
    /// Something a Steal skill can take an item from, such as an enemy with a drop table.
    /// </summary>
    public interface IStealTarget
    {
        /// <summary>Gets whether this target still has an item to steal. False once it was stolen from, until it respawns.</summary>
        bool CanBeStolenFrom { get; }

        /// <summary>
        /// Takes one item from this target and marks it as stolen from. Only valid while <see cref="CanBeStolenFrom"/> is true.
        /// </summary>
        /// <returns>The stolen item, or null when the target has no item to give.</returns>
        ItemDefinition Steal();
    }
}
