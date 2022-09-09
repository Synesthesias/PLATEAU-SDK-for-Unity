using PLATEAU.IO;
using UnityEngine;

namespace PLATEAU.CityLoader.Setting
{
    /// <summary>
    /// <see cref="PLATEAUCityModelLoader"/> の設定のうち、地物タイプごとの設定です。
    /// </summary>
    [SerializeField]
    public struct FeatureLoadSetting
    {
        public bool IncludeTexture;
        public MeshGranularity MeshGranularity;
        public int MinLOD;
        public int MaxLOD;
    }
}
