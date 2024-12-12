using PLATEAU.Editor.Window.Common;
using PLATEAU.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace PLATEAU.Editor.Window.Main.Tab
{
    /// <summary>
    /// PLATEAUウィンドウの「道路調整」タブです。
    /// </summary>
    public class RoadAdjustGui : ITabContent
    {
        /// <summary> 子タブのUIと機能クラスを関連付ける辞書です。 </summary>
        private Dictionary<RadioButton, RoadGuiParts.RoadAdjustGuiPartBase> childTabsDict = new ();

        private TemplateContainer container;
        
        /// <summary>
        /// 「道路調整」タブおよびその子タブ「生成」「編集」「追加」を生成します。
        /// </summary>
        public VisualElement CreateGui()
        {
            container = LoadMainUxml();
            
            var main = container.Q<VisualElement>("RoadNetwork_Main");
            if (main == null)
            {
                Debug.LogError("Failed to find main element of road adjusting.");
            }
            
            CreateChildTabs(main);
            
            return container;
        }

        /// <summary>
        /// 「道路調整」のメインUXMLをロードします。
        /// </summary>
        private TemplateContainer LoadMainUxml()
        {
            var visualTree =
                AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                    $"Packages/{PathUtil.packageFormalName}/Resources/PlateauUIDocument/RoadNetwork/RoadNetwork_Main.uxml"
                );
            if (visualTree == null)
            {
                Debug.LogError("Failed to load gui.");
            }

            var loadedContainer = visualTree.CloneTree();
            return loadedContainer;
        }

        /// <summary>
        /// 子タブ「生成」「編集」「追加」を用意します。
        /// </summary>
        private void CreateChildTabs(VisualElement mainVE)
        {
            var menuGroup = mainVE.Q<VisualElement>("MenuGroup");
            childTabsDict = new()
            {
                {menuGroup.Q<RadioButton>("MenuGenerate"), new RoadGuiParts.RoadGeneratePanel(mainVE)},
                {menuGroup.Q<RadioButton>("MenuEdit"), new RoadGuiParts.RoadEditPanel(mainVE)},
                {menuGroup.Q<RadioButton>("MenuAdd"), new RoadGuiParts.RoadAddPanel(mainVE)},
                {menuGroup.Q<RadioButton>("MenuTrafficRule"), new RoadGuiParts.RoadTrafficRulePanel(mainVE)}
            };

            // 各子タブの選択時と選択解除時の処理を設定します。
            foreach (var (radioButton, panel) in childTabsDict)
            {
                panel.InitUXMLState(mainVE);
                radioButton.RegisterCallback<ChangeEvent<bool>>(e =>
                {
                    if (e.newValue)
                    {
                        panel.OnRoadChildTabSelected(mainVE);
                    }
                    else
                    {
                        panel.OnRoadChildTabUnselected(mainVE);
                    }
                });
            }

            // 初期表示のタブをアクティブにします。
            var initialActive = childTabsDict.First(kvp => kvp.Key.name == "MenuGenerate");
            initialActive.Value.OnRoadChildTabSelected(mainVE);
        }
        

        
        public void Dispose()
        {
        }

        public void OnTabUnselect()
        {
        }
        

    }
}