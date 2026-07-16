namespace Project.AI
{
    /// <summary>
    /// A single behavioral state in the enemy state machine (e.g. idle,
    /// chasing, attacking). Each state is responsible only for its own
    /// entry/exit setup and per-frame logic, keeping <see cref="EnemyController"/>
    /// free of branching behavior code.
    /// </summary>
    public interface IEnemyState
    {
        /// <summary>Called once when the state becomes active.</summary>
        /// <param name="enemy">The enemy this state is controlling.</param>
        void Enter(EnemyController enemy);

        /// <summary>Called every frame while the state is active.</summary>
        /// <param name="enemy">The enemy this state is controlling.</param>
        void Tick(EnemyController enemy);

        /// <summary>Called once when the state is replaced by another.</summary>
        /// <param name="enemy">The enemy this state is controlling.</param>
        void Exit(EnemyController enemy);
    }
}