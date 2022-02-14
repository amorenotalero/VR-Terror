using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using ThunderWire.Editors;
using HFPS.Systems;

#if TW_LOCALIZATION_PRESENT
using ThunderWire.Localization;
using ThunderWire.Localization.Editor;
#endif

namespace HFPS.Editors
{
    public class InventoryEditorWindow : EditorWindow
    {
        [SerializeField] TreeViewState m_TreeViewState;
        ItemsTreeView m_TreeView;

        protected InventoryScriptable inventory;
        protected Vector2 ScrollPosition;

        private bool localizationExist = false;

        public void Initialize(InventoryScriptable database)
        {
            if (m_TreeViewState == null)
                m_TreeViewState = new TreeViewState();

#if TW_LOCALIZATION_PRESENT
            localizationExist = LocalizationSystem.HasReference;
#endif

            inventory = database;
            m_TreeView = new ItemsTreeView(m_TreeViewState, database);
        }

        void OnGUI()
        {
            Rect toolbarRect = new Rect(0, 0, position.width, 20f);
            EditorGUI.LabelField(toolbarRect, GUIContent.none, EditorStyles.toolbar);

            Rect exportToLoc = toolbarRect;
            exportToLoc.xMin = toolbarRect.xMax - 180f;

            using (new EditorGUI.DisabledGroupScope(!localizationExist))
            {
                if (GUI.Button(exportToLoc, "Export Items to Localization", EditorStyles.toolbarButton))
                {
#if TW_LOCALIZATION_PRESENT
                    EditorWindow browser = GetWindow<InventoryItemsExport>(true, "Export Items to Localization Map", true);
                    browser.minSize = new Vector2(600, 200);
                    browser.maxSize = new Vector2(600, 200);
                    ((InventoryItemsExport)browser).Show(inventory);
#endif
                }
            }

            Rect treeViewRect = new Rect(5f, 25f, position.width - 10f, position.height - 30f);
            m_TreeView.OnGUI(treeViewRect);
        }
    }

    internal class ItemTreeElement : TreeViewItem
    {
        public SerializedProperty ItemProperty { get; set; }

        public int ItemID;
        public string Title;
        public bool ItemTogglesState;
        public bool ItemSoundsState;
        public bool ItemSettingsState;
        public bool UseActionSettingsState;
        public bool CombineSettingsState;
        public bool LocalizationSettingsState;

        public ItemTreeElement(int id, int iid, int depth, string displayName, SerializedProperty property) : base(id, depth, displayName)
        {
            ItemProperty = property;
            ItemID = iid;
            Title = displayName;
        }

        public ItemTreeElement(int id, int depth, string displayName) : base(id, depth, displayName) { }
    }

    internal class ItemsTreeView : TreeView
    {
        protected List<ItemTreeElement> treeViewElements = new List<ItemTreeElement>();

        protected SerializedObject assetObject;
        protected InventoryScriptable assetRef;

        protected SerializedProperty listProperty;
        protected SerializedProperty localization;

        protected int selectedIndex;
        protected Vector2 scrollPosition;

        protected bool localizationExist = false;
        protected bool m_InitiateContextMenuOnNextRepaint = false;

        public static float Spacing => EditorGUIUtility.standardVerticalSpacing * 2;

        public ItemsTreeView(TreeViewState treeViewState, InventoryScriptable asset) : base(treeViewState)
        {
            assetObject = new SerializedObject(asset);
            listProperty = assetObject.FindProperty("ItemDatabase");
            localization = assetObject.FindProperty("enableLocalization");
            assetRef = asset;

#if TW_LOCALIZATION_PRESENT
            localizationExist = LocalizationSystem.HasReference;
#endif

            selectedIndex = -1;
            Reload();
        }

        protected override TreeViewItem BuildRoot()
        {
            assetObject.Update();
            treeViewElements.Clear();

            var root = new TreeViewItem(0, -1);
            treeViewElements.Add(new ItemTreeElement(0, -1, "Root"));

            for (int i = 0; i < listProperty.arraySize; i++)
            {
                SerializedProperty itemProperty = listProperty.GetArrayElementAtIndex(i);
                SerializedProperty title = itemProperty.FindPropertyRelative("Title");
                itemProperty.FindPropertyRelative("ID").intValue = i;

                ItemTreeElement item = new ItemTreeElement(i + 1, i, 0, title.stringValue, itemProperty);
                treeViewElements.Add(item);
                root.AddChild(item);
            }

            if (root.children == null)
                root.children = new List<TreeViewItem>();

            assetObject.ApplyModifiedProperties();
            return root;
        }

        protected override void ContextClickedItem(int id)
        {
            m_InitiateContextMenuOnNextRepaint = true;
            Repaint();
        }

        protected override bool CanRename(TreeViewItem item) => true;

