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

        public string DirFullPath => PathUtil.FullPathToAssetsPath(this.dirAssetPath);
        
        public string MetaDataAssetPath => Path.Combine(this.dirAssetPath, MetaDataFileName);
    }
}