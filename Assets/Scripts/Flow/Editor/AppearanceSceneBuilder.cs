using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Project.Character.Animation;
using Project.Character.Appearance;
using Project.Character.Combat;
using Project.Character.Hair;
using Project.Character.Head;
using Project.Character.Head.EditorTools;

namespace Project.Flow.EditorTools
{
    /// <summary>
    /// Editor menu commands that assemble the character appearance feature in the open scene:
    /// the responsive Character Creation layout with the preview and selector rows, and the
    /// gameplay Player wiring. Every command records Undo and can be run again safely.
    /// </summary>
    public static class AppearanceSceneBuilder
    {
        private const string MaleGuid = "6776fc426f1ace94f80429ff674709d6";
        private const string FemaleGuid = "e507ad9d7dbd3374a9533edee67eee50";
        private const string AnimatorGuid = "4a3a42b9687649ecb7e3c2a8a3288eee";
        private const float RowHeight = 56f;
        private const float ArrowWidth = 56f;
        private const float TitleWidth = 150f;

        /// <summary>
        /// Lays out the Character Creation screen with anchors, adds the preview and the appearance
        /// rows, and wires every reference. Run it with the CharacterCreation scene open.
        /// </summary>
        [MenuItem("Tools/Character Appearance/Build Character Creation Screen")]
        public static void BuildCreationScreen()
        {
            var creation = Object.FindFirstObjectByType<CharacterCreationController>();
            if (creation == null)
            {
                EditorUtility.DisplayDialog("Appearance", "Open the CharacterCreation scene first.", "OK");
                return;
            }

            var canvasComponent = Object.FindFirstObjectByType<Canvas>(FindObjectsInactive.Include);
            if (canvasComponent == null)
            {
                EditorUtility.DisplayDialog("Appearance", "No Canvas found in the open scene.", "OK");
                return;
            }

            Transform canvas = canvasComponent.transform;
            ConfigureScaler(canvas.GetComponent<CanvasScaler>());
            ApplyLayout(canvas);

            var existing = Object.FindFirstObjectByType<CharacterAppearanceSelectionController>();
            if (existing == null)
            {
                BuildAppearance(canvas, creation);
            }
            else
            {
                AddSkinToneRow(existing);
                Assign(existing, "headCatalog", HeadCatalogBuilder.Build());
            }

            EditorSceneManager.MarkSceneDirty(creation.gameObject.scene);
            Debug.Log("Character Creation screen assembled. Save the scene.");
        }

        /// <summary>
        /// Makes every canvas of the open scene scale with the screen (1920x1080 reference).
        /// </summary>
        [MenuItem("Tools/Character Appearance/Apply Responsive Scaler To Open Scene")]
        public static void ApplyScalerToScene()
        {
            foreach (CanvasScaler scaler in Object.FindObjectsByType<CanvasScaler>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                ConfigureScaler(scaler);
            }

            EditorSceneManager.MarkAllScenesDirty();
        }

        /// <summary>
        /// Adds the appearance controller to the Player and wires it. Run it with the Bootstrap scene open.
        /// </summary>
        [MenuItem("Tools/Character Appearance/Wire Player In Open Scene")]
        public static void WirePlayer()
        {
            var gender = Object.FindFirstObjectByType<PlayerGenderController>();
            if (gender == null)
            {
                EditorUtility.DisplayDialog("Appearance", "Open the Bootstrap scene first.", "OK");
                return;
            }

            var appearance = gender.GetComponent<PlayerAppearanceController>();
            if (appearance == null)
            {
                appearance = Undo.AddComponent<PlayerAppearanceController>(gender.gameObject);
            }

            Assign(appearance, "headCatalog", HeadCatalogBuilder.Build());
            Assign(appearance, "hairCatalog", FindAsset<HairCatalog>());
            Assign(appearance, "faceCatalog", FindAsset<FacePartCatalog>());
            Assign(gender, "appearanceController", appearance);

            var bootstrap = Object.FindFirstObjectByType<CharacterSessionBootstrap>();
            if (bootstrap != null)
            {
                Assign(bootstrap, "appearanceController", appearance);
            }

            EditorSceneManager.MarkSceneDirty(gender.gameObject.scene);
            Debug.Log("Player appearance wired. Save the scene.");
        }

        private static void ConfigureScaler(CanvasScaler scaler)
        {
            if (scaler == null)
            {
                return;
            }

            Undo.RecordObject(scaler, "Responsive scaler");
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
        }

