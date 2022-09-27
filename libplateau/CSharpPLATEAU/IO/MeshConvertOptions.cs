using PLATEAU.CityGML;
using PLATEAU.Interop;

namespace PLATEAU.IO
{
    /// <summary>
    /// 各列挙子について、3つのアルファベットはXYZ軸がどの方角、方向になるかを表しています。<br/>
    /// N,S,E,Wはそれぞれ北,南,東,西<br/>
    /// U,Dはそれぞれ上,下<br/>
    /// に対応します。<br/>
    /// </summary>
    public enum CoordinateSystem
    {
        /// <summary>
        /// PLATEAUでの座標系
        /// </summary>
        ENU,
        WUN,
        /// <summary>
        /// Unreal Engineでの座標系
        /// </summary>
        NWU,
        /// <summary>
        /// Unityでの座標系
        /// </summary>
        EUN
    }

    /// <summary>
    /// メッシュの結合単位
    /// </summary>
    public enum MeshGranularity
    {
        /// <summary>
        /// 最小地物単位(LOD2, LOD3の各部品)
        /// </summary>
        PerAtomicFeatureObject,
        /// <summary>
        /// 主要地物単位(建築物、道路等)
        /// </summary>
        PerPrimaryFeatureObject,
        /// <summary>
        /// 都市モデル地域単位(GMLファイル内のすべてを結合)
        /// </summary>
        PerCityModelArea
    }

    /// <summary>
    /// 都市モデルからメッシュへ変換するためのオプションです。
    /// </summary>
    public class MeshConvertOptions
    {
        private MeshConvertOptionsData data;

        /// <summary>
        /// 出力後のメッシュの座標系を指定します。
        /// </summary>
        public CoordinateSystem MeshAxes
        {
            get => this.data.MeshAxes;
            set => this.data.MeshAxes = value;
        }

        /// <summary>
        /// 変換時の基準点を指定します。
        /// 基準点は平面直角座標であり、メッシュは基準点からの相対座標で出力されます。
        /// </summary>
        public PlateauVector3d ReferencePoint
        {
            get => this.data.ReferencePoint;
            set => this.data.ReferencePoint = value;
        }

        /// <summary>
        /// 出力後のメッシュの粒度(結合単位)を指定します。
        /// </summary>
        public MeshGranularity MeshGranularity
        {
            get => this.data.MeshGranularity;
            set => this.data.MeshGranularity = value;
        }

        /// <summary>
        /// 出力後のメッシュに含める最小のLODを指定します。
        /// </summary>
        public uint MinLOD
        {
            get => this.data.MinLOD;
            set => this.data.MinLOD = value;
        }

        /// <summary>
        /// 出力後のメッシュに含める最大のLODを指定します。
        /// </summary>
        public uint MaxLOD
        {
            get => this.data.MaxLOD;
            set => this.data.MaxLOD = value;
        }

        /// <summary>
        /// 1つの地物について複数のLODがある場合に最大LOD以外のジオメトリを出力するかどうかを指定します。
        /// </summary>
        public bool ExportLowerLOD
        {
            get => this.data.ExportLowerLOD;
            set => this.data.ExportLowerLOD = value;
        }

        /// <summary>
        /// テクスチャ、マテリアル情報を出力するかどうかを指定します。
        /// </summary>
        public bool ExportAppearance
        {
            get => this.data.ExportAppearance;
            set => this.data.ExportAppearance = value;
        }

        /// <summary>
        /// メートル法基準での単位の倍率を指定します。
        /// </summary>
        public float UnitScale
        {
            get => this.data.UnitScale;
            set => this.data.UnitScale = value;
        }

        internal MeshConvertOptionsData Data => this.data;

        public MeshConvertOptions()
        {
            this.data = NativeMethods.plateau_create_mesh_convert_options();
        }

        internal MeshConvertOptions(MeshConvertOptionsData data)
        {
            this.data = data;
        }

        public void SetValidReferencePoint(CityModel cityModel)
        {
            NativeMethods.plateau_mesh_convert_options_set_valid_reference_point(ref this.data, cityModel.Handle);
        }
    }
}
