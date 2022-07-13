using System;
using System.IO;
using PLATEAU.Util;
using UnityEngine;

namespace PLATEAU.CityMeta
{
    [Serializable]
    internal class ImportDestPath
    {
        public const string MetaDataFileName = "CityMapMetaData.asset";
        
        [SerializeField] private string dirAssetsPath;

        public string DirAssetsPath
        {
            get => this.dirAssetsPath;
            set => this.dirAssetsPath = value;
        }

        public string DirFullPath
        {
            get
            {
                if (this.dirAssetsPath == null) return "";
                return PathUtil.AssetsPathToFullPath(this.dirAssetsPath);
            }
        }

        public string MetadataAssetPath => Path.Combine(this.dirAssetsPath, MetaDataFileName);
    }
}