using NUnit.Framework;
using Project.Character.Stats;

namespace Project.Character.Combat.Tests
{
    /// <summary>
    /// Verifies the Base Level and Job Level caps that apply before and after a Novice evolves.
    /// </summary>
    public class ClassProgressionTests
    {
        /// <summary>A Novice stops at Base Level 10.</summary>
        [Test]
        public void GetBaseLevelCap_Novice_IsTen()
        {
            Assert.AreEqual(10, ClassProgression.GetBaseLevelCap(CharacterClass.Novice));
        }

        /// <summary>A Novice stops at Job Level 10.</summary>
        [Test]
        public void GetJobLevelCap_Novice_IsTen()
        {
            Assert.AreEqual(10, ClassProgression.GetJobLevelCap(CharacterClass.Novice));
        }

        /// <summary>Every first class is capped at Base Level 99.</summary>
        [TestCase(CharacterClass.Swordman)]
        [TestCase(CharacterClass.Archer)]
        [TestCase(CharacterClass.Merchant)]
        [TestCase(CharacterClass.Acolyte)]
        [TestCase(CharacterClass.Thief)]
        [TestCase(CharacterClass.Mage)]
        public void GetBaseLevelCap_FirstClass_Is99(CharacterClass characterClass)
        {
            Assert.AreEqual(99, ClassProgression.GetBaseLevelCap(characterClass));
        }

        /// <summary>Every first class is capped at Job Level 50.</summary>
        [TestCase(CharacterClass.Swordman)]
        [TestCase(CharacterClass.Archer)]
        [TestCase(CharacterClass.Merchant)]
        [TestCase(CharacterClass.Acolyte)]
        [TestCase(CharacterClass.Thief)]
        [TestCase(CharacterClass.Mage)]
        public void GetJobLevelCap_FirstClass_Is50(CharacterClass characterClass)
        {
            Assert.AreEqual(50, ClassProgression.GetJobLevelCap(characterClass));
        }
    }
}
