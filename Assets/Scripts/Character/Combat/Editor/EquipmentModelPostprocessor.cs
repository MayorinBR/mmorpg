using UnityEditor;

namespace Project.Character.Combat.EditorTools
{
    /// <summary>
    /// Applies the project import settings to a newly added equipment FBX: scale 1.7 to match the
    /// character, a Generic rig and material descriptions so materials can be remapped. Only the
    /// first import is touched, so settings changed later by hand are kept.
    /// </summary>
    public sealed class EquipmentModelPostprocessor : AssetPostprocessor
    {
        private const string EquipmentRoot = "Assets/Models/Items/Equipaments/";
        private const float EquipmentScale = 1.7f;

        private void OnPreprocessModel()
        {
            if (!assetPath.StartsWith(EquipmentRoot) || !assetImporter.importSettingsMissing)
            {
                return;
            }

            var importer = (ModelImporter)assetImporter;
            importer.globalScale = EquipmentScale;
            importer.animationType = ModelImporterAnimationType.Generic;
            importer.materialImportMode = ModelImporterMaterialImportMode.ImportViaMaterialDescription;
        }
    }
}
