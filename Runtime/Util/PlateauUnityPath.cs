using System;
using System.IO;
using UnityEngine;

namespace PLATEAU.Util
{
    /// <summary>
    /// PlateauUnitySDK が利用するパスです。
    /// </summary>
    [Obsolete("Use PathUtil.")] internal static class PlateauUnityPath
    {
        /// <summary>
        /// gmlをインポートする時、インポート元のgmlはデフォルトではこのパスにコピーされます。
        /// gmlを StreamingAssets フォルダ内に配置する意図は、
        /// 実行中にPlateauオブジェクトの情報を得るためには、実行中にgmlファイルをロードできるようにしたいためです。
        /// パスは StreamingAssets/PLATEAU を指します。
        /// </summary>
        [Obsolete] private static string StreamingGmlFolder { get; set; } = Path.Combine(Application.streamingAssetsPath, "PLATEAU");

        

        public static string StreamingFolder => Path.Combine(StreamingGmlFolder, "../");

        public static void TestOnly_SetStreamingGmlFolder(string path)
        {
            StreamingGmlFolder = path;
        }
    }
}