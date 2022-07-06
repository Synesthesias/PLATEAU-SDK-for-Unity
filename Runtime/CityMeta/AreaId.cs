using System;
using System.Collections.Generic;
using PLATEAU.CommonDataStructure;
using PLATEAU.Util;
using UnityEngine;
using UnityEngine.Animations;
using AreaIdTree = PLATEAU.CommonDataStructure.ClassificationTree<int, PLATEAU.CityMeta.AreaId>;

namespace PLATEAU.CityMeta
{

    // 地域ID は 6桁または8桁からなります。
    // 最初の6桁が 第2次地域区画 (1辺10km) です。
    // 残りの2桁がある場合、それが 第3次地域区画 (1辺1km) です。
    
    [Serializable]
    internal class AreaId : IComparable<AreaId>
    {
        [SerializeField] private int id;
        [SerializeField] private bool isTarget;

        private const int numDigitsOfSecondSection = 6;

        public AreaId(int id)
        {
            this.id = id;
        }
        
        /// <summary> 6桁または8桁の地域IDです。 </summary>
        public int Id { get; set; }
        
        /// <summary> この地域をインポート対象とするかどうかの設定です。 </summary>
        public bool IsTarget { get; set; }

        /// <summary> 第2地域区画 (1辺10km)の番号、すなわちIDの最初の6桁です。 </summary>
        public int SecondSectionId => DigitsUtil.PickFirstDigits(Id, numDigitsOfSecondSection);


        public int CompareTo(AreaId other)
        {
            return this.id.CompareTo(other.id);
        }

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
        /// <param name="areaIds">地域IDのリストを渡します。</param>
        public static AreaIdTree GenerateClassificationTree(IEnumerable<AreaId> areaIds)
        {
            var rootNode = new AreaIdTree(new AreaId(-1));
            foreach (var areaId in areaIds)
            {
                int secondKey = areaId.SecondSectionId;
                if (!rootNode.ContainsInChildren(secondKey))
                {
                    rootNode.AddChild(secondKey, new AreaId(secondKey));
                }

                var secondNode = rootNode.GetChild(secondKey);
                int thirdKey = areaId.id;
                if (!secondNode.ContainsInChildren(thirdKey))
                {
                    secondNode.AddChild(thirdKey, areaId);
                }
            }

            return rootNode;
        }
        
        
    }
}