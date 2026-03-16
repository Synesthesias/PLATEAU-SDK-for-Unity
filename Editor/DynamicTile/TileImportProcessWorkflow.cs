using UnityEngine;
using UnityEngine.Rendering;

namespace PLATEAU.Editor.DynamicTile
{

    /// <summary>
    /// Tile ImportのPre/Post処理を行う
    /// HDRPの際、ライトを全てOFFにし、Volume Profileをキャプチャ用のものに差し替え、終了時に元に戻す処理を行うクラス
    /// </summary>
    public class TileImportProcessWorkflow
    {
        public Volume volume;                     // Global Volume
        public VolumeProfile normalProfile;       // 通常用
        public VolumeProfile captureProfile;      // キャプチャ用

        private Light[] allLights;                // シーン内の全ライト
        private bool[] originalLightStates;       // 元の ON/OFF 状態を保存

        private bool isHDRP = false;

        public TileImportProcessWorkflow()
        {
#if PLATEAU_HDRP
            isHDRP = true;
#endif
            if (!isHDRP)
                return;

            // シーン内の全 Light を取得
            allLights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);

            // 元の状態を保存する配列を確保
            originalLightStates = new bool[allLights.Length];
            for (int i = 0; i < allLights.Length; i++)
                originalLightStates[i] = allLights[i].enabled;

            volume = Object.FindFirstObjectByType<Volume>();
            normalProfile = volume.profile;
            captureProfile = Resources.Load<VolumeProfile>("PlateauVolume/HDRPCaptureVolumeProfile");
        }

        // キャプチャ開始
        public void PreProcess()
        {
            if (!isHDRP)
                return;

            // 1. 全ライト OFF
            foreach (var light in allLights)
                light.enabled = false;

            // 2. Volume Profile をキャプチャ用に差し替え
            volume.profile = captureProfile;
        }

        // キャプチャ終了
        public void PostProcess()
        {
            if (!isHDRP)
                return;

            // 1. ライトの状態を元に戻す
            for (int i = 0; i < allLights.Length; i++)
                allLights[i].enabled = originalLightStates[i];

            // 2. Volume Profile を通常用に戻す
            volume.profile = normalProfile;
        }
    }

}
