using System;

namespace PLATEAU.CityMeta
{
    /// <summary>
    /// ファイルパスとLODを紐付けて保持します。
    /// </summary>
    [Serializable]
    internal class FileLodInfo
    {
        public string AssetsPath { get; set; }
        public int Lod { get; set; }

        public FileLodInfo(string assetsPath, int lod)
        {
            AssetsPath = assetsPath;
            Lod = lod;
        }

        public FileLodInfo(FileLodInfo copySrc)
        {
            AssetsPath = copySrc.AssetsPath;
            Lod = copySrc.Lod;
        }

        public override string ToString()
        {
            return $"[LOD{Lod}] {AssetsPath}";
        }
    }
}