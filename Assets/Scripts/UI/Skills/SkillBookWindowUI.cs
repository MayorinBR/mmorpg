using Project.Character.Combat;
using Project.Character.Stats;
using Project.Skills;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Project.UI
{
    /// <summary>
    /// Skill Book window. One tab per class tier the player has reached
    /// (Novice, 1st Class, 2nd Class); each tab shows every skill of that
    /// class at once as a compact grid of <see cref="SkillBookEntryUI"/>
    /// cards (icon, name, level, learn button), so no scrolling is needed.
    /// Class databases are resolved through <see cref="ClassSkillDatabaseLookup"/>.
    /// The window refreshes when a skill is learned, the player levels up or
    /// the class changes. Its layout (tab strip and grid) is built at
    /// runtime under <see cref="contentRoot"/>, replacing any layout group
    /// authored there.
    /// </summary>
    public class SkillBookWindowUI : MonoBehaviour
    {
        private const float HeaderHeight = 32f;
        private const float TabHeight = 28f;
        private const float Padding = 8f;
        private const int Columns = 5;

        private static readonly string[] TierNames = { "Novice", "1st Class", "2nd Class" };
        private static readonly Color SelectedTabColor = new Color(0.3f, 0.42f, 0.78f, 1f);
        private static readonly Color ReachedTabColor = new Color(0.2f, 0.22f, 0.3f, 1f);
        private static readonly Color LockedTabColor = new Color(0.15f, 0.15f, 0.15f, 0.6f);

        [SerializeField] private ClassSkillDatabaseLookup skillDatabaseLookup;
        [SerializeField] private PlayerSkillBook skillBook;
        [SerializeField] private PlayerClassController classController;
        [SerializeField] private PlayerExperience experience;
        [SerializeField] private Transform contentRoot;

        [Tooltip("Optional. Buttons for Novice, 1st Class, 2nd Class, in order. Left empty, the tabs are created at runtime.")]
        [SerializeField] private Button[] tabButtons;

        [Tooltip("Optional. Container for the runtime-created tabs. Left empty, a strip is created at the top of the window.")]
        [SerializeField] private RectTransform tabsRoot;

        private RectTransform gridRoot;
        private int selectedTab;

        private void Start()
        {
            BuildLayout();
            BindTabs();
            selectedTab = GetLastTab();
            Rebuild();
            skillBook.SkillLeveledUp += OnSkillLeveledUp;
            classController.ClassChanged += OnClassChanged;

            if (experience != null)
            {
                experience.LeveledUp += OnPlayerLeveledUp;
            }
        }

        private void OnDestroy()
        {
            skillBook.SkillLeveledUp -= OnSkillLeveledUp;
            classController.ClassChanged -= OnClassChanged;

            if (experience != null)
            {
                experience.LeveledUp -= OnPlayerLeveledUp;
            }
        }

        private void BuildLayout()
        {
            foreach (var group in contentRoot.GetComponents<LayoutGroup>())
            {
                DestroyImmediate(group);
            }

            var fitter = contentRoot.GetComponent<ContentSizeFitter>();

            if (fitter != null)
            {
                DestroyImmediate(fitter);
            }

            var panel = (RectTransform)contentRoot;
            panel.anchorMin = Vector2.zero;
            panel.anchorMax = Vector2.one;
            panel.offsetMin = Vector2.zero;
            panel.offsetMax = new Vector2(0f, -HeaderHeight);

            if (tabButtons == null || tabButtons.Length == 0)
            {
                CreateTabs(tabsRoot != null ? tabsRoot : CreateTabsStrip(panel));
            }

            gridRoot = new GameObject("SkillGrid", typeof(RectTransform), typeof(GridLayoutGroup)).GetComponent<RectTransform>();
            gridRoot.SetParent(panel, false);
            gridRoot.anchorMin = Vector2.zero;
            gridRoot.anchorMax = Vector2.one;
            gridRoot.offsetMin = new Vector2(Padding, Padding);
            gridRoot.offsetMax = new Vector2(-Padding, -(TabHeight + Padding * 2f));

            var grid = gridRoot.GetComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(SkillBookEntryUI.CardWidth, SkillBookEntryUI.CardHeight);
            grid.spacing = new Vector2(4f, 4f);
            grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
            grid.startAxis = GridLayoutGroup.Axis.Horizontal;
            grid.childAlignment = TextAnchor.UpperCenter;
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = Columns;
        }

        private static RectTransform CreateTabsStrip(RectTransform panel)
        {
            var strip = new GameObject("Tabs", typeof(RectTransform), typeof(HorizontalLayoutGroup)).GetComponent<RectTransform>();
            strip.SetParent(panel, false);
            strip.anchorMin = new Vector2(0f, 1f);
            strip.anchorMax = new Vector2(1f, 1f);
            strip.pivot = new Vector2(0.5f, 1f);
            strip.offsetMin = new Vector2(Padding, -(Padding + TabHeight));
            strip.offsetMax = new Vector2(-Padding, -Padding);

            var layout = strip.GetComponent<HorizontalLayoutGroup>();
            layout.spacing = 4f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = true;
            return strip;
        }

        private void CreateTabs(Transform root)
        {
            tabButtons = new Button[TierNames.Length];

            for (var i = 0; i < tabButtons.Length; i++)
            {
                tabButtons[i] = CreateTabButton(root, TierNames[i]);
            }
        }

        private static Button CreateTabButton(Transform parent, string text)
        {
            var buttonObject = new GameObject(text + " Tab", typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);

            var labelObject = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            labelObject.transform.SetParent(buttonObject.transform, false);
            var labelRect = labelObject.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            var label = labelObject.GetComponent<TextMeshProUGUI>();
            label.text = text;
            label.alignment = TextAlignmentOptions.Center;
            label.enableAutoSizing = true;
            label.fontSizeMin = 8f;
            label.fontSizeMax = 14f;
            label.textWrappingMode = TextWrappingModes.NoWrap;
            label.overflowMode = TextOverflowModes.Ellipsis;
            label.raycastTarget = false;
            label.color = Color.white;
            return buttonObject.GetComponent<Button>();
        }

        private void BindTabs()
        {
            for (var i = 0; i < tabButtons.Length; i++)
            {
                var tabIndex = i;
                tabButtons[i].onClick.AddListener(() => SelectTab(tabIndex));
            }
        }

        private void SelectTab(int tabIndex)
        {
            if (tabIndex >= classController.ClassPath.Count || tabIndex == selectedTab)
            {
                return;
            }

            selectedTab = tabIndex;
            Rebuild();
        }

        private void RefreshTabs()
        {
            var path = classController.ClassPath;

            for (var i = 0; i < tabButtons.Length; i++)
            {
                var reached = i < path.Count;
                tabButtons[i].interactable = reached;

                if (tabButtons[i].targetGraphic is Image background)
                {
                    background.color = !reached ? LockedTabColor : i == selectedTab ? SelectedTabColor : ReachedTabColor;
                }

                var label = tabButtons[i].GetComponentInChildren<TMP_Text>();

                if (label != null)
                {
                    label.text = reached ? path[i].ToString() : TierNames[Mathf.Min(i, TierNames.Length - 1)];
                }
            }
        }

        private void Rebuild()
        {
            ClearEntries();
            Populate();
            RefreshTabs();
        }

        private void Populate()
        {
            var database = skillDatabaseLookup.GetDatabase(classController.ClassPath[selectedTab]);

            if (database == null)
            {
                return;
            }

            foreach (var skill in database.AllSkills)
            {
                if (skill == null)
                {
                    continue;
                }

                SkillBookEntryUI.CreateCard(gridRoot).Setup(skill, skillBook, experience);
            }
        }

        private void OnSkillLeveledUp(SkillDefinition skill, int newLevel)
        {
            RefreshAllEntries();
        }

        private void OnPlayerLeveledUp(int newLevel)
        {
            RefreshAllEntries();
        }

        private void OnClassChanged(CharacterClass newClass)
        {
            selectedTab = GetLastTab();
            Rebuild();
        }

        private int GetLastTab()
        {
            return Mathf.Max(0, classController.ClassPath.Count - 1);
        }

        private void ClearEntries()
        {
            for (var i = gridRoot.childCount - 1; i >= 0; i--)
            {
                var child = gridRoot.GetChild(i).gameObject;
                child.SetActive(false);
                Destroy(child);
            }
        }

        private void RefreshAllEntries()
        {
            foreach (Transform child in gridRoot)
            {
                var entry = child.GetComponent<SkillBookEntryUI>();

                if (entry != null)
                {
                    entry.Refresh();
                }
            }
        }
    }
}
