using System;
using System.Collections.Generic;
using PLATEAU.CommonDataStructure;
using UnityEngine;

namespace PLATEAU.CityMeta
{

    /// <summary>
    /// GmlSearcher の設定です。
    /// 対象gmlファイル選択において、地域IDと地物タイプの絞り込み設定を保持します。
    /// </summary>
    
    // 注意事項:
    // このクラスは CityImporterConfig によって保持されるので、そちらの注意事項もご覧ください。
    
    [Serializable]
    internal class GmlSearcherConfig
    {

        [SerializeField] private AreaTree areaTree;

        /// <summary> 地物タイプの絞り込み情報です。 </summary>
        [SerializeField] private GmlTypeTarget gmlTypeTarget = new GmlTypeTarget();

        public AreaTree AreaTree => this.areaTree;
        /// <summary>
        /// 引数が true のとき、すべてのエリアを対象とします。
        /// 　　　 false のとき、すべてのエリアを除外します。
        /// </summary>
        internal void SetAllAreaId(bool isTarget)
        {
            var nodes = IterateAreaTree();
            if (nodes == null)
            {
                Debug.LogError($"{nameof(nodes)} is null. Call method {nameof(GenerateAreaTree)} or GmlSearcherPresenter.OnImportSrcPathChanged  before reading AreaId.");
            }
            foreach (var tuple in nodes)
            {
                tuple.node.Value.IsTarget = isTarget;
            }
        }

        /// <summary> 地物タイプを変換対象とするかについて、すべて true または すべて false にします。 </summary>
        public void SetAllTypeTarget(bool val)
        {
            this.gmlTypeTarget.SetAllTarget(val);
        }

        /// <summary>
        /// ターゲットとなる AreaId をリストで返します。
        /// </summary>
        public List<int> GetTargetAreaIds()
        {
            var targetIds = new List<int>();
            var nodes = IterateAreaTree();
            foreach (var tuple in nodes)
            {
                var area = tuple.node.Value;
                if (area.IsTarget)
                {  
                    targetIds.Add(area.Id);
                }
            }

            return targetIds;
        }

        public bool GetIsTypeTarget(GmlType t)
        {
            return this.gmlTypeTarget.IsTypeTarget(t);
        }

        public GmlType[] AllGmlTypes()
        {
            return this.gmlTypeTarget.Keys;
        }

        public void SetIsTypeTarget(GmlType t, bool isTarget)
        {
            this.gmlTypeTarget.SetIsTypeTarget(t, isTarget);
        }

        public void SetIsAreaIdTarget(int areaId, bool isTarget)
        {
            this.areaTree.SetAreaIdTarget(areaId, isTarget);
        }

        public void GenerateAreaTree(int[] areaIdArray, bool ignoreIfTreeExists)
        {
            if (ignoreIfTreeExists && this.areaTree != null) return;
            List<Area> areas = new List<Area>();
            foreach (int areaId in areaIdArray)
            {
                areas.Add(new Area(areaId));
            }
            this.areaTree = new AreaTree();
            this.areaTree.Generate(areas);
        }

        public IEnumerable<(int depth, ClassificationTree<int, Area> node)> IterateAreaTree()
        {
            
            var areasIter = this.areaTree.IterateDfs();
            foreach (var iter in areasIter)
            {
                yield return iter;
            }
        }
    }
}