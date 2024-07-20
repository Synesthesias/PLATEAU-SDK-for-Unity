using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PLATEAU.RoadNetwork
{
    /// <summary>
    /// 実際にデータ化されるものではない
    /// Node -> Nodeを繋ぐ複数のLinkをまとめるクラス
    /// </summary>
    [Serializable]
    public class RnLinkGroup
    {
        //----------------------------------
        // start: フィールド
        //----------------------------------
        /// <summary>
        /// 開始ノード
        /// </summary>
        [field: SerializeField]
        public RnNode PrevNode { get; private set; }

        /// <summary>
        /// 終了ノード
        /// </summary>
        [field: SerializeField]
        public RnNode NextNode { get; private set; }

        [SerializeField]
        private List<RnLink> links;

        //----------------------------------
        // end: フィールド
        //----------------------------------

        public IReadOnlyList<RnLink> Links => links;

        public RnLinkGroup(RnNode prevNode, RnNode nextNode, IEnumerable<RnLink> links)
        {
            PrevNode = prevNode;
            NextNode = nextNode;
            this.links = links.ToList();
        }

    }
}