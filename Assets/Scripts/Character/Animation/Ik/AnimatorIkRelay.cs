using System.Collections.Generic;
using UnityEngine;

namespace Project.Character.Animation
{
    /// <summary>
    /// Runs a list of <see cref="IAnimatorIkPass"/> from the Animator IK callback. Unity calls
    /// <c>OnAnimatorIK</c> only on a component that sits on the same GameObject as the Animator, and the
    /// player's model is destroyed and recreated on every gender swap, so the owner of the passes adds this
    /// component to the new model and hands the passes over again. The Animator layer needs its IK Pass enabled.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Animator))]
    public sealed class AnimatorIkRelay : MonoBehaviour
    {
        private static readonly int DeathState = Animator.StringToHash("Death");

        private readonly List<IAnimatorIkPass> passes = new List<IAnimatorIkPass>();
        private Animator animator;

        /// <summary>
        /// Replaces the passes that run on this Animator.
        /// </summary>
        /// <param name="value">The passes, applied in order.</param>
        public void SetPasses(IEnumerable<IAnimatorIkPass> value)
        {
            passes.Clear();
            passes.AddRange(value);
        }

        private void Awake()
        {
            animator = GetComponent<Animator>();
        }

        private void OnAnimatorIK(int layerIndex)
        {
            if (layerIndex != 0)
            {
                return;
            }

            var master = animator.GetCurrentAnimatorStateInfo(0).shortNameHash == DeathState ? 0f : 1f;

            foreach (var pass in passes)
            {
                pass.Apply(animator, Time.deltaTime, master);
            }
        }
    }
}
