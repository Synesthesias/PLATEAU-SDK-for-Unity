using PLATEAU.CityAdjust.ConvertToAsset;
using PLATEAU.CityInfo;
using PLATEAU.Editor.Window.Common;
using PLATEAU.Editor.Window.Main.Tab.DynamicTileGUI;
using PLATEAU.Util;
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.IO;
using UnityEditor.UIElements;

namespace PLATEAU.Editor.Window.Main
{
    /// <summary>
    /// PLATEAUウィンドウの動的タイルタブ（DynamicTile）UI
    /// </summary>
    public class DynamicTileGui : ITabContent
    {
        private TemplateContainer container;
        private ConvertToAssetConfig assetConfig;
        private List<PLATEAUCityObjectGroup> excludeObjects;

        // UI要素
        private Button addObjectFieldButton, removeObjectFieldButton, selectFolderButton, execButton;
        private VisualElement objectFieldList, folderSelectRow;
        private Label folderPathLabel;
        private DropdownField saveLocationDropdown;
        
        private const string PrefabsSavePath = "Assets/PLATEAUPrefabs";
        private const string CustomFolderLabel = "任意のフォルダに保存";

        private bool isFolderShow;

        public VisualElement CreateGui()
        {
            container = LoadMainUxml();
            InitializeState();
            FindUIElements();
            RegisterEvents();
            return container;
        }

        private TemplateContainer LoadMainUxml()
        {
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                $"{PathUtil.SdkBasePath}/Resources/PlateauUIDocument/DynamicTile/DynamicTile.uxml");
            if (visualTree == null)
            {
                Debug.LogError("Failed to load gui.");
            }
            return visualTree.CloneTree();
        }

        private void InitializeState()
        {
            assetConfig = ConvertToAssetConfig.DefaultValue;
            assetConfig.AssetPath = PrefabsSavePath;
            excludeObjects = new List<PLATEAUCityObjectGroup>();
            isFolderShow = false;
        }

        private void FindUIElements()
        {
            addObjectFieldButton = container.Q<Button>("AddObjectFieldButton");
            removeObjectFieldButton = container.Q<Button>("RemoveObjectFieldButton");
            selectFolderButton = container.Q<Button>("SelectFolderButton");
            execButton = container.Q<Button>("ExecButton");
            objectFieldList = container.Q<VisualElement>("ObjectFieldList");
            folderSelectRow = container.Q<VisualElement>("FolderSelectRow");
            folderPathLabel = container.Q<Label>("FolderPathLabel");
            saveLocationDropdown = container.Q<DropdownField>("SaveLocationDropdown");
        }

        private void RegisterEvents()
        {
            // 除外ObjectFieldの追加・削除
            addObjectFieldButton.clicked += AddExcludeObjectField;
            removeObjectFieldButton.clicked += RemoveExcludeObjectField;

            // 最初のObjectFieldを追加
            AddExcludeObjectField();

            // フォルダ選択
            selectFolderButton.clicked += () =>
            {
                string path = EditorUtility.OpenFolderPanel("保存先フォルダを選択", assetConfig.AssetPath, "");
                if (!string.IsNullOrEmpty(path))
                {
                    folderPathLabel.text = path;
                }
            };

            // 保存先ドロップダウン
            saveLocationDropdown.RegisterValueChangedCallback(evt =>
            {
                isFolderShow = evt.newValue == CustomFolderLabel;
                folderPathLabel.text = "";
                folderSelectRow.style.display = isFolderShow ? DisplayStyle.Flex : DisplayStyle.None;
            });

            // 実行
            execButton.clicked += Export;
        }

        private void AddExcludeObjectField()
        {
            var objectField = new ObjectField
            {
                objectType = typeof(PLATEAUCityObjectGroup),
                allowSceneObjects = true,
                style = { marginTop = 4, marginRight = 10 }
            };

            objectField.userData = objectFieldList.childCount;
            objectField.RegisterValueChangedCallback(evt =>
            {
                var newValue = evt.newValue as PLATEAUCityObjectGroup;
                int currentIndex = (int)objectField.userData;

                if (newValue == null)
                {
                    if (excludeObjects.Count > currentIndex)
                    {
                        excludeObjects.RemoveAt(currentIndex);
                    }
                    return;
                }

                if (excludeObjects.Count > currentIndex)
                {
                    excludeObjects[currentIndex] = newValue;
                }
                else
                {
                    excludeObjects.Add(evt.newValue as PLATEAUCityObjectGroup);
                }
            });
            objectFieldList.Add(objectField);

            // 削除ボタン表示制御
            removeObjectFieldButton.style.display = objectFieldList.childCount > 1 ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void RemoveExcludeObjectField()
        {
            if (objectFieldList.childCount > 0)
            {
                objectFieldList.RemoveAt(objectFieldList.childCount - 1);
                if (excludeObjects.Count > 0)
                {
                    excludeObjects.RemoveAt(excludeObjects.Count - 1);
                }
            }
            removeObjectFieldButton.style.display = objectFieldList.childCount > 1 ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void Export()
        {
            if (isFolderShow && string.IsNullOrEmpty(folderPathLabel.text))
            {
                Dialogue.Display("保存先フォルダを指定してください。", "OK");
                return;
            }
            DynamicTileExporter.Export(
                assetConfig,
                folderPathLabel.text, 
                msg => Dialogue.Display(msg, "OK"));
        }

        public void OnTabUnselect() { }
        public void Dispose()
        {
            InitializeState();
        }
    }
}