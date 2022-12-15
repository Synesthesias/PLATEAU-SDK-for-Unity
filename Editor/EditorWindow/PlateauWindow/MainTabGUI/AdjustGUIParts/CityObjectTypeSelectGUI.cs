using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using PLATEAU.CityInfo;
using PLATEAU.Editor.EditorWindow.Common;
using UnityEditor;
using Hierarchy = PLATEAU.CityInfo.CityObjectTypeHierarchy;

namespace PLATEAU.Editor.EditorWindow.PlateauWindow.MainTabGUI.AdjustGUIParts
{
    public class CityObjectTypeSelectGUI
    {
        private readonly Dictionary<Hierarchy.Node, bool> selectionDict = new Dictionary<Hierarchy.Node, bool>();

        public ReadOnlyDictionary<Hierarchy.Node, bool> SelectionDict =>
            new ReadOnlyDictionary<CityObjectTypeHierarchy.Node, bool>(this.selectionDict);

        public void Draw()
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
                DrawNodeRecursive(node);
            }
        }

        private void DrawNodeRecursive(Hierarchy.Node node)
        {
            if (!this.selectionDict.ContainsKey(node))
            {
                this.selectionDict.Add(node, true);
            }

            this.selectionDict[node] =
                EditorGUILayout.ToggleLeft(node.NodeName, this.selectionDict[node]);

            if (this.selectionDict[node])
            {
                EditorGUI.indentLevel++;
                foreach (var child in node.Children)
                {
                    DrawNodeRecursive(child);
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
