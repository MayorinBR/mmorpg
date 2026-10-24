using NUnit.Framework;
using Project.Character.Stats;

namespace Project.Character.Combat.Tests
{
    /// <summary>
    /// Verifies the level caps that apply before and after a Novice evolves.
    /// </summary>
    public class ClassProgressionTests
    {
        /// <summary>A Novice stops at level 10.</summary>
        [Test]
        public void GetLevelCap_Novice_IsTen()
        {
            Assert.AreEqual(10, ClassProgression.GetLevelCap(CharacterClass.Novice));
        }

        /// <summary>Every evolved class is capped at level 99.</summary>
        [TestCase(CharacterClass.Swordman)]
        [TestCase(CharacterClass.Archer)]
        [TestCase(CharacterClass.Merchant)]
        [TestCase(CharacterClass.Acolyte)]
        [TestCase(CharacterClass.Thief)]
        [TestCase(CharacterClass.Mage)]
        public void GetLevelCap_EvolvedClass_Is99(CharacterClass characterClass)
        {
            Assert.AreEqual(99, ClassProgression.GetLevelCap(characterClass));
        }
    }
}
