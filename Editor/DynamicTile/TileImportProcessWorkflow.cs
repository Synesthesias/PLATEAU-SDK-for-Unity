using System.Collections.Generic;
using System.Linq;
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
        private bool isHDRP;

        private List<Volume> originalVolumes = new();           // シーン内の全Global Volume
        private List<bool> originalVolumeStates = new();        // 元の ON/OFF 状態を保存

        private Volume captureVolume;
        private VolumeProfile captureProfile;

        private List<Light> allLights = new();                // シーン内の全ライト
        private List<bool> originalLightStates = new();       // 元の ON/OFF 状態を保存

        public TileImportProcessWorkflow()
        {
#if PLATEAU_HDRP
            isHDRP = true;
#else
            isHDRP = false;
#endif
        }

        // Import前に呼び出される処理
        public void PreProcess()
        {
            if (!isHDRP)
                return;

            // シーン内の全 Light を取得
            allLights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None).ToList();

            // 元の状態を保存する配列を確保
            originalLightStates = new();
            for (int i = 0; i < allLights.Count; i++)
                originalLightStates.Add(allLights[i].enabled);

            // 既存の Global Volume を取得
            originalVolumes = Object.FindObjectsByType<Volume>(FindObjectsSortMode.None)
                                    .Where(v => v.isGlobal)
                                    .ToList();

            // 全ライト OFF
            foreach (var light in allLights)
                light.enabled = false;

            // 全ての Global Volume を無効化
            originalVolumeStates = originalVolumes.Select(v => v.enabled).ToList();
            foreach (var v in originalVolumes)
                v.enabled = false;

            // プロファイルをロード
            captureProfile = Resources.Load<VolumeProfile>("PlateauVolume/HDRPCaptureVolumeProfile");

            if (captureProfile == null)
            {
                Debug.LogError("Capture VolumeProfile が見つかりません");
                return;
            }

            // キャプチャ専用 Volume を生成
            var go = new GameObject("HDRP_CaptureVolume (Temporary)");
            captureVolume = go.AddComponent<Volume>();
            captureVolume.isGlobal = true;

            captureVolume.profile = captureProfile;
        }
        
        // Import後に呼び出される処理
        public void PostProcess()
        {
            if (!isHDRP)
                return;

            // ライトの状態を元に戻す
            for (int i = 0; i < allLights.Count; i++)
            {
                if (allLights[i] != null)
                    allLights[i].enabled = originalLightStates[i];
            }
            
            // キャプチャ専用 Volume を削除
            if (captureVolume != null)
                Object.DestroyImmediate(captureVolume.gameObject);

            // 元の Volume の enabled を復元
            for (int i = 0; i < originalVolumes.Count; i++)
            {
                if (originalVolumes[i] != null)
                    originalVolumes[i].enabled = originalVolumeStates[i];
            }
        }

    }
}
