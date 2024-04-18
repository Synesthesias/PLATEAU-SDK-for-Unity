using PLATEAU.CityInfo;
using PLATEAU.Util;

namespace PLATEAU.GranularityConvert
{
    /// <summary>
    /// 分割結合の設定値です。
    /// DLL向けの設定とUnity向けの設定をまとめたものです。
    /// </summary>
    public class GranularityConvertOptionUnity
    {
        /// <summary> DLL向けの設定です。 </summary>
        public GranularityConvertOption NativeOption { get; }
        public UniqueParentTransformList SrcTransforms { get; }
        public bool DoDestroySrcObjs { get; }

        public GranularityConvertOptionUnity(
            GranularityConvertOption nativeOption, // 注意: GranularityConvertOption.GridCountの設定は未実装であり、何の値に設定しても動作に影響しません。
            UniqueParentTransformList srcTransforms,
            bool doDestroySrcObjs)
        {
            NativeOption = nativeOption;
            SrcTransforms = srcTransforms;
            DoDestroySrcObjs = doDestroySrcObjs;
        }

        public bool IsValid()
        {
            if (SrcTransforms.Count == 0)
            {
                Dialogue.Display("ゲームオブジェクトが選択されていません。\n選択してから実行してください。", "OK");
                return false;
            }
            
            bool containsCog = false;
            foreach (var srcTrans in SrcTransforms.Get)
            {
                if (srcTrans.GetComponentInChildren<PLATEAUCityObjectGroup>() != null)
                {
                    containsCog = true;
                    break;
                }
            }

            if (!containsCog)
            {
                Dialogue.Display(
                    "選択されたゲームオブジェクトまたはその子に地物情報が含まれていないため分割結合できません。\nPLATEAUInstancedCityObjectまたはその親を選択してください。", "OK");
                return false;
            }

            return true;
        } 
    }
}