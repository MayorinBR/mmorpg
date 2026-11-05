using System.Diagnostics;

namespace Project.Combat
{
    /// <summary>
    /// Writes combat and skill diagnostics to the Unity console so skills and status effects can be tested
    /// without reading the code. Calls are removed from release builds.
    /// </summary>
    public static class CombatLog
    {
        private const string Prefix = "[Combat] ";

        /// <summary>
        /// Writes a message to the console in the editor and in development builds only.
        /// </summary>
        /// <param name="message">The message to write.</param>
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void Write(string message)
        {
            UnityEngine.Debug.Log(Prefix + message);
        }
    }
}
