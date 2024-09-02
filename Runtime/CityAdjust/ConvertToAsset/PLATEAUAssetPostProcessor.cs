
#if UNITY_EDITOR

using UnityEditor;

namespace PLATEAU.CityAdjust.ConvertToAsset
{
    /// <summary>
    /// アセットのインポート時の動作を、イベントでカスタマイズできるようにしたクラスです。
    /// UnityEditorでのみ動作します。
    /// </summary>
    internal class PLATEAUAssetPostProcessor : AssetPostprocessor
    {
        public delegate void PostProcessHandler();
        
        public static event PostProcessHandler OnPostProcess;
        
        private void OnPreprocessModel()
        {
            OnPostProcess?.Invoke();
        }
    }
}
#endif