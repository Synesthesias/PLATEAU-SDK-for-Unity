using System.IO;
using UnityEngine;

namespace PlateauUnitySDK.Runtime.Util
{
    /// <summary>
    /// Plateau が利用するパスです。
    /// </summary>
    public static class PlateauPath
    {
        /// <summary>
        /// gmlをインポートする時、インポート元のgmlはデフォルトではこのパスにコピーされます。
        /// gmlを StreamingAssets フォルダ内に配置する意図は、
        /// 実行中にPlateauオブジェクトの情報を得るためには、実行中にgmlファイルをロードできるようにしたいためです。
        /// パスは StreamingAssets/PLATEAU を指します。
        /// </summary>
        public static string StreamingGmlFolder { get; private set; } = Path.Combine(Application.streamingAssetsPath, "PLATEAU");

        public static void TestOnly_SetStreamingGmlFolder(string path)
        {
            StreamingGmlFolder = path;
        }
    }
}