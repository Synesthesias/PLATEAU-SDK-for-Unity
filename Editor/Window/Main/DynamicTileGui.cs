using PLATEAU.Editor.Window.Common;
using PLATEAU.Util;
using UnityEditor;
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

            var main = container.Q<VisualElement>("RoadNetwork_Main");
            if (main == null)
            {
                Debug.LogError("Failed to find main element of road adjusting.");
            }
            

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
    }
}