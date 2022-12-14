using System.Threading.Tasks;
using PLATEAU.CityGML;
using PLATEAU.Geometries;
using PLATEAU.Interop;
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
        /// 名称に合致するGMLがなければ null を返します。
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
        /// 極座標と平面直角座標を変換するインスタンスです。
        /// </summary>
        public GeoReference GeoReference
        {
            get
            {
                var grd = this.geoReferenceData;
                var gr =
                    new GeoReference(
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
                this.geoReferenceData = data;
            }
        }
    }
}
