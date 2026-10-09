using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Project.Flow.EditorTools
{
    /// <summary>
    /// One-time migration after the body parts of the low-poly models were renamed from
    /// <c>part_N</c> to region names. Reassigns the meshes of the Male_Body and Female_Body prefabs
    /// from the re-imported FBX files, renames the part objects, and updates the head mesh name
    /// stored on the face and head components.
    /// </summary>
    public static class BodyPartRelinker
    {
        private const string MalePrefabGuid = "6776fc426f1ace94f80429ff674709d6";
        private const string FemalePrefabGuid = "e507ad9d7dbd3374a9533edee67eee50";
        private const string MaleModelGuid = "b0c1cf74da728d14cbc68646636d1a88";
        private const string FemaleModelGuid = "cdd1eeccbec1a3046b1d06c1b8000ae2";
        private const string OldHeadName = "part_0";
        private const string NewHeadName = "Body_Head";

        private static readonly string[] MaleParts =
        {
            "Body_Head", "Body_Pelvis", "Body_Torso", "Body_UpperArm_L", "Body_UpperArm_R", "Body_Leg_R",
            "Body_Leg_L", "Body_Foot_R", "Body_Foot_L", "Body_Hand_R", "Body_Hand_L"
        };

        private static readonly string[] FemaleParts =
        {
            "Body_Head", "Body_Pelvis", "Body_Torso", "Body_Leg_R", "Body_Leg_L", "Body_Foot_R",
            "Body_Foot_L", "Body_UpperArm_R", "Body_UpperArm_L", "Body_Hand_R", "Body_Hand_L"
        };

        /// <summary>
        /// Relinks both body prefabs to the renamed parts. Safe to run again.
        /// </summary>
        [MenuItem("Tools/Character Appearance/Relink Body Part Meshes")]
        public static void RelinkAll()
        {
            Relink(MalePrefabGuid, MaleModelGuid, MaleParts);
            Relink(FemalePrefabGuid, FemaleModelGuid, FemaleParts);
            AssetDatabase.SaveAssets();
        }

        private static void Relink(string prefabGuid, string modelGuid, string[] partNames)
        {
            string prefabPath = AssetDatabase.GUIDToAssetPath(prefabGuid);
            string modelPath = AssetDatabase.GUIDToAssetPath(modelGuid);
            if (string.IsNullOrEmpty(prefabPath) || string.IsNullOrEmpty(modelPath))
            {
                Debug.LogError($"Prefab {prefabGuid} or model {modelGuid} was not found.");
                return;
            }

            Dictionary<string, Mesh> meshes = AssetDatabase.LoadAllAssetsAtPath(modelPath)
                .OfType<Mesh>()
                .GroupBy(mesh => mesh.name)
                .ToDictionary(group => group.Key, group => group.First());

            GameObject root = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                int relinked = 0;
                foreach (SkinnedMeshRenderer renderer in root.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                {
                    if (!TryResolveName(renderer.name, partNames, out string newName))
                    {
                        continue;
                    }

                    if (!meshes.TryGetValue(newName, out Mesh mesh))
                    {
                        Debug.LogError($"Mesh '{newName}' was not found in {modelPath}.");
                        continue;
                    }

                    renderer.sharedMesh = mesh;
                    renderer.gameObject.name = newName;
                    relinked++;
                }

                UpdateHeadNames(root);
                PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
                Debug.Log($"{prefabPath}: {relinked} part meshes relinked.");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(root);
            }
        }

        private static bool TryResolveName(string objectName, string[] partNames, out string newName)
        {
            newName = null;
            if (objectName.StartsWith("Body_"))
            {
                newName = objectName;
                return true;
            }

            const string prefix = "part_";
            if (objectName.StartsWith(prefix) && int.TryParse(objectName.Substring(prefix.Length), out int index)
                && index >= 0 && index < partNames.Length)
            {
                newName = partNames[index];
                return true;
            }

            return false;
        }

        private static void UpdateHeadNames(GameObject root)
        {
            foreach (MonoBehaviour behaviour in root.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (behaviour == null)
                {
                    continue;
                }

                var serialized = new SerializedObject(behaviour);
                foreach (string field in new[] { "faceMeshName", "headMeshName" })
                {
                    SerializedProperty property = serialized.FindProperty(field);
                    if (property != null && property.stringValue == OldHeadName)
                    {
                        property.stringValue = NewHeadName;
                    }
                }

                serialized.ApplyModifiedPropertiesWithoutUndo();
            }
        }
    }
}
