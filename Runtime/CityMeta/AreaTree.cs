using System;
using System.Collections.Generic;
using System.Linq;
using PLATEAU.CommonDataStructure;

namespace PLATEAU.CityMeta
{
    [Serializable]
    internal class AreaTree
    {
        private ClassificationTree<int, Area> rootNode;
        
        /// <summary>
        /// 地域IDをキーとする分類ツリーを生成します。
        /// <seealso cref="ClassificationTree{TKey,TValue}"/>
        /// 
        /// 次のようなツリーが生成されます:
        /// ・ルートは id=-1 のノードです。 
        /// ・ルートの直下に 第2次地域区画のノードが列挙されます。
        /// ・各 第2次地域区画 のノードの子に、自身に属する 第3次地域区画　のノードが列挙されます。
        /// すなわち、深さが 3 である地域分類ツリーが生成されます。
        /// </summary>
        /// <param name="areas">地域IDのリストを渡します。</param>
        public AreaTree(IEnumerable<Area> areas)
        {
            var root = new ClassificationTree<int ,Area>(new Area(-1));
            foreach (var area in areas)
            {
                int secondKey = area.SecondSectionId();
                // 木の深さ 2 の部分に追加します。
                if (!root.ContainsInChildren(secondKey))
                {
                    root.AddChild(secondKey, new Area(secondKey));
                }
                // 深さ 3 に相当するIDがなければ終了します。
                if (area.ThirdSectionId() < 0)
                {
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

        public IEnumerable<(int depth, Area area)> IterateDfs()
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
    }
}