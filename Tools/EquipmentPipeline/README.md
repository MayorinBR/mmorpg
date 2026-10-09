# Equipment pipeline

Turns a raw static model dropped in `Assets/Models/Items/Equipaments/` into a skinned FBX rigged to the project skeleton.

Needs Blender (the `bpy` module or `blender -b`) with numpy. Files:

- `rig_equipment.py`: analyzes the model against the male and female body, then rigs and exports it.
- `equip_lib.py`: classification, weight transfer (interpolate, smooth, limit 4, clean, normalize), mirror and export helpers.
- `check_classification.py`: regression check that re-classifies the equipment already in the project.

Phase 1, analysis only (writes a report, changes nothing):

    python3 -I rig_equipment.py -- --input Raw.fbx --models <Assets/Models> --out <Equipaments> --report r.json --analyze-only

Phase 2, rig and export with the confirmed parts:

    python3 -I rig_equipment.py -- --input Raw.fbx --models <Assets/Models> --out <Equipaments> --report r.json --covered Torso,UpperArmLeft

Then in Unity: add the item to `Assets/Data/Items/ArmorManifest.json`, run `Tools/Equipment/Build Armor Items`, then `Tools/Equipment/Archive Processed Sources`.
