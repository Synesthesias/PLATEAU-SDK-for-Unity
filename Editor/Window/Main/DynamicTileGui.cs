using PLATEAU.CityInfo;
using PLATEAU.Editor.Window.Common;
using PLATEAU.Util;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace PLATEAU.Editor.Window.Main
{
    /// <summary>
    /// PLATEAUウィンドウの動的タイルタブです。
    /// </summary>
    public class DynamicTileGui : ITabContent
    {
        private TemplateContainer container;

        public VisualElement CreateGui()
        {
            container = LoadMainUxml();
            RegisterEvents();
            return container;
        }

        public void OnTabUnselect()
        {
        }
        
        public void Dispose()
        {
        }
        
        private TemplateContainer LoadMainUxml()
        {
            var visualTree =
                AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                    $"{PathUtil.SdkBasePath}/Resources/PlateauUIDocument/DynamicTile/DynamicTile.uxml"
                );
            if (visualTree == null)
            {
                Debug.LogError("Failed to load gui.");
            }

            var loadedContainer = visualTree.CloneTree();
            return loadedContainer;
        }

        private void RegisterEvents()
        {
            var addObjectFieldButton = container.Q<Button>("AddObjectFieldButton");
            var objectFieldList = container.Q<VisualElement>("ObjectFieldList");
            var removeObjectFieldButton = container.Q<Button>("RemoveObjectFieldButton");
            var selectFolderButton = container.Q<Button>("SelectFolderButton");
            var folderPathLabel = container.Q<Label>("FolderPathLabel");
            var saveLocationDropdown = container.Q<DropdownField>("SaveLocationDropdown");
            var folderSelectRow = container.Q<VisualElement>("FolderSelectRow");
            var execButton = container.Q<Button>("ExecButton");

            // 最初のObjectFieldを追加
            AddObjectField(objectFieldList, removeObjectFieldButton);

            addObjectFieldButton.clicked += () =>
            {
                AddObjectField(objectFieldList, removeObjectFieldButton);
            };

            removeObjectFieldButton.clicked += () =>
            {
                if (objectFieldList.childCount > 0)
                {
                    objectFieldList.RemoveAt(objectFieldList.childCount - 1);
                }

                // 1つ以下なら削除ボタン非表示
                if (objectFieldList.childCount <= 1)
                {
                    removeObjectFieldButton.style.display = DisplayStyle.None;
                }
            };

            selectFolderButton.clicked += () =>
            {
                string path = EditorUtility.OpenFolderPanel("保存先フォルダを選択", "", "");
                if (!string.IsNullOrEmpty(path))
                {
                    folderPathLabel.text = path;
                }
            };

            saveLocationDropdown.RegisterValueChangedCallback(evt =>
            {
                bool show = evt.newValue == "任意のフォルダに保存";
                folderSelectRow.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
            });

            execButton.clicked += () =>
            {
                Debug.Log("実行ボタンが押されました");
            };
        }
        
        private void AddObjectField(VisualElement objectFieldList, Button removeObjectFieldButton)
        {
            var objectField = new ObjectField
            {
                objectType = typeof(PLATEAUCityObjectGroup),
                allowSceneObjects = true,
                style = { marginTop = 4, marginRight = 10 }
            };
            objectFieldList.Add(objectField);

            // 2つ以上になったら削除ボタン表示
            if (objectFieldList.childCount > 1)
            {
                removeObjectFieldButton.style.display = DisplayStyle.Flex;
            }
        }
    }
}