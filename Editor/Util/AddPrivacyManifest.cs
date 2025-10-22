using PLATEAU.Util;
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
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

        // Xcode プロジェクトを開く
        string projPath = PBXProject.GetPBXProjectPath(path);
        PBXProject proj = new PBXProject();
        proj.ReadFromFile(projPath);

        // メインターゲット取得
        string targetGuid = proj.GetUnityMainTargetGuid();


        // PrivacyInfo.xcprivacy をアプリルートにコピー。
        // ただしユーザーのものがすでにあればそれを尊重するため上書きはしない
        string src = Path.Combine(PathUtil.SdkBasePath, "Plugins/iOS", "PrivacyInfo.xcprivacy");
        string dst = Path.Combine(path, "PrivacyInfo.xcprivacy");
        if (!File.Exists(dst))
        {
            File.Copy(src, dst, false);
        }
        

        // Xcode プロジェクトに追加（Copy Bundle Resources）
        string fileGuid = proj.AddFile("PrivacyInfo.xcprivacy", "PrivacyInfo.xcprivacy", PBXSourceTree.Source);
        proj.AddFileToBuild(targetGuid, fileGuid);

        proj.WriteToFile(projPath);
        UnityEngine.Debug.Log("✅ PrivacyInfo.xcprivacy added to iOS build.");
    }
#endif
    }
}
