using System;

namespace Project.Combat
{
    /// <summary>
    /// Static channel for short player-facing feedback messages (e.g. "why
    /// did nothing happen") raised by any gameplay system whose action
    /// failed with no other visible effect. Decouples the systems that
    /// detect a failure from whatever UI displays it: <c>Project.UI</c>
    /// subscribes to <see cref="MessagePublished"/> without gameplay
    /// assemblies (which sit lower in the dependency graph) needing a
    /// reference back into it.
    /// </summary>
    public static class PlayerFeedbackChannel
    {
        /// <summary>Raised whenever a system publishes a feedback message for display.</summary>
        public static event Action<string> MessagePublished;

        /// <summary>
        /// Publishes a feedback message. Safe to call with no subscriber
        /// attached yet (e.g. before the UI has initialized).
        /// </summary>
        /// <param name="message">The message text to show the player.</param>
        public static void Publish(string message)
        {
            MessagePublished?.Invoke(message);
        }
    }
}
