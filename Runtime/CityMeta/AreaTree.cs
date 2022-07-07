using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using PLATEAU.CommonDataStructure;
using UnityEngine;

namespace PLATEAU.CityMeta
{
    /// <summary>
    /// 地域IDをキーとする分類ツリーです。
    /// <seealso cref="ClassificationTree{TKey,TValue}"/>
    /// 
    /// 次のようなツリーが生成されます:
    /// ・ルートは id=-1 のノードです。 
    /// ・ルートの直下に 第2次地域区画のノードが列挙されます。
    /// ・各 第2次地域区画 のノードの子に、自身に属する 第3次地域区画　のノードが列挙されます。
    /// すなわち、深さが 3 である地域分類ツリーが生成されます。
    /// </summary>
    [Serializable]
    internal class AreaTree : ISerializationCallbackReceiver
    {
        private ClassificationTree<int, Area> rootNode;
        
        // シリアライズする時に List型に変換されて保存されます。
        // デシリアライズする時にここから復元します。
        [SerializeField] private List<Area> serializedAreas;
        private const int rootAreaId = -1;
        

        /// <summary>
        /// <see cref="Area"/> のリストから木を生成します。
        /// </summary>
        public void Generate(IEnumerable<Area> areas)
        {
            var root = new ClassificationTree<int ,Area>(new Area(rootAreaId), null);
            foreach (var area in areas)
            {
                int secondKey = area.SecondSectionId();
                // 木の深さ 2 の部分にノードを追加します。
                if (!root.ContainsInChildren(secondKey))
                {
                    root.AddChild(secondKey, new Area(secondKey));
                }
                // 深さ 3 に相当するID部分がなければ、深さ 2 部分のみ更新します。
                if (area.ThirdSectionId() < 0)
                {
                    root.GetChild(secondKey).Value.IsTarget = area.IsTarget;
                    continue;
                }
                // 深さ 3 の部分にノードを追加します。
                var secondNode = root.GetChild(secondKey);
                int thirdKey = area.Id;
                if (!secondNode.ContainsInChildren(thirdKey))
                {
                    secondNode.AddChild(thirdKey, area);
                }
            }

            this.rootNode = root;
        }

        public IEnumerable<(int depth, ClassificationTree<int, Area> node)> IterateDfs()
        {
            var iter = this.rootNode.IterateDfsWithDepth();
            // 木のルートノードは便宜上 id=-1 としただけの無意味な値なのでスキップします。
            return iter.Skip(1);
        }

        public void SetAreaIdTarget(int areaId, bool isTarget)
        {
            int secondId = Area.SecondSectionId(areaId);
            var secondNode = this.rootNode.GetChild(secondId);
            secondNode.Value.IsTarget = isTarget;
            var thirdNode = secondNode.GetChild(areaId);
            thirdNode.Value.IsTarget = isTarget;
        }

        /// <summary>
        /// 親となる地域ノードが存在するかどうかをboolで返します。
        /// </summary>
        public static bool IsTopLevelArea(ClassificationTree<int, Area> node)
        {
            // 木のルートノードは、便宜上 id=-1 が設定されただけの仮ノードです。
            // 親が仮ノードであれば、地域としてはトップレベルであるとみなします。
            var parentArea = node.Parent?.Value;
            return parentArea == null || parentArea.Id == rootAreaId;
        }

        /// <summary>
        /// 指定のノードと、その子のノードの IsTarget を再帰的に設定します。
        /// </summary>
        public static void SetIsTargetRecursive(ClassificationTree<int, Area> node, bool isTarget)
        {
            var enumerable = ClassificationTree<int, Area>.IterateDfsRecursive(node, 0);
            foreach (var tuple in enumerable)
            {
                var area = tuple.node.Value;
                area.IsTarget = isTarget;
            }
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            this.serializedAreas = new List<Area>();
            foreach (var tuple in this.rootNode.IterateDfsWithDepth())
            {
                var area = tuple.node.Value;
                this.serializedAreas.Add(area);
            }
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            Generate(this.serializedAreas.Where(area => area.Id >= 0));
        }
    }
}