        private static Transform FindDeep(Transform root, string childName)
        {
            foreach (Transform candidate in root.GetComponentsInChildren<Transform>(true))
            {
                if (candidate.name == childName)
                {
                    return candidate;
                }
            }

            return null;
        }

        private static void ApplyLayout(Transform canvas)
        {
            Transform panel = FindDeep(canvas, "Panel");
            if (panel != null && panel.parent != canvas)
            {
                Undo.SetTransformParent(panel, canvas, "Move panel to canvas");
            }

            Stretch(canvas, "Text (TMP)", 0.10f, 0.88f, 0.90f, 0.98f);
            Stretch(canvas, "GenderContainer", 0.02f, 0.72f, 0.36f, 0.84f);
            Stretch(canvas, "ClassContainer", 0.02f, 0.42f, 0.36f, 0.68f);
            Stretch(canvas, "ViewContainer", 0.38f, 0.10f, 0.64f, 0.88f);
            Stretch(canvas, "Panel", 0.67f, 0.10f, 0.98f, 0.88f);
            Point(canvas, "CharacterNameInputField (TMP)", 0.19f, 0.30f);
            Point(canvas, "WarningText (TMP)", 0.19f, 0.22f);
            Point(canvas, "CancelButton", 0.11f, 0.12f);
            Point(canvas, "CreateButton", 0.27f, 0.12f);
        }

