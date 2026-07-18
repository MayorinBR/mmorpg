namespace Project.Character.Stats
{
    /// <summary>
    /// Placeholder character class list, used by <see cref="Project.Items.ItemDefinition"/>
    /// to mark class-restricted equipment. Actual class assignment and
    /// validation don't exist yet — this only prepares the data so items
    /// don't need to be reconfigured once a real class/job system is built.
    /// </summary>
    public enum CharacterClass
    {
        Warrior,
        Archer,
        Mage
    }
}