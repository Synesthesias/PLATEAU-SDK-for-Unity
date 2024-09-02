using PLATEAU.Native;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PLATEAU.TerrainConvert
{
    public class PLATEAUSmoothedDem : MonoBehaviour
    {
        [SerializeField, HideInInspector]
        private ConvertedTerrainData.HeightmapData heightMapData;

        internal ConvertedTerrainData.HeightmapData HeightMapData
        {
            get
            {
                return heightMapData;
            }
            set
            {
                heightMapData = value;
            }
        }

        //[field: SerializeField]
        //[field: HideInInspector]
        //public int TextureWidth { get; set; }

        //[field: SerializeField]
        //[field: HideInInspector]
        //public int TextureHeight { get; set; }

        //[field: SerializeField]
        //[field: HideInInspector]
        //public float TerrainHeight { get; set; }

        //[field: SerializeField]
        //[field: HideInInspector]
        //public ushort[] HeightData { get; set; }

        //[field: SerializeField]
        //[field: HideInInspector]
        //public PlateauVector3d Min { get; set; }

        //[field: SerializeField]
        //[field: HideInInspector]
        //public PlateauVector3d Max { get; set; }

        //[field: SerializeField]
        //[field: HideInInspector]
        //public PlateauVector2f MinUV { get; set; }

        //[field: SerializeField]
        //[field: HideInInspector]
        //public PlateauVector2f MaxUV { get; set; }
    }
}
