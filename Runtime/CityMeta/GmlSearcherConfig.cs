using System;
using System.Collections.Generic;
using System.Linq;
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
        /// <summary> 見つかったエリアIDの一覧です。 </summary>
        // [SerializeField] private int[] areaIds = { };
        
        /// <summary> <see cref="areaIds"/> の i番目を変換対象とするかどうかです。 </summary>
        // [SerializeField] private bool[] isAreaIdTarget = { };
        
        private AreaTree areaIdTree;

        /// <summary> 地物タイプの絞り込み情報です。 </summary>
        [SerializeField] private GmlTypeTarget gmlTypeTarget = new GmlTypeTarget();

        public AreaTree AreaTree => this.areaIdTree;
        /// <summary>
        /// 引数が true のとき、すべてのエリアを対象とします。
        /// 　　　 false のとき、すべてのエリアを除外します。
        /// </summary>
        internal void SetAllAreaId(bool isTarget)
        {
            var areas = IterateAreaTree();
            foreach (var tuple in areas)
            {
                tuple.area.IsTarget = isTarget;
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
            var areas = IterateAreaTree();
            foreach (var tuple in areas)
            {
                var area = tuple.area;
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

        // public IReadOnlyList<int> AreaIds => this.areaIds;
        
        // public void SetAreaIds(int[] areaIdsArg)
        // {
        //     GenerateAreaTree(areaIdsArg);
        // }

        // public IReadOnlyList<bool> IsAreaIdTarget => this.isAreaIdTarget;

        public void SetIsAreaIdTarget(int areaId, bool isTarget)
        {
            this.areaIdTree.SetAreaIdTarget(areaId, isTarget);
        }

        // public void ResetIsAreaIdTarget(int areaCount)
        // {
        //     this.isAreaIdTarget = Enumerable.Repeat(true, areaCount ).ToArray();
        // }
        
        public void GenerateAreaTree(int[] areaIdArray)
        {
            List<Area> areas = new List<Area>();
            foreach (int areaId in areaIdArray)
            {
                areas.Add(new Area(areaId));
            }
            this.areaIdTree = new AreaTree(areas);
        }

        public IEnumerable<(int depth, Area area)> IterateAreaTree()
        {
            
            var areasIter = this.areaIdTree.IterateDfs();
            foreach (var iter in areasIter)
            {
                yield return iter;
            }
        }
    }
}