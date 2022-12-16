using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using PLATEAU.CityInfo;
using PLATEAU.Dataset;
using PLATEAU.Editor.EditorWindow.Common;
using UnityEditor;
using Hierarchy = PLATEAU.CityInfo.CityObjectTypeHierarchy;

namespace PLATEAU.Editor.EditorWindow.PlateauWindow.MainTabGUI.AdjustGUIParts
{
    internal class AdjustCityObjectTypeGUI
    {
        private readonly Dictionary<Hierarchy.Node, bool> selectionDict = new Dictionary<Hierarchy.Node, bool>();

        public ReadOnlyDictionary<Hierarchy.Node, bool> SelectionDict =>
            new ReadOnlyDictionary<CityObjectTypeHierarchy.Node, bool>(this.selectionDict);


        public void Draw(CityAdjustGUI.PackageToLodMinMax packageToLodMinMax)
        {

            using (new EditorGUILayout.HorizontalScope())
            {
                if(PlateauEditorStyle.MiniButton("全選択", 100))
                {
                    SetSelectionAll(true);
                }

                if (PlateauEditorStyle.MiniButton("全選択解除", 100))
                {
                    SetSelectionAll(false);
                }
            }
            // ルートノードを除いて、ノードごとのトグルを描画します。
            var rootNode = Hierarchy.RootNode;
            foreach (var node in rootNode.Children)
            {
                DrawNodeRecursive(node, packageToLodMinMax);
            }
        }

        private void DrawNodeRecursive(Hierarchy.Node node, CityAdjustGUI.PackageToLodMinMax packageToLodMinMax)
        {
            if (!this.selectionDict.ContainsKey(node))
            {
                this.selectionDict.Add(node, true);
            }

            var package = node.UpperPackage;
            bool isPackageExistInScene =
                packageToLodMinMax.Packages.Contains(package) || package == PredefinedCityModelPackage.None;

            // シーン上にないパッケージ種であれば、GUIへの描画をスキップして自動的に false が選択されたものとみなします。
            // シーンに存在するパッケージ種であれば、トグルGUIを表示します。
            this.selectionDict[node] = isPackageExistInScene && EditorGUILayout.ToggleLeft(node.NodeName, this.selectionDict[node]);

            if (this.selectionDict[node])
            {
                EditorGUI.indentLevel++;
                foreach (var child in node.Children)
                {
                    DrawNodeRecursive(child, packageToLodMinMax);
                }

                EditorGUI.indentLevel--;
            }
            
        }

        private void SetSelectionAll(bool isActive)
        {
            foreach (var node in this.selectionDict.Keys.ToArray()) this.selectionDict[node] = isActive;
        }
    }
}
