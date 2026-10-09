using System.Linq;
using NUnit.Framework;

namespace Project.Character.Combat.Tests
{
    /// <summary>
    /// Tests for <see cref="BodyPartNames"/>.
    /// </summary>
    public class BodyPartNamesTests
    {
        [Test]
        public void Resolve_MaskWithSeveralParts_ReturnsTheirRendererNames()
        {
            var names = BodyPartNames.Resolve(BodyPart.Torso | BodyPart.HandLeft | BodyPart.FootRight).ToArray();

            CollectionAssert.AreEquivalent(new[] { "Body_Torso", "Body_Hand_L", "Body_Foot_R" }, names);
        }

        [Test]
        public void Resolve_None_ReturnsNothing()
        {
            Assert.IsEmpty(BodyPartNames.Resolve(BodyPart.None));
        }

        [TestCase("Body_Pelvis", true)]
        [TestCase("Body_Head", false)]
        [TestCase("PlateArmor_Body", false)]
        public void IsBodyMesh_ReturnsWhetherNameIsABodyRegion(string meshName, bool expected)
        {
            Assert.AreEqual(expected, BodyPartNames.IsBodyMesh(meshName));
        }
    }
}