        protected override void RenameEnded(RenameEndedArgs args)
        {
            string newName = args.newName;

            if (string.IsNullOrEmpty(newName))
            {
                int count = listProperty.arraySize;
                newName = count > 0 ? "New Item " + count : "New Item";
            }

            if (args.acceptedRename && !assetRef.ItemDatabase.Any(x => x.Title.Equals(newName)))
            {
                assetRef.ItemDatabase[args.itemID - 1].Title = newName;
                Reload();
            }
        }

        protected override float GetCustomRowHeight(int row, TreeViewItem item) => 20;

        protected override bool CanMultiSelect(TreeViewItem item) => true;

        protected override bool CanStartDrag(CanStartDragArgs args) => true;

        protected override void SetupDragAndDrop(SetupDragAndDropArgs args)
        {
            DragAndDrop.PrepareStartDrag();
            DragAndDrop.SetGenericData("IDs", args.draggedItemIDs.ToArray());
            DragAndDrop.SetGenericData("Type", "InventoryItems");
            DragAndDrop.StartDrag("Items");
        }

        private void PopUpContextMenu()
        {
            var menu = new GenericMenu();

            menu.AddItem(new GUIContent("Insert Key"), false, () =>
            {
                AddNewItem(GetSelection()[0] + 1);
            });

            if (GetSelection().Count <= 1)
            {
                menu.AddItem(new GUIContent("Rename"), false, () =>
                {
                    var treeItem = treeViewElements[GetSelection()[0]];
                    BeginRename(treeItem);
                });
            }
            else
            {
                menu.AddDisabledItem(new GUIContent("Rename"));
            }

            menu.AddItem(new GUIContent("Delete"), false, () =>
            {
                DeleteSelected();
            });

            menu.ShowAsContext();
        }

        protected override DragAndDropVisualMode HandleDragAndDrop(DragAndDropArgs args)
        {
            int[] draggedIDs = (int[])DragAndDrop.GetGenericData("IDs");
            string type = (string)DragAndDrop.GetGenericData("Type");

            switch (args.dragAndDropPosition)
            {
                case DragAndDropPosition.BetweenItems:
                    {
                        if (type.Equals("InventoryItems"))
                        {
                            if (args.performDrop)
                            {
                                MoveElements(draggedIDs, args.insertAtIndex);
                            }

                            return DragAndDropVisualMode.Move;
                        }
                        return DragAndDropVisualMode.Rejected;
                    }
                case DragAndDropPosition.UponItem:
                case DragAndDropPosition.OutsideItems:
                    return DragAndDropVisualMode.Rejected;
                default:
                    Debug.LogError("Unhandled enum " + args.dragAndDropPosition);
                    return DragAndDropVisualMode.None;
            }
        }

        protected void MoveElements(int[] drag, int insert)
        {
            var database = assetRef.ItemDatabase.ToArray();

            foreach (var i in drag)
            {
                var item = database[i - 1];
                int index = assetRef.ItemDatabase.IndexOf(item);
                int insertTo = insert > index ? insert - 1 : insert;

                assetRef.ItemDatabase.RemoveAt(index);
                assetRef.ItemDatabase.Insert(insertTo, item);
            }

            SetSelection(new int[0]);
            Reload();
        }

        private void AddNewItem(int insert = -1)
        {
            int count = listProperty.arraySize;

            string itemTitle = count > 0 ? "New Item " + count : "New Item";
            var newItem = new InventoryScriptable.ItemMapper() { Title = itemTitle };

            if (insert < 0)
                assetRef.ItemDatabase.Add(newItem);
            else
                assetRef.ItemDatabase.Insert(insert, newItem);

            Reload();

            SetSelection(new List<int>() { count + 1});
            selectedIndex = count + 1;
        }

        private void DeleteSelected()
        {
            foreach (var index in GetSelection().Select(x => x - 1).OrderByDescending(i => i))
            {
                assetRef.ItemDatabase.RemoveAt(index);
            }

            selectedIndex = -1;
            SetSelection(new int[0]);
            Reload();
        }

