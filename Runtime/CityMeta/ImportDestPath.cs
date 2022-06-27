using System;
using PLATEAU.Util;

namespace PLATEAU.CityMeta
{
    [Serializable]
    internal class ImportDestPath
    {
        public string dirAssetPath;

        public string DirFullPath => PathUtil.FullPathToAssetsPath(this.dirAssetPath);
    }
}