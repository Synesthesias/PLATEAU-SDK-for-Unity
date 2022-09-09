#if UNITY_EDITOR
#endif
using System.Collections.Generic;
using System.Threading.Tasks;
using PLATEAU.CityGML;
using PLATEAU.Interop;
using PLATEAU.IO;
using PLATEAU.PolygonMesh;
using PLATEAU.Util;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Texture = UnityEngine.Texture;

namespace PLATEAU.CityLoader.Load
{
    /// <summary>
    /// 都市を指定数のグリッドに分割し、各グリッド内のメッシュを結合し、シーンに配置します。
    /// インスペクタでの表示については CityLoaderBehaviourEditor を参照してください。
    /// </summary>
    internal class CityLoaderBehaviour : MonoBehaviour
    {
        [SerializeField] private string gmlRelativePathFromStreamingAssets;
        [SerializeField] private MeshGranularity meshGranularity = MeshGranularity.PerCityModelArea;
        [SerializeField, Tooltip("グリッド分けした時の一辺のグリッド数です。グリッドの数はこの数値の2乗になります。")] private int gridCountOfSide = 10;
        [SerializeField] private bool doExportAppearance = true;
        [SerializeField] private uint minLOD = 2;
        [SerializeField] private uint maxLOD = 2;
        [SerializeField, Tooltip("範囲指定（緯度）です。")] private double minLatitude = -90;
        [SerializeField] private double maxLatitude = 90;
        [SerializeField, Tooltip("範囲指定（経度）です。")] private double minLongitude = -180;
        [SerializeField] private double maxLongitude = 180;

        public Task LoadAsync()
        {
            return CityLoader.Load(
                this.gmlRelativePathFromStreamingAssets,
                this.meshGranularity,
                this.minLOD, this.maxLOD,
                this.doExportAppearance, this.gridCountOfSide,
                this.minLatitude, this.minLongitude, this.maxLatitude, this.maxLongitude);
        }
    }
}