        private static void Stretch(Transform canvas, string childName, float minX, float minY, float maxX, float maxY)
        {
            var rect = FindRect(canvas, childName);
            if (rect == null)
            {
                return;
            }

            Undo.RecordObject(rect, "Responsive layout");
            rect.anchorMin = new Vector2(minX, minY);
            rect.anchorMax = new Vector2(maxX, maxY);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void Point(Transform canvas, string childName, float x, float y)
        {
            var rect = FindRect(canvas, childName);
            if (rect == null)
            {
                return;
            }

            Undo.RecordObject(rect, "Responsive layout");
            rect.anchorMin = new Vector2(x, y);
            rect.anchorMax = new Vector2(x, y);
            rect.anchoredPosition = Vector2.zero;
        }

        private static RectTransform FindRect(Transform canvas, string childName)
        {
            Transform child = FindDeep(canvas, childName);
            if (child == null)
            {
                Debug.LogWarning($"'{childName}' was not found under the canvas and was left unchanged.");
            }

            return child as RectTransform;
        }

        private static void BuildAppearance(Transform canvas, CharacterCreationController creation)
        {
            var viewContainer = FindDeep(canvas, "ViewContainer");
            var panel = FindDeep(canvas, "Panel");
            if (viewContainer == null || panel == null)
            {
                Debug.LogError("ViewContainer and Panel are required under the canvas.");
                return;
            }

            var preview = viewContainer.GetComponent<CharacterPreview>();
            if (preview == null)
            {
                preview = Undo.AddComponent<CharacterPreview>(viewContainer.gameObject);
            }

            Assign(preview, "malePrefab", LoadByGuid<GameObject>(MaleGuid));
            Assign(preview, "femalePrefab", LoadByGuid<GameObject>(FemaleGuid));
            Assign(preview, "sharedAnimatorController", LoadByGuid<RuntimeAnimatorController>(AnimatorGuid));

            PreparePanel(panel);

            var skinTone = CreateRow(panel, "Skin Color");
            var hairStyle = CreateRow(panel, "Hair");
            var hairColor = CreateRow(panel, "Hair Color");
            var eyeStyle = CreateRow(panel, "Eyes");
            var eyeColor = CreateRow(panel, "Eye Color");
            var mouthStyle = CreateRow(panel, "Mouth");
            var headModel = CreateRow(panel, "Head");
            var bodyModel = CreateRow(panel, "Body");

            var controller = Undo.AddComponent<CharacterAppearanceSelectionController>(panel.gameObject);
            Assign(controller, "headCatalog", HeadCatalogBuilder.Build());
            Assign(controller, "hairCatalog", FindAsset<HairCatalog>());
            Assign(controller, "faceCatalog", FindAsset<FacePartCatalog>());
            Assign(controller, "preview", preview);
            Assign(controller, "skinToneSelector", skinTone);
            Assign(controller, "hairStyleSelector", hairStyle);
            Assign(controller, "hairColorSelector", hairColor);
            Assign(controller, "eyeStyleSelector", eyeStyle);
            Assign(controller, "eyeColorSelector", eyeColor);
            Assign(controller, "mouthStyleSelector", mouthStyle);
            Assign(controller, "headModelSelector", headModel);
            Assign(controller, "bodyModelSelector", bodyModel);
            Assign(creation, "appearanceSelection", controller);
        }

        private static void AddSkinToneRow(CharacterAppearanceSelectionController controller)
        {
            var serialized = new SerializedObject(controller);
            SerializedProperty property = serialized.FindProperty("skinToneSelector");
            if (property == null || property.objectReferenceValue != null)
            {
                return;
            }

            AppearanceOptionSelector row = CreateRow(controller.transform, "Skin Color");
            row.transform.SetSiblingIndex(0);
            property.objectReferenceValue = row;
            serialized.ApplyModifiedProperties();
        }

        private static void PreparePanel(Transform panel)
        {
            var background = panel.GetComponent<Image>();
            if (background == null)
            {
                background = Undo.AddComponent<Image>(panel.gameObject);
                background.color = new Color(0f, 0f, 0f, 0.55f);
            }

            var layout = panel.GetComponent<VerticalLayoutGroup>();
            if (layout == null)
            {
                layout = Undo.AddComponent<VerticalLayoutGroup>(panel.gameObject);
            }

            layout.padding = new RectOffset(20, 20, 20, 20);
            layout.spacing = 12f;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
        }

        private static AppearanceOptionSelector CreateRow(Transform parent, string title)
        {
            var row = NewUiObject($"Row_{title}", parent);
            var layout = row.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 8f;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = true;
            row.AddComponent<LayoutElement>().preferredHeight = RowHeight;

            var titleText = NewText("Title", row.transform, title, TextAlignmentOptions.MidlineLeft);
            titleText.gameObject.AddComponent<LayoutElement>().preferredWidth = TitleWidth;

            var previous = NewArrowButton("Previous", row.transform, "<");
            var value = NewText("Value", row.transform, "-", TextAlignmentOptions.Center);
            value.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1f;
            var next = NewArrowButton("Next", row.transform, ">");

            var selector = row.AddComponent<AppearanceOptionSelector>();
            Assign(selector, "previousButton", previous);
            Assign(selector, "nextButton", next);
            Assign(selector, "label", value);
            return selector;
        }

        private static Button NewArrowButton(string name, Transform parent, string symbol)
        {
            var go = NewUiObject(name, parent);
            go.AddComponent<LayoutElement>().preferredWidth = ArrowWidth;

            var image = go.AddComponent<Image>();
            image.sprite = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            image.type = Image.Type.Sliced;

            var button = go.AddComponent<Button>();
            button.targetGraphic = image;

            var text = NewText("Text", go.transform, symbol, TextAlignmentOptions.Center);
            text.color = Color.black;
            var rect = text.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return button;
        }

        private static TextMeshProUGUI NewText(string name, Transform parent, string content, TextAlignmentOptions alignment)
        {
            var go = NewUiObject(name, parent);
            var text = go.AddComponent<TextMeshProUGUI>();
            text.text = content;
            text.alignment = alignment;
            text.fontSize = 26f;
            text.color = Color.white;
            text.raycastTarget = false;
            return text;
        }

        private static GameObject NewUiObject(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            Undo.RegisterCreatedObjectUndo(go, "Create appearance UI");
            go.transform.SetParent(parent, false);
            return go;
        }

        private static void Assign(Object target, string field, Object value)
        {
            var serialized = new SerializedObject(target);
            SerializedProperty property = serialized.FindProperty(field);
            if (property == null)
            {
                Debug.LogError($"Field '{field}' was not found on {target.GetType().Name}.");
                return;
            }

            property.objectReferenceValue = value;
            serialized.ApplyModifiedProperties();
        }

        private static T LoadByGuid<T>(string guid) where T : Object
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            T asset = string.IsNullOrEmpty(path) ? null : AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null)
            {
                Debug.LogWarning($"Asset {guid} ({typeof(T).Name}) was not found. Assign it by hand.");
            }

            return asset;
        }

        private static T FindAsset<T>() where T : Object
        {
            string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
            if (guids.Length == 0)
            {
                Debug.LogWarning($"No {typeof(T).Name} asset exists. Assign it by hand.");
                return null;
            }

            return AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(guids[0]));
        }
    }
}
