using NUnit.Framework;

namespace Project.Quests.Tests
{
    /// <summary>
    /// Tests for the decision rules behind the class trial master's conversation.
    /// </summary>
    public class ClassTrialRulesTests
    {
        [TestCase(false, false, false)]
        [TestCase(true, true, true)]
        public void Resolve_NotNovice_IsAlreadyPromoted(bool completed, bool active, bool canEvolve)
        {
            Assert.AreEqual(ClassTrialStage.AlreadyPromoted, ClassTrialRules.Resolve(false, completed, active, canEvolve));
        }

        [Test]
        public void Resolve_CompletedTrial_WinsOverActiveTrialAndRequirements()
        {
            Assert.AreEqual(ClassTrialStage.ClaimPromotion, ClassTrialRules.Resolve(true, true, true, false));
        }

        [Test]
        public void Resolve_ActiveTrial_BlocksChoosingAnother()
        {
            Assert.AreEqual(ClassTrialStage.TrialActive, ClassTrialRules.Resolve(true, false, true, true));
        }

        [Test]
        public void Resolve_NoTrialAndRequirementsUnmet_IsNotReady()
        {
            Assert.AreEqual(ClassTrialStage.NotReady, ClassTrialRules.Resolve(true, false, false, false));
        }

        [Test]
        public void Resolve_NoTrialAndRequirementsMet_OffersChoice()
        {
            Assert.AreEqual(ClassTrialStage.ChooseTrial, ClassTrialRules.Resolve(true, false, false, true));
        }
    }
}
