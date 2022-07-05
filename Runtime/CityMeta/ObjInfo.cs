using System;
using System.Collections.Generic;

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
        
        
        /// <summary>
        /// インポート時に生成された objファイルのリストを受け取り、
        /// Gmlタイプ別にどの範囲のLODが存在するかを辞書形式で返します。
        /// </summary>
        /// <returns>
        /// <see cref="GmlType"/> を key として、存在するLODの範囲を value とします。
        /// <see cref="GmlType"/> に対応する objファイルが存在しない場合は、そのタイプの value は null となります。
        /// </returns>
        public static Dictionary<GmlType, MinMax<int>> AvailableLodInObjs(IReadOnlyList<ObjInfo> objInfoList)
        {
            var typeLodDict = GmlTypeConvert.ComposeTypeDict<MinMax<int>>(null);
            foreach (var objInfo in objInfoList)
            {
                int objLod = objInfo.lod;
                GmlType objType = objInfo.gmlType;
                MinMax<int> dictLods = typeLodDict[objType];
                if (dictLods == null)
                {
                    typeLodDict[objType] = new MinMax<int>(objLod, objLod);
                }
                else
                {
                    int minLod = Math.Min(dictLods.Min, objLod);
                    int maxLod = Math.Max(dictLods.Max, objLod);
                    typeLodDict[objType].SetMinMax(minLod, maxLod);
                }
            }

            return typeLodDict;
        }
    }
}