﻿using PLATEAU.CityGML;
using PLATEAU.RoadNetwork.CityObject;
using PLATEAU.RoadNetwork.Factory;
using PLATEAU.RoadNetwork.Graph;
using PLATEAU.RoadNetwork.Structure.Drawer;
using PLATEAU.RoadNetwork.Util;
using PLATEAU.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using PLATEAUCityObjectGroup = PLATEAU.CityInfo.PLATEAUCityObjectGroup;

namespace PLATEAU.RoadNetwork.Tester
{
    [Serializable]
    [RequireComponent(typeof(PLATEAURnModelDrawerDebug))]
    public class PLATEAURoadNetworkTester : MonoBehaviour
    {
        // --------------------
        // start:フィールド
        // --------------------
        [field: SerializeField] public RoadNetworkFactory Factory { get; set; } = new RoadNetworkFactory();

        // 道路ネットワーク作成用のPLATEAUCityObjectGroupのプリセットテーブル
        [SerializeField]
        public List<TestTargetPresets> savedTargets = new List<TestTargetPresets>();

        // シーンに配置している全てのPLATEAUCityObjectGroupを対象にするか
        [SerializeField] private bool targetAll = false;

        // 今回作成するPLATEAUCityObjectGroupのプリセットテーブル
        public string targetPresetName = "";

        // --------------------
        // end:フィールド
        // --------------------

        [Serializable]
        public class TestTargetPresets
        {
            public string name;
            public List<PLATEAUCityObjectGroup> targets = new List<PLATEAUCityObjectGroup>();
        }

        public List<PLATEAUCityObjectGroup> GetTargetCityObjects()
        {
            var ret = targetAll
                ? (IList<PLATEAUCityObjectGroup>)GameObject.FindObjectsOfType<PLATEAUCityObjectGroup>()
                : savedTargets
                    .FirstOrDefault(s => s.name == targetPresetName)
                    ?.targets;
            if (ret == null)
                return new List<PLATEAUCityObjectGroup>();

            return ret
                .Where(c => c.transform.childCount == 0)
                .Where(c => c.CityObjects.rootCityObjects.Any(a => a.CityObjectType == CityObjectType.COT_Road))
                .Distinct()
                .ToList();
        }

        /// <summary>
        /// 道路ネットワークを作成する
        /// </summary>
        /// <returns></returns>
        public async Task CreateNetwork()
        {
            try
            {
                await Factory.CreateRnModelAsync(GetTargetCityObjects(), gameObject);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        /// <summary>
        /// 同名のCityObjectGroupがあった場合に最大のLODのもの以外を非表示にする
        /// </summary>
        public void RemoveSameNameCityObjectGroup()
        {
            var groups = GameObject.FindObjectsOfType<PLATEAUCityObjectGroup>();

            foreach (var g in groups.GroupBy(g => g.gameObject.name)
                         .Where(g => g.Count() > 1))
            {
                var level = g.Select(a => a.GetLodLevel()).Max();
                g.TryFindMax(a => a.GetLodLevel(), out var maxG);

                foreach (var a in g)
                {
                    if (a != maxG)
                    {
                        a.gameObject.SetActive(false);
                    }
                }
            }
        }
    }
}