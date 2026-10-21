using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace City2.Editor
{
    /// <summary>
    /// Keeps the player out of the sea by marking the submerged band of the City2 terrain as non-walkable,
    /// and places the warp portal to Map04 on the southern island.
    /// </summary>
    public static class City2SeaGuard
    {
        private const string VolumeName = "SeaNavBlock";
        private const string PortalName = "WarpPortal_ToMap04";
        private const string ArrivalName = "SpawnPoint_FromMap04";
        private const string VolumeTypeName = "Unity.AI.Navigation.NavMeshModifierVolume, Unity.AI.Navigation";
        private const int NotWalkableArea = 1;
        private const float WalkableFloor = -0.6f;
        private const float VolumeHeight = 3f;
        private const float VolumeExtent = 400f;
        private static readonly Vector3 IslandCenter = new Vector3(-6.25f, 1f, -63.3f);
        private static readonly Vector3 ArrivalLocalPosition = new Vector3(4.1f, -1f, 0f);

        /// <summary>Adds the sea blocking volume, moves the Map04 portal onto the island and rebakes the NavMesh.</summary>
        [MenuItem("Tools/City2/Block Sea And Fix Portal")]
        public static void Apply()
        {
            EnsureVolume();
            PlacePortal();
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            EditorApplication.ExecuteMenuItem("Tools/City2/Bake NavMesh");
        }

        private static void EnsureVolume()
        {
            var volumeType = System.Type.GetType(VolumeTypeName);
            if (volumeType == null)
            {
                Debug.LogError("NavMeshModifierVolume not found. Install the AI Navigation package.");
                return;
            }

            var host = GameObject.Find(VolumeName) ?? new GameObject(VolumeName);
            host.transform.position = new Vector3(0f, WalkableFloor - VolumeHeight * 0.5f, 0f);
            var volume = host.GetComponent(volumeType) ?? host.AddComponent(volumeType);

            var serialized = new SerializedObject(volume);
            serialized.FindProperty("m_Size").vector3Value = new Vector3(VolumeExtent, VolumeHeight, VolumeExtent);
            serialized.FindProperty("m_Center").vector3Value = Vector3.zero;
            serialized.FindProperty("m_Area").intValue = NotWalkableArea;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void PlacePortal()
        {
            var portal = GameObject.Find(PortalName);
            if (portal == null)
            {
                Debug.LogWarning($"{PortalName} not found in the open scene.");
                return;
            }

            portal.transform.position = IslandCenter;
            var arrival = portal.transform.Find(ArrivalName);
            if (arrival != null)
            {
                arrival.localPosition = ArrivalLocalPosition;
            }
        }
    }
}