        public override void OnGUI(Rect rect)
        {
            Rect treeViewRect = rect;
            treeViewRect.width = 300;

            Rect invHeader = EditorUtils.DrawHeaderWithBorder("Inventory Database", 20, ref treeViewRect, true);

            var addItemButton = invHeader;
            addItemButton.width = EditorGUIUtility.singleLineHeight;
            addItemButton.x += invHeader.width - EditorGUIUtility.singleLineHeight - EditorGUIUtility.standardVerticalSpacing;
            addItemButton.y += EditorGUIUtility.standardVerticalSpacing;

            if (GUI.Button(addItemButton, EditorUtils.Styles.PlusIcon, EditorUtils.Styles.IconButton))
            {
                AddNewItem();
            }

            Rect itemViewRect = rect;
            itemViewRect.width -= 300 + 2f;
            itemViewRect.x = treeViewRect.width + 8f;

            var selected = GetSelection();
            if (selected.Count == 1)
            {
                selectedIndex = GetSelection()[0];

                Rect itemHeader = EditorUtils.DrawHeaderWithBorder("Item View", 20, ref itemViewRect, true);

                GUIContent idText = new GUIContent("ITEM ID: " + treeViewElements[selectedIndex].ItemID);
                Vector2 idTextSize = EditorStyles.miniBoldLabel.CalcSize(idText);

                var idRect = itemHeader;
                idRect.xMin = idRect.xMax - idTextSize.x - EditorGUIUtility.standardVerticalSpacing;
                idRect.y += EditorGUIUtility.standardVerticalSpacing;
                EditorGUI.LabelField(idRect, idText, EditorStyles.miniBoldLabel);

                OnSelectedGUI(itemViewRect);
            }
            else if(selected.Count > 1)
            {
                selectedIndex = -1;

                GUIContent text = new GUIContent("Multi-selected items cannot be edited.");
                Vector2 textSize = EditorStyles.miniBoldLabel.CalcSize(text);

                Rect multiSelectMsg = itemViewRect;
                multiSelectMsg.x += (itemViewRect.width / 2) - (textSize.x / 2);
                multiSelectMsg.y += 50f;

                EditorGUI.LabelField(multiSelectMsg, text, EditorStyles.miniBoldLabel);
            }

            if (m_InitiateContextMenuOnNextRepaint)
            {
                m_InitiateContextMenuOnNextRepaint = false;
                PopUpContextMenu();
            }

            base.OnGUI(treeViewRect);
        }

