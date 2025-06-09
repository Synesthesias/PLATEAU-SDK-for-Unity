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
        public string Address => address;
        
        [SerializeField]
        private Transform parent;
        public Transform Parent => parent;
        
        [SerializeField]
        private bool isExcluded;
        public bool IsExcluded => isExcluded;

        // TODO: Extentの取得
        [SerializeField]
        private Bounds extent;
        public Bounds Extent => extent;

        public PLATEAUDynamicTile(string address, Transform parent)
        {
            this.address = address;
            this.parent = parent;
        }
    }
} 