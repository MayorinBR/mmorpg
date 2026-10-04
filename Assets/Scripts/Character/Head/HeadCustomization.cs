using System.Collections.Generic;
using UnityEngine;

namespace Project.Character.Head
{
    /// <summary>
    /// Swaps the head mesh of a character model for one of the shapes in a <see cref="HeadCatalog"/>.
    /// Only the mesh and its bone list are replaced on the existing head renderer, so the material,
    /// the skin color and the face drawings written to that renderer stay in place. Head models
    /// share the skeleton of the body, and their bones are matched to the character by name.
    /// </summary>
    public sealed class HeadCustomization : MonoBehaviour
    {
        [Tooltip("Name of the head renderer. The first skinned renderer with this name is used.")]
        [SerializeField] private string headMeshName = "part_0";

        [SerializeField] private HeadCatalog catalog;

        [Tooltip("Whether the character wears the female body. Selects the model of each head shape.")]
        [SerializeField] private bool female;

        private SkinnedMeshRenderer target;
        private Mesh originalMesh;
        private Transform[] originalBones;

        /// <summary>
        /// Sets the catalog and body this component uses, for components that are added to a model
        /// at runtime and so cannot be configured in the inspector.
        /// </summary>
        /// <param name="newCatalog">The head catalog.</param>
        /// <param name="isFemale">True when the character wears the female body.</param>
        public void Configure(HeadCatalog newCatalog, bool isFemale)
        {
            catalog = newCatalog;
            female = isFemale;
        }

        /// <summary>
        /// Shows a head shape. Styles without a model for this body restore the original head.
        /// </summary>
        /// <param name="style">Index of the head shape in the catalog.</param>
        public void Apply(int style)
        {
            if (!TryCaptureOriginal())
            {
                return;
            }

            if (catalog == null || !catalog.TryGetModel(style, female, out GameObject model))
            {
                RestoreOriginal();
                return;
            }

            SkinnedMeshRenderer source = model.GetComponentInChildren<SkinnedMeshRenderer>(true);
            Transform[] bones = source == null ? null : MapBones(source.bones);
            if (bones == null)
            {
                Debug.LogWarning($"Head model '{model.name}' does not match the skeleton of '{name}'. The original head is kept.");
                RestoreOriginal();
                return;
            }

            target.sharedMesh = source.sharedMesh;
            target.bones = bones;
        }

        private bool TryCaptureOriginal()
        {
            if (target != null)
            {
                return true;
            }

            foreach (SkinnedMeshRenderer candidate in GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                if (candidate.name == headMeshName)
                {
                    target = candidate;
                    originalMesh = candidate.sharedMesh;
                    originalBones = candidate.bones;
                    return true;
                }
            }

            return false;
        }

        private void RestoreOriginal()
        {
            target.sharedMesh = originalMesh;
            target.bones = originalBones;
        }

        private Transform[] MapBones(Transform[] sourceBones)
        {
            var byName = new Dictionary<string, Transform>();
            foreach (Transform candidate in GetComponentsInChildren<Transform>(true))
            {
                byName.TryAdd(candidate.name, candidate);
            }

            var mapped = new Transform[sourceBones.Length];
            for (int i = 0; i < sourceBones.Length; i++)
            {
                if (sourceBones[i] == null || !byName.TryGetValue(sourceBones[i].name, out mapped[i]))
                {
                    return null;
                }
            }

            return mapped;
        }
    }
}