        private void DrawPropertyWithHeader(ref bool state, SerializedProperty property, string label)
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);
            if (state = EditorUtils.DrawFoldoutHeader(20f, label, state, true))
            {
                EditorGUILayout.Space(Spacing);
                EditorUtils.DrawRelativeProperties(property, 5f);
                EditorGUILayout.Space(EditorGUIUtility.standardVerticalSpacing);
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawLocalizationHeader(ref bool state, SerializedProperty property, string label)
        {
            var p_TitleKey = property.FindPropertyRelative("titleKey");
            var p_DescriptionKey = property.FindPropertyRelative("descriptionKey");

            EditorGUILayout.BeginVertical(GUI.skin.box);
            if (state = EditorUtils.DrawFoldoutHeader(20f, label, state, true))
            {
                EditorGUILayout.Space(Spacing);

#if !TW_LOCALIZATION_PRESENT
                EditorUtils.TrHelpIconText("<b>HFPS Localization System Integration</b> is required in order to translate Title and Description!", MessageType.Warning, true, false);
                EditorGUILayout.Space(EditorGUIUtility.standardVerticalSpacing);
#else
                if (localization.boolValue && localizationExist)
                {
                    EditorUtils.TrHelpIconText("<b>Title</b> and <b>Description</b> will change depending on the current localization.", MessageType.Warning, true, false);
                    EditorGUILayout.Space(EditorGUIUtility.standardVerticalSpacing);
                }
#endif

                using (new EditorGUI.DisabledGroupScope(!localization.boolValue || !localizationExist))
                {
                    Rect titleKeyRect = GUILayoutUtility.GetRect(1, 20);
                    Rect descKeyRect = GUILayoutUtility.GetRect(1, 20);
                    descKeyRect.y += EditorGUIUtility.standardVerticalSpacing * 2;

                    DrawLocalizationSelector(ref titleKeyRect, p_TitleKey);
                    DrawLocalizationSelector(ref descKeyRect, p_DescriptionKey);

                    EditorGUI.PropertyField(titleKeyRect, p_TitleKey);
                    EditorGUI.PropertyField(descKeyRect, p_DescriptionKey);
                }

                EditorGUILayout.Space(EditorGUIUtility.standardVerticalSpacing);
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawLocalizationSelector(ref Rect pos, SerializedProperty property)
        {
            pos.xMax -= EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            Rect selectRect = pos;
            selectRect.width = EditorGUIUtility.singleLineHeight;
            selectRect.x = pos.xMax + EditorGUIUtility.standardVerticalSpacing;
            selectRect.y += 0.6f;

            GUIContent Linked = EditorGUIUtility.TrIconContent("d_Linked", "Open Localization System Key Browser");
            GUIContent UnLinked = EditorGUIUtility.TrIconContent("d_Unlinked", "Localization System not found!");

            GUIContent icon = localizationExist ? Linked : UnLinked;

            using (new EditorGUI.DisabledGroupScope(!localization.boolValue && !localizationExist))
            {
                if (GUI.Button(selectRect, icon, EditorUtils.Styles.IconButton))
                {
#if TW_LOCALIZATION_PRESENT
                    EditorWindow browser = EditorWindow.GetWindow<LocalizationUtility.LocaleKeyBrowserWindow>(true, "Localization Key Browser", true);
                    browser.minSize = new Vector2(320, 500);
                    browser.maxSize = new Vector2(320, 500);

                    LocalizationUtility.LocaleKeyBrowserWindow keyBrowser = browser as LocalizationUtility.LocaleKeyBrowserWindow;
                    keyBrowser.OnSelectKey += key =>
                    {
                        property.stringValue = key;
                        property.serializedObject.ApplyModifiedProperties();
                    };

                    keyBrowser.Show(LocalizationSystem.Instance);
#endif
                }
            }
        }

        private void OnSelectedGUI(Rect rect)
        {
            Repaint();

            ItemTreeElement item = treeViewElements[selectedIndex];
            var p_Title = item.ItemProperty.FindPropertyRelative("Title");
            var p_Description = item.ItemProperty.FindPropertyRelative("Description");
            var p_ItemType = item.ItemProperty.FindPropertyRelative("itemType");
            var p_UseActionType = item.ItemProperty.FindPropertyRelative("useActionType");
            var p_ItemSprite = item.ItemProperty.FindPropertyRelative("itemSprite");
            var p_DropObject = item.ItemProperty.FindPropertyRelative("DropObject");
            var p_PackDropObject = item.ItemProperty.FindPropertyRelative("PackDropObject");

            var p_ItemToggles = item.ItemProperty.FindPropertyRelative("itemToggles");
            var p_ItemSounds = item.ItemProperty.FindPropertyRelative("itemSounds");
            var p_ItemSettings = item.ItemProperty.FindPropertyRelative("itemSettings");
            var p_UseActionSettings = item.ItemProperty.FindPropertyRelative("useActionSettings");
            var p_CombineSettings = item.ItemProperty.FindPropertyRelative("combineSettings");
            var p_LocalizationSettings = item.ItemProperty.FindPropertyRelative("localizationSettings");

            Rect itemViewArea = rect;
            itemViewArea.y += Spacing;
            itemViewArea.yMax -= Spacing;
            itemViewArea.xMin += Spacing;
            itemViewArea.xMax -= Spacing;

            GUILayout.BeginArea(itemViewArea);
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            // draw icon
            Rect iconRect = GUILayoutUtility.GetRect(100, 100);
            iconRect.xMax = 100;
            p_ItemSprite.objectReferenceValue = EditorGUI.ObjectField(iconRect, p_ItemSprite.objectReferenceValue, typeof(Sprite), false);

            // draw title field
            Rect titleRect = GUILayoutUtility.GetRect(1, 20);
            titleRect.y = 0f;
            titleRect.xMin = iconRect.xMax + Spacing;

            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(titleRect, p_Title, new GUIContent());
            if (EditorGUI.EndChangeCheck())
            {
                treeViewElements[selectedIndex].Title = p_Title.stringValue;
            }

            using (new EditorGUI.DisabledGroupScope(localization.boolValue))
            {
                // draw description field
                Rect descriptionRect = titleRect;
                descriptionRect.y = 20f + Spacing;
                descriptionRect.height = 80f - Spacing;
                EditorGUI.PropertyField(descriptionRect, p_Description, new GUIContent());
            }

            // draw properties
            EditorGUILayout.Space(-20 + EditorGUIUtility.standardVerticalSpacing);
            EditorGUILayout.PropertyField(p_ItemType);
            EditorGUILayout.PropertyField(p_UseActionType);
            EditorGUILayout.PropertyField(p_DropObject);
            EditorGUILayout.PropertyField(p_PackDropObject);
            EditorGUILayout.Space(5);

            // draw foldouts
            DrawPropertyWithHeader(ref item.ItemTogglesState, p_ItemToggles, "Item Toggles");
            DrawPropertyWithHeader(ref item.ItemSoundsState, p_ItemSounds, "Item Sounds");
            DrawPropertyWithHeader(ref item.ItemSettingsState, p_ItemSettings, "Item Settings");
            DrawPropertyWithHeader(ref item.UseActionSettingsState, p_UseActionSettings, "Use Action Settings");
            DrawPropertyWithHeader(ref item.CombineSettingsState, p_CombineSettings, "Combine Settings");

            DrawLocalizationHeader(ref item.LocalizationSettingsState, p_LocalizationSettings, "Localization Settings");

            EditorGUILayout.EndScrollView();
            GUILayout.EndArea();

            assetObject.ApplyModifiedProperties();
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            var item = (ItemTreeElement)args.item;

            var labelRect = args.rowRect;
            labelRect.x += 2f;

            GUIContent label = EditorGUIUtility.TrTextContentWithIcon($" [{item.ItemID}] {item.Title}", "PreMatCube");
            EditorGUI.LabelField(labelRect, label);
        }
    }
}