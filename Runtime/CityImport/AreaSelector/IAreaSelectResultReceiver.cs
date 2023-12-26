using PLATEAU.CityImport.Config;
using PLATEAU.CityImport.Config.PackageImportConfigs;

namespace PLATEAU.CityImport.AreaSelector
{
    /// <summary>
    /// 範囲選択画面の結果を受け取るインターフェイスです。
    /// </summary>
    internal interface IAreaSelectResultReceiver
    {
        public void ReceiveResult(AreaSelectResult areaSelectResult);
    }
    
    /// <summary>
    /// 範囲選択画面の結果を格納するクラスです。
    /// </summary>
    public class AreaSelectResult
    {
        public ConfigBeforeAreaSelect ConfBeforeAreaSelect { get; }
        public MeshCodeList AreaMeshCodes { get; }
        public PackageToLodDict PackageToLodDict { get; }

        public AreaSelectResult(ConfigBeforeAreaSelect confBeforeAreaSelect, MeshCodeList areaMeshCodes)
        {
            ConfBeforeAreaSelect = confBeforeAreaSelect;
            AreaMeshCodes = areaMeshCodes;
            PackageToLodDict = areaMeshCodes.CalcAvailablePackageLodInMeshCodes(ConfBeforeAreaSelect.DatasetSourceConfig);
        }

    }
}
