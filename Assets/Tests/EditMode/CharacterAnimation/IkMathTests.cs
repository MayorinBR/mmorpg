using NUnit.Framework;
using Project.Character.Animation;

namespace Project.Character.Animation.Tests
{
    /// <summary>
    /// Covers the weight curves used by the inverse kinematics passes.
    /// </summary>
    public sealed class IkMathTests
    {
        [Test]
        public void FootPlantWeight_IsOneWhenFootIsWithinPlantedHeight()
        {
            Assert.AreEqual(1f, IkMath.FootPlantWeight(0f, 0.02f, 0.15f), 1e-5f);
            Assert.AreEqual(1f, IkMath.FootPlantWeight(0.02f, 0.02f, 0.15f), 1e-5f);
        }

        [Test]
        public void FootPlantWeight_IsZeroWhenFootIsLifted()
        {
            Assert.AreEqual(0f, IkMath.FootPlantWeight(0.15f, 0.02f, 0.15f), 1e-5f);
            Assert.AreEqual(0f, IkMath.FootPlantWeight(0.5f, 0.02f, 0.15f), 1e-5f);
        }

        [Test]
        public void FootPlantWeight_FallsLinearlyBetweenThePlantedAndLiftedHeights()
        {
            Assert.AreEqual(0.5f, IkMath.FootPlantWeight(0.085f, 0.02f, 0.15f), 1e-4f);
        }

        [Test]
        public void LookAngleWeight_IsOneInsideTheComfortableAngle()
        {
            Assert.AreEqual(1f, IkMath.LookAngleWeight(0f, 60f, 120f), 1e-5f);
            Assert.AreEqual(1f, IkMath.LookAngleWeight(60f, 60f, 120f), 1e-5f);
        }

        [Test]
        public void LookAngleWeight_IsZeroBeyondTheLimit()
        {
            Assert.AreEqual(0f, IkMath.LookAngleWeight(120f, 60f, 120f), 1e-5f);
            Assert.AreEqual(0f, IkMath.LookAngleWeight(180f, 60f, 120f), 1e-5f);
        }

        [Test]
        public void LookAngleWeight_FallsLinearlyBetweenTheTwoAngles()
        {
            Assert.AreEqual(0.5f, IkMath.LookAngleWeight(90f, 60f, 120f), 1e-4f);
        }
    }
}
