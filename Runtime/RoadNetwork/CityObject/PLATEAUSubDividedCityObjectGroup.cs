using System.Collections.Generic;
using UnityEngine;

namespace PLATEAU.RoadNetwork.CityObject
{
    public class PLATEAUSubDividedCityObjectGroup : MonoBehaviour
    {
        // --------------------
        // start:フィールド
        // --------------------
        // 保存された中間データ
        [SerializeField]
        private List<SubDividedCityObject> cityObjects = new List<SubDividedCityObject>();

        // --------------------
        // end:フィールド
        // --------------------

        /// <summary>
        /// 構成SubDividedCityObjectリスト
        /// </summary>
        public List<SubDividedCityObject> CityObjects
        {
            get => cityObjects;
            set => cityObjects = value;
        }
    }
}