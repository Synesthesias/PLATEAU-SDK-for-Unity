using PLATEAU.CityImport.AreaSelector;
using PLATEAU.CityImport.Config;
using PLATEAU.CityImport.Import;
using PLATEAU.Dataset;
using UnityEngine;

// ランタイムでインポートするサンプルです。
public class RuntimeImportSample : MonoBehaviour
{
    async void Start()
    {
        // このパスをあなたのマシンに合わせて変更してください。
        var datasetConf = new DatasetSourceConfigLocal("F:\\Desktop\\13100_tokyo23-ku_2022_citygml_1_2_op");
        
        var meshCodesStr = new string[] { "53393652", "53393653", "53393642", "53393643" };
        var meshCodeList = MeshCodeList.CreateFromMeshCodesStr(meshCodesStr);
        var conf = CityImportConfig.CreateWithAreaSelectResult(new AreaSelectResult(new ConfigBeforeAreaSelect(datasetConf, 9), meshCodeList));
        await CityImporter.ImportAsync(conf, null, null);
    }
}
