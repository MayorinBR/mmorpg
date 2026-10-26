using UnityEngine;

namespace Project.EditorTools
{
    /// <summary>
    /// Creates warp portals by cloning an existing one, so the disc, label and arrival point stay consistent across maps.
    /// </summary>
    public static class PortalFactory
    {
        private const string PortalTypeName = "Project.World.WarpPortal";
        private const string SpawnPointTypeName = "Project.World.MapSpawnPoint";
        private const string LabelTypeName = "TextMeshProUGUI";

        /// <summary>
        /// Clones a portal, retargets it and moves its arrival point.
        /// </summary>
        /// <param name="template">The portal to copy, including its child arrival point and label.</param>
        /// <param name="name">Name of the new portal.</param>
        /// <param name="position">World position of the new portal.</param>
        /// <param name="destinationScene">Scene the portal leads to.</param>
        /// <param name="destinationSpawnId">Arrival point id in the destination scene.</param>
        /// <param name="arrivalSpawnId">Id of the arrival point that sits next to this portal.</param>
        /// <param name="arrivalOffsetX">Local X offset of the arrival point, positive or negative to point inward.</param>
        /// <returns>The new portal.</returns>
        public static GameObject Clone(
            GameObject template,
            string name,
            Vector3 position,
            string destinationScene,
            string destinationSpawnId,
            string arrivalSpawnId,
            float arrivalOffsetX)
        {
            var portal = Object.Instantiate(template);
            portal.name = name;
            portal.transform.position = position;

            EditorReflection.Set(
                portal.GetComponent(EditorReflection.FindType(PortalTypeName)),
                serialized =>
                {
                    serialized.FindProperty("destinationSceneName").stringValue = destinationScene;
                    serialized.FindProperty("destinationSpawnPointId").stringValue = destinationSpawnId;
                });

            ConfigureArrivalPoint(portal, arrivalSpawnId, arrivalOffsetX);
            SetLabel(portal, destinationScene);
            return portal;
        }

        private static void ConfigureArrivalPoint(GameObject portal, string arrivalSpawnId, float offsetX)
        {
            var arrival = portal.GetComponentInChildren(EditorReflection.FindType(SpawnPointTypeName), true);
            EditorReflection.Set(arrival, serialized => serialized.FindProperty("spawnPointId").stringValue = arrivalSpawnId);
            arrival.gameObject.name = $"SpawnPoint_{arrivalSpawnId}";

            var local = arrival.transform.localPosition;
            local.x = offsetX;
            arrival.transform.localPosition = local;
        }

        /// <summary>
        /// Rewrites the label text of a portal so it names the scene the portal actually leads to.
        /// </summary>
        /// <param name="portal">The portal whose label is updated.</param>
        /// <param name="destinationScene">Scene shown on the label.</param>
        public static void Relabel(GameObject portal, string destinationScene)
        {
            SetLabel(portal, destinationScene);
        }

        private static void SetLabel(GameObject portal, string destinationScene)
        {
            var label = EditorReflection.FindByTypeName(portal, LabelTypeName);

            if (label == null)
            {
                return;
            }

            EditorReflection.Set(label, serialized =>
                serialized.FindProperty("m_text").stringValue = $"WARP\n<size=60%>{destinationScene}</size>");
        }
    }
}
