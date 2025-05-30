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
        /// <summary>
        /// 範囲選択画面が終了した理由を表す列挙型です。
        /// </summary>
        public enum ResultReason
        {
            /// <summary>
            /// ユーザーが範囲選択を決定した
            /// </summary>
            Confirm,
            
            /// <summary>
            /// ユーザーが範囲選択をキャンセルした
            /// </summary>
            Cancel
        }
        
        public ConfigBeforeAreaSelect ConfBeforeAreaSelect { get; }
        public GridCodeList AreaGridCodes { get; }
        public PackageToLodDict PackageToLodDict { get; }
        
        /// <summary>
        /// 範囲選択画面が終了した理由
        /// </summary>
        public ResultReason Reason { get; }
    
        /// <summary>
        /// 範囲選択結果を作成します。
        /// </summary>
        /// <param name="confBeforeAreaSelect">範囲選択前の設定</param>
        /// <param name="areaGridCodes">選択されたグリッドコードのリスト</param>
        /// <param name="reason">範囲選択画面が終了した理由</param>
        public AreaSelectResult(ConfigBeforeAreaSelect confBeforeAreaSelect, GridCodeList areaGridCodes, ResultReason reason)
        {
            ConfBeforeAreaSelect = confBeforeAreaSelect;
            AreaGridCodes = areaGridCodes;
            PackageToLodDict = areaGridCodes.CalcAvailablePackageLodInGridCodes(ConfBeforeAreaSelect.DatasetSourceConfig);
            Reason = reason;
        }

    }
}
