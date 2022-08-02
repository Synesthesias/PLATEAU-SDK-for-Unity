using System;
using System.Collections.Generic;
using System.Linq;
using PLATEAU.CommonDataStructure;
using UnityEditor;
using UnityEngine;

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

        [NonSerialized] private Dictionary<string, GameObject> nameToObjCache; 

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
        public static Dictionary<GmlType, MinMax<int>> AvailableLodInObjs(IReadOnlyCollection<ObjInfo> objInfoList)
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

        /// <summary>
        /// [Editor専用]
        /// <see cref="ObjInfo"/> が指し示す3Dモデルファイルの中身のうち、引数に合致する名前の GameObject を返します。
        /// 初回実行時はファイル内の全オブジェクト名をキャッシュに入れるので時間がかかりますが、2回目以降はキャッシュから引っ張るので早いです。
        /// PLATEAU プロジェクトにおいて、このキャッシュは重要です。
        /// なぜなら、1つの3Dモデルファイルの中に大量にオブジェクトが入っている状況で、モデル配置のために何度もメッシュオブジェクトを検索するには
        /// キャッシュがないと極めて遅くなるためです。
        /// </summary>
        #if UNITY_EDITOR
        public GameObject GetGameObjByName(string objName)
        {
            if (this.nameToObjCache == null)
            {
                ComposeNameToObjCache();
            }

            if (this.nameToObjCache.TryGetValue(objName, out var gameObj))
            {
                return gameObj;
            }

            return null;
        }
        #endif // UNITY_EDITOR

        #if UNITY_EDITOR
        private void ComposeNameToObjCache()
        {
            this.nameToObjCache = new Dictionary<string, GameObject>();
            // 3Dモデルファイル内で、対応するメッシュを探します。
            var gameObjs = AssetDatabase
                .LoadAllAssetsAtPath(this.assetsPath)
                .OfType<GameObject>()
                .Skip(1); // 順番は 3Dモデルファイル → 中身　です。中身だけ見たいので最初は飛ばします。
            foreach (var go in gameObjs)
            {
                this.nameToObjCache.Add(go.name, go);
            }
        }
        #endif // UNITY_EDITOR
    }
}