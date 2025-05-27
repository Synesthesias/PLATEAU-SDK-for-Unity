using PLATEAU.CityInfo;
using PLATEAU.Dataset;
using System;
using System.Text.RegularExpressions;
using UnityEngine;

namespace PLATEAU.DynamicTile
{
    [Serializable]
    public class PLATEAUDynamicTile
    {
        [SerializeField]
        private string address;
        
        [SerializeField]
        private Transform parent;
        
        [SerializeField]
        private bool isExcluded;

        // TODO: Extentの取得
        [SerializeField]
        private Bounds Extent;

        public PLATEAUDynamicTile(string address)
        {
            this.address = address;
        }
    }
} 