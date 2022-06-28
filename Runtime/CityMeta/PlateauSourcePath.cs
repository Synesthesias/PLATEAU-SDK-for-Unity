using System;
using System.IO;
using PLATEAU.Util;
using UnityEngine;

namespace PLATEAU.CityMeta
{
    /// <summary>
    /// Plateauの元データのパスです。
    /// 2つの役割があります。
    /// ・インポート時の設定として <see cref="CityMetaData"/> に 元データパスを保持、記録されること
    /// ・udxフォルダのパスから、関連パスを取得できるようにすること。
    /// </summary>
    [Serializable]
    internal class PlateauSourcePath
    {
        /// <summary>
        /// Plateau元データのパスの記録です。
        /// パスは Assets/ から始まります。
        /// </summary>
        public string udxAssetPath;
        
        

        /// <summary>
        /// udxパスで初期化します。
        /// </summary>
        public PlateauSourcePath(string udxAssetPath)
        {
            this.udxAssetPath = udxAssetPath;
        }
        

        /// <summary>
        /// 元データのRoot (= udxの親) フォルダの名前です。
        /// udxPath は Assetパスでもフルパスでも動作します。
        /// </summary>
        public static string RootDirName(string udxPath)
        {
            string root = RootDirFullPath(udxPath);
            return Path.GetFileName(Path.GetDirectoryName(root));
        }

        /// <summary>
        /// 元データのRoot (= udxの親) フォルダのパスです。
        /// udxPathは Assetパスでもフルパスでも動作します。
        /// </summary>
        public static string RootDirFullPath(string udxPath)
        {
            return Path.GetFullPath(Path.Combine(udxPath, "../"));
        }
        
        

        /// <summary>
        /// udxフォルダからの相対パスを受け取り、そのフルパスを返します。
        /// </summary>
        public string UdxRelativeToFullPath(string relativePathFromUdx)
        {
            return Path.GetFullPath(Path.Combine(this.udxAssetPath, relativePathFromUdx));
        }

        /// <summary>
        /// パスは内部的には Assets/ から始まるパスで記録されますが、
        /// フルパスでget,setしたいときはこのプロパティを使います。
        /// </summary>
        public string FullUdxPath
        {
            get => Path.GetFullPath(Path.Combine(Application.dataPath, "../", this.udxAssetPath));
            set => this.udxAssetPath = PathUtil.FullPathToAssetsPath(value);
        }
    }

    internal static class PlateauSourcePathExtension
    {
        public static string RootDirFullPath(this PlateauSourcePath source)
        {
            return PlateauSourcePath.RootDirFullPath(source.udxAssetPath);
        }
    }
}