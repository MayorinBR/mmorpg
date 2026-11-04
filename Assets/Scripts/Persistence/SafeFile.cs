using System;
using System.IO;

namespace Project.Persistence
{
    /// <summary>
    /// Crash-safe text file storage. Writes go to a temporary file first and
    /// then replace the real file, keeping the previous version as a backup,
    /// so an interrupted write can never leave a half-written save behind.
    /// </summary>
    public static class SafeFile
    {
        private const string TemporarySuffix = ".tmp";
        private const string BackupSuffix = ".bak";

        /// <summary>Gets the path of the backup copy kept next to the given file.</summary>
        /// <param name="path">The path of the primary file.</param>
        /// <returns>The backup file path.</returns>
        public static string BackupPath(string path)
        {
            return path + BackupSuffix;
        }

        /// <summary>
        /// Writes the text atomically: the previous content of an existing file is kept as the backup copy.
        /// </summary>
        /// <param name="path">The path of the file to write.</param>
        /// <param name="contents">The text to store.</param>
        public static void WriteAllText(string path, string contents)
        {
            var directory = Path.GetDirectoryName(path);

            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var temporaryPath = path + TemporarySuffix;
            File.WriteAllText(temporaryPath, contents);

            if (File.Exists(path))
            {
                File.Replace(temporaryPath, path, BackupPath(path));
            }
            else
            {
                File.Move(temporaryPath, path);
            }
        }

        /// <summary>
        /// Reads and parses the file, falling back to the backup copy when the
        /// primary file is missing, unreadable or fails to parse.
        /// </summary>
        /// <typeparam name="T">The parsed type.</typeparam>
        /// <param name="path">The path of the primary file.</param>
        /// <param name="parse">Converts the text to a value; returns null or throws when the text is invalid.</param>
        /// <param name="value">The parsed value, or null when neither file could be read.</param>
        /// <returns>True when a value was read from the primary file or the backup.</returns>
        public static bool TryRead<T>(string path, Func<string, T> parse, out T value) where T : class
        {
            return TryReadFile(path, parse, out value) || TryReadFile(BackupPath(path), parse, out value);
        }

        private static bool TryReadFile<T>(string path, Func<string, T> parse, out T value) where T : class
        {
            value = null;

            if (!File.Exists(path))
            {
                return false;
            }

            try
            {
                value = parse(File.ReadAllText(path));
            }
            catch (Exception)
            {
                value = null;
            }

            return value != null;
        }
    }
}
