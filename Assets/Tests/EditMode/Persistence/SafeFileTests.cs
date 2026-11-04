using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace Project.Persistence.Tests
{
    /// <summary>
    /// Tests for <see cref="SafeFile"/> and the save version stamped by <see cref="JsonFileSaveRepository"/>.
    /// </summary>
    public class SafeFileTests
    {
        private string directory;
        private string path;

        [SetUp]
        public void SetUp()
        {
            directory = Path.Combine(Path.GetTempPath(), "SafeFileTests_" + System.Guid.NewGuid().ToString("N"));
            path = Path.Combine(directory, "save.json");
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(directory))
            {
                Directory.Delete(directory, true);
            }
        }

        private static string Parse(string text)
        {
            return text.StartsWith("{") ? text : null;
        }

        [Test]
        public void WriteAllText_CreatesDirectoryAndFile_WithoutLeavingTemporaryFile()
        {
            SafeFile.WriteAllText(path, "{a}");

            Assert.AreEqual("{a}", File.ReadAllText(path));
            Assert.IsFalse(File.Exists(path + ".tmp"));
        }

        [Test]
        public void WriteAllText_OverwritingKeepsPreviousVersionAsBackup()
        {
            SafeFile.WriteAllText(path, "{old}");
            SafeFile.WriteAllText(path, "{new}");

            Assert.AreEqual("{new}", File.ReadAllText(path));
            Assert.AreEqual("{old}", File.ReadAllText(SafeFile.BackupPath(path)));
        }

        [Test]
        public void TryRead_ReturnsPrimaryFile()
        {
            SafeFile.WriteAllText(path, "{ok}");

            Assert.IsTrue(SafeFile.TryRead(path, Parse, out var value));
            Assert.AreEqual("{ok}", value);
        }

        [Test]
        public void TryRead_FallsBackToBackup_WhenPrimaryIsCorrupt()
        {
            SafeFile.WriteAllText(path, "{good}");
            SafeFile.WriteAllText(path, "{newer}");
            File.WriteAllText(path, "garbage");

            Assert.IsTrue(SafeFile.TryRead(path, Parse, out var value));
            Assert.AreEqual("{good}", value);
        }

        [Test]
        public void TryRead_FallsBackToBackup_WhenPrimaryIsMissing()
        {
            SafeFile.WriteAllText(path, "{good}");
            SafeFile.WriteAllText(path, "{newer}");
            File.Delete(path);

            Assert.IsTrue(SafeFile.TryRead(path, Parse, out var value));
            Assert.AreEqual("{good}", value);
        }

        [Test]
        public void TryRead_ReturnsFalse_WhenNothingIsReadable()
        {
            Assert.IsFalse(SafeFile.TryRead(path, Parse, out var value));
            Assert.IsNull(value);
        }

        [Test]
        public void Save_StampsCurrentVersion_AndRoundTrips()
        {
            var repository = new JsonFileSaveRepository(path);
            repository.Save(new PlayerSaveData { characterName = "Hero" });

            Assert.IsTrue(repository.TryLoad(out var loaded));
            Assert.AreEqual("Hero", loaded.characterName);
            Assert.AreEqual(PlayerSaveData.CurrentSaveVersion, loaded.saveVersion);
        }

        [Test]
        public void TryLoad_ReadsLegacyFileWithoutVersion_AsVersionZero()
        {
            Directory.CreateDirectory(directory);
            File.WriteAllText(path, "{\"characterName\":\"Old\"}");

            Assert.IsTrue(new JsonFileSaveRepository(path).TryLoad(out var loaded));
            Assert.AreEqual(0, loaded.saveVersion);
        }
    }
}
