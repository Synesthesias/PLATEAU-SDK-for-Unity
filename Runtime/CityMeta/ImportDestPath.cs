using System;
using System.IO;
using PLATEAU.Util;

namespace PLATEAU.CityMeta
{
    [Serializable]
    internal class ImportDestPath
    {
        public const string MetaDataFileName = "CityMapMetaData.asset";
        
        public string dirAssetPath;

        public string DirFullPath
        {
            get
            {
                if (this.dirAssetPath == null) return "";
                return PathUtil.AssetsPathToFullPath(this.dirAssetPath);
            }
        }

        public string MetaDataAssetPath => Path.Combine(this.dirAssetPath, MetaDataFileName);
    }
}