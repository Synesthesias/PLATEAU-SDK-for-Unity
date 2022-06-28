namespace PLATEAU.Editor.CityImport
{
    internal class ObjInfo
    {
        public string AssetsPath { get; set; }
        public int Lod { get; set; }

        public ObjInfo(string assetsPath, int lod)
        {
            AssetsPath = assetsPath;
            Lod = lod;
        }

        public ObjInfo(ObjInfo copySrc)
        {
            AssetsPath = copySrc.AssetsPath;
            Lod = copySrc.Lod;
        }
    }
}