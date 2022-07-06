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
        [SerializeField] private int[] areaIds = { };
        
        /// <summary> <see cref="areaIds"/> の i番目を変換対象とするかどうかです。 </summary>
        [SerializeField] private bool[] isAreaIdTarget = { };

        /// <summary> 地物タイプの絞り込み情報です。 </summary>
        [SerializeField] private GmlTypeTarget gmlTypeTarget = new GmlTypeTarget();
        
        /// <summary>
        /// 引数が true のとき、すべてのエリアを対象とします。
        /// 　　　 false のとき、すべてのエリアを除外します。
        /// </summary>
        internal void SetAllAreaId(bool isTarget)
        {
            for (int i = 0; i < this.isAreaIdTarget.Length; i++)
            {
                this.isAreaIdTarget[i] = isTarget;
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
        public List<int> TargetAreaIds()
        {
            var targetIds = new List<int>();
            for (int i = 0; i < this.areaIds.Length; i++)
            {
                if (this.isAreaIdTarget[i])
                {
                    targetIds.Add(this.areaIds[i]);
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

        public IReadOnlyList<int> AreaIds => this.areaIds;
        
        public void SetAreaIds(int[] areaIdsArg)
        {
            this.areaIds = areaIdsArg;
        }

        public IReadOnlyList<bool> IsAreaIdTarget => this.isAreaIdTarget;

        public void SetIsAreaIdTarget(int index, bool isTarget)
        {
            this.isAreaIdTarget[index] = isTarget;
        }

        public void ResetIsAreaIdTarget(int areaCount)
        {
            this.isAreaIdTarget = Enumerable.Repeat(true, areaCount ).ToArray();
        }
    }
}