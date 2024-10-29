using PLATEAU.Editor.Window.Common;
using PLATEAU.Util;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace PLATEAU.Editor.Window.Main.Tab
{
    public class RoadAdjustGui : ITabContent
    {
        
        public VisualElement CreateGui()
        {
            var visualTree =
                AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                    $"Packages/{PathUtil.packageFormalName}/Resources/PlateauUIDocument/RoadNetwork/RoadNetwork_Main.uxml"
                );
            if (visualTree == null)
            {
                Debug.LogError("Failed to load gui.");
            }

            return visualTree.CloneTree();
        }
        
        
        public void Dispose()
        {
            
        }

        public void OnTabUnselect()
        {
        }
    }
}