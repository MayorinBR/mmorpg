using UnityEngine;

namespace Project.Combat
{
    /// <summary>
    /// Marks the visual of a skill area (a flat disc of radius one) and
    /// scales it to the radius of the skill that uses it. Placed on the
    /// zone prefab of ground skills and on the placement preview.
    /// </summary>
    public class SkillAreaVisual : MonoBehaviour
    {
        /// <summary>
        /// Scales the visual so its edge matches a world radius.
        /// </summary>
        /// <param name="radius">The radius of the area, in world units.</param>
        public void SetRadius(float radius)
        {
            transform.localScale = new Vector3(radius, 1f, radius);
        }
    }
}
