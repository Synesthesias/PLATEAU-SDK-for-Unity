using PLATEAU.CityExport;
using PLATEAU.CityExport.Exporters;
using PLATEAU.CityInfo;
using PLATEAU.Geometries;
using UnityEngine;

namespace PlateauSDKDevSample.Scripts
{
    /// <summary>
    /// ランタイムでエクスポートするサンプルです。
    /// </summary>
    public class RuntimeExportSample : MonoBehaviour
    {
        [SerializeField] private PLATEAUInstancedCityModel target;
        private void Start()
        {
            // 実行するには、ターゲットのstatic batchingをオフにする必要があります。staticをインスペクタから外します。
            
            // このパスをあなたのマシンに合わせたパスに変更してください。
            string destDir = "F:\\Desktop\\exportTest\\exportTest";
            var option = new MeshExportOptions(MeshExportOptions.MeshTransformType.Local, true, false,
                MeshFileFormat.FBX, CoordinateSystem.ENU, new CityExporterFbx());
            UnityModelExporter.Export(destDir, target, option);
        }
    }
}