using PLATEAU.Util;
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
#if UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif

namespace PLATEAU.Editor.Util
{
    /// <summary>
    /// iOS向けにPrivacyマニフェストをビルド時に追加します。
    /// Plugins/iOS以下に PrivacyInfo.xcprivacy が存在することが前提です。
    /// これがないとTestFlightなどにアップロードできません。
    /// 動作確認するには次のようにします。
    /// ・iOS向けビルド→xcodeでxcodeprojを開いてメニューバーのProduct→Archive
    /// ・Archive完了後にOrganizerが開くので、アーカイブをValidateしてプライバシーマニフェストに関する指摘がなければOK
    /// </summary>
    public static class AddPrivacyManifest {
#if UNITY_IOS
    [PostProcessBuild(999)]
    public static void OnPostProcessBuild(BuildTarget target, string path) {
        if (target != BuildTarget.iOS) return;


        try
        {
            // Xcode プロジェクトを開く
            string projPath = PBXProject.GetPBXProjectPath(path);
            PBXProject proj = new PBXProject();
            proj.ReadFromFile(projPath);

            // メインターゲット取得
            string targetGuid = proj.GetUnityMainTargetGuid();


            // PrivacyInfo.xcprivacy をアプリルートにコピー。
            // ただしユーザーのものがすでにあればそれを尊重するため上書きはしない
            string src = Path.Combine(PathUtil.SdkBasePath, "Plugins/iOS", "PrivacyInfo.xcprivacy");
            if (!File.Exists(src))
            {
                Debug.LogError($"Source privacy manifest not found at: {src}");
                return;
            }
            string dst = Path.Combine(path, "PrivacyInfo.xcprivacy");
            if (!File.Exists(dst))
            {
                File.Copy(src, dst, false);
                Debug.Log("Adding PrivacyInfo.xcprivacy to iOS build.");
            }
            else
            {
                Debug.Log("Using existing PrivacyInfo.xcprivacy");
            }


            // Xcode プロジェクトに追加（Copy Bundle Resources）
            // 既にプロジェクトに追加されているか確認
            string existingFileGuid = proj.FindFileGuidByProjectPath("PrivacyInfo.xcprivacy");
            if (string.IsNullOrEmpty(existingFileGuid))
            {
                string fileGuid = proj.AddFile("PrivacyInfo.xcprivacy", "PrivacyInfo.xcprivacy", PBXSourceTree.Source);
                proj.AddFileToBuild(targetGuid, fileGuid);
                Debug.Log("PrivacyInfo.xcprivacy added to Xcode project.");
            }
            else
            {
                Debug.Log("PrivacyInfo.xcprivacy already exists in Xcode project.");
            }

            proj.WriteToFile(projPath);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to add privacy manifest to Xcode project: {e.Message}\n{e.StackTrace}");
        }

    }
#endif
    }
}
