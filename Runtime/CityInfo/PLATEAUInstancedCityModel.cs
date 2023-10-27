using System.Collections.Generic;
using System.Threading.Tasks;
using PLATEAU.CityGML;
using PLATEAU.Geometries;
using PLATEAU.Native;
using UnityEngine;

namespace PLATEAU.CityInfo
{
    /// <summary>
    /// GMLファイルからUnityにインポートした都市のルートGameObjectに付与されます。
    /// </summary>
    public class PLATEAUInstancedCityModel : MonoBehaviour
    {
        [SerializeField] private GeoReferenceData geoReferenceData;

        /// <summary>
        /// ゲームオブジェクトの階層のうち、GMLファイルに対応する Transform の一覧を返します。
        /// </summary>
        public Transform[] GmlTransforms
        {
            get
            {
                var trans = transform;
                int childCount = trans.childCount;
                var ret = new Transform[childCount];
                for (int i = 0; i < childCount; i++)
                {
                    ret[i] = trans.GetChild(i);
                }

                return ret;
            }
        }

        /// <summary>
        /// GMLに対応する Transform からGMLを非同期ロードして <see cref="CityModel"/> を返します。
        /// 合致するGMLがなければ null を返します。
        /// </summary>
        public async Task<CityModel> LoadGmlAsync(Transform gmlTransform)
        {
            var trans = transform;
            int childCount = trans.childCount;
            for (int i = 0; i < childCount; i++)
            {
                var child = trans.GetChild(i);
                if (child == gmlTransform)
                {
                    return await PLATEAUCityGmlProxy.LoadAsync(child.gameObject, trans.name);
                }
            }

            return null;
        }

        /// <summary>
        /// GMLに対応する Transform を受け取り、そのゲームオブジェクト階層の子を見て
        /// 利用可能なLODの番号をリストで返します。
        /// </summary>
        public static List<int> GetLods(Transform gmlTransform)
        {
            var lods = new List<int>();
            foreach (var lodTrans in GetLodTransforms(gmlTransform))
            {
                if (TryParseLodGameObjectName(lodTrans.name, out int lod))
                {
                    lods.Add(lod);
                }
            }

            return lods;
        }

        /// <summary>
        /// GMLに対応する Transform を受け取り、そのゲームオブジェクト階層の子を見て
        /// 利用可能なLODに対応する Transform のリストを返します。
        /// </summary>
        public static List<Transform> GetLodTransforms(Transform gmlTransform)
        {
            int childCount = gmlTransform.childCount;
            var ret = new List<Transform>();
            for (int i = 0; i < childCount; i++)
            {
                var child = gmlTransform.GetChild(i);
                string childName = child.name;
                if (childName.StartsWith("LOD"))
                {
                    ret.Add(child);
                }
            }

            return ret;
        }

        public static bool TryParseLodGameObjectName(string lodGameObjName, out int outLod)
        {
            return int.TryParse(lodGameObjName.Substring("LOD".Length), out outLod);
        }

        /// <summary>
        /// 引数として GMLに対応する Transform と LOD番号を受け取ります。
        /// GMLのゲームオブジェクト階層を見て、そのLODで存在する CityObject に対応する Transform をリストで返します。
        /// </summary>
        //TODO staticにしない記法のほうが分かりやすそう
        public static List<Transform> GetCityObjects(Transform gmlTransform, int lod)
        {
            var lodTrans = gmlTransform.Find($"LOD{lod}");
            if (lodTrans == null)
            {
                Debug.LogError("LOD GameObject is not found.");
                return null;
            }

            var result = new List<Transform>();
            GetChildrenRecursive(lodTrans, result);
            return result;
        }

        private static void GetChildrenRecursive(Transform trans, List<Transform> result)
        {
            int childCount = trans.childCount;
            for (int i = 0; i < childCount; i++)
            {
                var childTrans = trans.GetChild(i);
                result.Add(childTrans);
                GetChildrenRecursive(childTrans, result);
            }
        }
        
        /// <summary>
        /// 極座標と平面直角座標を変換するインスタンスです。
        /// </summary>
        public GeoReference GeoReference
        {
            get
            {
                var grd = geoReferenceData;
                var gr =
                    GeoReference.Create(
                        new PlateauVector3d(
                            grd.referencePointX,
                            grd.referencePointY,
                            grd.referencePointZ
                        ),
                        grd.unitScale, grd.coordinateSystem, grd.zoneID
                    );
                return gr;
            }
            set
            {
                var point = value.ReferencePoint;
                var data =
                    new GeoReferenceData(
                        point.X, point.Y, point.Z, value.UnitScale,
                        value.CoordinateSystem, value.ZoneID
                    );
                geoReferenceData = data;
            }
        }

        /// <summary>
        /// 緯度を取得します
        /// </summary>
        public double Latitude
        {
            get => GeoReference.Unproject(new PlateauVector3d(0, 0, 0)).Latitude;
        }

        /// <summary>
        /// 経度を取得します
        /// </summary>
        public double Longitude
        {
            get => GeoReference.Unproject(new PlateauVector3d(0, 0, 0)).Longitude;
        }

        public void CopyFrom(PLATEAUInstancedCityModel src)
        {
            this.GeoReference = src.GeoReference;
        }
    }
}
