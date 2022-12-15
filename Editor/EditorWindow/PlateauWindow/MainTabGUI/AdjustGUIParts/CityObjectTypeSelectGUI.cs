using System.Collections.Generic;
using System.Collections.ObjectModel;
using PLATEAU.CityInfo;
using UnityEditor;
using UnityEngine;
using Hierarchy = PLATEAU.CityInfo.CityObjectTypeHierarchy;

namespace PLATEAU.Editor.EditorWindow.PlateauWindow.MainTabGUI.AdjustGUIParts
{
    public class CityObjectTypeSelectGUI
    {
        private Dictionary<Hierarchy.Node, bool> selectionDict = new Dictionary<Hierarchy.Node, bool>();

        public ReadOnlyDictionary<Hierarchy.Node, bool> SelectionDict =>
            new ReadOnlyDictionary<CityObjectTypeHierarchy.Node, bool>(this.selectionDict);

        public void Draw()
        {
            // ルートノードを除いて描画します。
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
                EditorGUILayout.Toggle(node.NodeName, this.selectionDict[node]);

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
    }
}
