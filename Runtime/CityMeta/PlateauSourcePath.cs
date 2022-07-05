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
        [SerializeField] private string rootDirAssetPath;

        public string RootDirAssetPath
        {
            get => this.rootDirAssetPath;
            set => this.rootDirAssetPath = value;
        }


        /// <summary>
        /// udxパスで初期化します。
        /// </summary>
        public PlateauSourcePath(string rootDirAssetPath)
        {
            this.rootDirAssetPath = rootDirAssetPath;
        }
        

        /// <summary>
        /// 元データのRoot (= udxの親) フォルダの名前です。
        /// udxPath は Assetパスでもフルパスでも動作します。
        /// </summary>
        public static string RootDirName(string rootPath)
        {
            if (!(rootPath.EndsWith("/") || rootPath.EndsWith("\\")))
            {
                rootPath += "/";
            }
            string root = GetRootDirFullPath(rootPath);
            return Path.GetFileName(Path.GetDirectoryName(root));
        }

        /// <summary>
        /// 元データのRoot (= udxの親) フォルダのパスです。
        /// rootPath Assetパスでもフルパスでも動作します。
        /// </summary>
        public static string GetRootDirFullPath(string rootPath)
        {
            return Path.GetFullPath(rootPath);
        }

        public void SetRootDirFullPath(string rootDirFullPath)
        {
            this.rootDirAssetPath = PathUtil.FullPathToAssetsPath(rootDirFullPath);
        }
        
        

        /// <summary>
        /// udxフォルダからの相対パスを受け取り、そのフルパスを返します。
        /// </summary>
        public string UdxRelativeToFullPath(string relativePathFromUdx)
        {
            return Path.GetFullPath(Path.Combine(UdxAssetsPath(), relativePathFromUdx));
        }

        public string UdxAssetsPath()
        {
            return Path.Combine(this.rootDirAssetPath, "udx");
        }

        public string UdxFullPath()
        {
            return Path.GetFullPath(Path.Combine(Application.dataPath, "../", UdxAssetsPath()));
        }

        /// <summary>
        /// 地物タイプのフォルダパスを返します。
        /// </summary>
        public string GmlTypeDirFullPath(string gmlTypePrefix)
        {
            return Path.GetFullPath(Path.Combine(FullUdxPath, gmlTypePrefix));
        }

        /// <summary>
        /// パスは内部的には Assets/ から始まるパスで記録されますが、
        /// フルパスでget,setしたいときはこのプロパティを使います。
        /// </summary>
        public string FullUdxPath
        {
            get => Path.GetFullPath(Path.Combine(Application.dataPath, "../", UdxAssetsPath()));
            // set => this.udxAssetPath = PathUtil.FullPathToAssetsPath(value);
        }
    }

    internal static class PlateauSourcePathExtension
    {
        public static string RootDirFullPath(this PlateauSourcePath source)
        {
            return PlateauSourcePath.GetRootDirFullPath(source.RootDirAssetPath);
        }

        // public static string RootDirAssetsPath(this PlateauSourcePath source)
        // {
        //     return PathUtil.FullPathToAssetsPath(Path.GetFullPath(source.rootAssetPath));
        // }
    }
}