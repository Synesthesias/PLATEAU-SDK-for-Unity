using PLATEAU.CityExport;
using PLATEAU.CityExport.Exporters;
using PLATEAU.CityImport.AreaSelector;
using PLATEAU.CityImport.Config;
using PLATEAU.CityImport.Import;
using PLATEAU.CityInfo;
using PLATEAU.Dataset;
using PLATEAU.Geometries;
using PLATEAU.GranularityConvert;
using PLATEAU.PolygonMesh;
using UnityEngine;
using UnityEngine.UI;

namespace PLATEAU.Samples.APISample
{
    public class RuntimeAPISample : MonoBehaviour
    {
        [SerializeField] private InputField importPathInputField;
        [SerializeField] private InputField exportPathInputField;
    
        /// <summary>
        /// ランタイムでインポートするコードです。
        /// </summary>
        public async void Import()
        {
            // インポートのパスをUnityのテキストフィールドから取得します。
            string importPath = importPathInputField.text;
            // PLATEAUのデータセットの場所をセットします。
            var datasetConf = new DatasetSourceConfigLocal(importPath);
            // インポート対象のメッシュコード（範囲）を指定します。文字列形式の番号の配列からMeshCodeListを生成できます。
            var meshCodesStr = new string[] { "53393652" };
            var meshCodeList = MeshCodeList.CreateFromMeshCodesStr(meshCodesStr);
            // データセットやメッシュコードから、インポート設定を生成します。
            var conf = CityImportConfig.CreateWithAreaSelectResult(
                new AreaSelectResult(new ConfigBeforeAreaSelect(datasetConf, 9), meshCodeList));
            // ここでconfを編集することも可能ですが、このサンプルではデフォルトでインポートします。
            await CityImporter.ImportAsync(conf, null, null);
        }

        /// <summary>
        /// ラインタイムでエクスポートするコードです。
        /// </summary>
        public void Export()
        {
            // 前提条件：
            // 実行するには、ターゲットのstatic batchingをオフにする必要があります。
            // そのためにはstaticのチェックをインスペクタから外します。
        
            // エクスポート先をUnityのテキストフィールドから取得します。
            string exportDir = exportPathInputField.text;
            // エクスポート設定です。
            var option = new MeshExportOptions(MeshExportOptions.MeshTransformType.Local, true, false,
                MeshFileFormat.FBX, CoordinateSystem.ENU, new CityExporterFbx());
            // 都市モデルを取得します。
            var target = FindObjectOfType<PLATEAUInstancedCityModel>();
            if (target == null)
            {
                Debug.LogError("都市モデルが見つかりませんでした。");
                return;
            }
            // エクスポートします。
            UnityModelExporter.Export(exportDir, target, option);
        }

        /// <summary>
        /// ランタイムで結合分割（ゲームオブジェクトの粒度の変更）をするコードです。
        /// </summary>
        public async void GranularityConvert()
        {
            // 都市オブジェクトを取得します。
            var target = FindObjectOfType<PLATEAUInstancedCityModel>();
            if (target == null)
            {
                Debug.LogError("都市モデルが見つかりませんでした。");
                return;
            }

            // 分割結合の設定です。
            var conf = new GranularityConvertOptionUnity(new GranularityConvertOption(MeshGranularity.PerCityModelArea, 1),
                new[] { target.gameObject }, true);
            // 分割結合します。
            await new CityGranularityConverter().ConvertAsync(conf);
        }
    }
}
