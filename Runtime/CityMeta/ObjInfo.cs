using System;

namespace PLATEAU.CityMeta
{
    /// <summary>
    /// ファイルパスとLODを紐付けて保持します。
    /// </summary>
    [Serializable]
    internal class ObjInfo
    {
        public string assetsPath;
        public int lod;
        public GmlType gmlType;

        public ObjInfo(string assetsPath, int lod, GmlType gmlType)
        {
            this.assetsPath = assetsPath;
            this.lod = lod;
            this.gmlType = gmlType;
        }

        public ObjInfo(ObjInfo copySrc)
        {
            this.assetsPath = copySrc.assetsPath;
            this.lod = copySrc.lod;
            this.gmlType = copySrc.gmlType;
        }

        public override string ToString()
        {
            return $"[LOD{this.lod}] {this.assetsPath}";
        }
    }
}