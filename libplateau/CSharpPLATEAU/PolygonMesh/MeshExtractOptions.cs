using System;
using System.Runtime.InteropServices;
using PLATEAU.Geometries;
using PLATEAU.Interop;
using PLATEAU.Native;

namespace PLATEAU.PolygonMesh
{
    
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
    /// GMLファイルから3Dメッシュを取り出すための設定です。
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct MeshExtractOptions
    {
        /// <summary> 直交座標系における座標で、3Dモデルの原点をどこに設定するかです。 </summary>
        public PlateauVector3d ReferencePoint;
        /// <summary> 座標軸の向きです。 </summary>
        public CoordinateSystem MeshAxes;
        /// <summary> メッシュ結合の粒度です。 </summary>
        public MeshGranularity MeshGranularity;
        /// <summary> 出力するLODの範囲上限です。 </summary>
        public uint MaxLOD;
        /// <summary> 出力するLODの範囲の下限です。 </summary>
        public uint MinLOD;
        /// <summary> テクスチャを含めるかどうかです。 </summary>
        [MarshalAs(UnmanagedType.U1)] public bool ExportAppearance;
        /// <summary> メッシュ結合の粒度が「都市モデル単位」の時のみ有効で、この設定では都市を格子状のグリッドに分割するので、その1辺あたりの分割数(縦の数 = 横の数)です。</summary>
        public int GridCountOfSide;
        /// <summary>  大きさ補正です。  </summary>
        public float UnitScale;
        /// <summary>
        /// 国土交通省が規定する、日本の平面直角座標系の基準点の番号です。
        /// 詳しくは次の国土地理院のサイトをご覧ください。
        /// <see href="https://www.gsi.go.jp/sokuchikijun/jpc.html"/>
        /// </summary>
        public int CoordinateZoneID;
        /// <summary>
        /// 範囲外の3Dモデルを出力から除外するための、2つの方法のうち1つを有効にするかどうかを bool で指定します。
        /// その方法とは、都市オブジェクトの最初の頂点の位置が範囲外のとき、そのオブジェクトはすべて範囲外とみなして出力から除外します。
        /// これはビル1棟程度の大きさのオブジェクトでは有効ですが、
        /// 10km×10kmの地形のような巨大なオブジェクトでは、実際には範囲内なのに最初の頂点が遠いために除外されるということがおきます。
        /// したがって、この値は建物では true, 地形では false となるべきです。
        /// </summary>
        [MarshalAs(UnmanagedType.U1)] public bool ExcludeCityObjectOutsideExtent;
        /// <summary>
        /// 範囲外の3Dモデルを出力から除外するための、2つの方法のうち1つを有効にするかどうかを bool で指定します。
        /// その方法とは、メッシュ操作によって、範囲外に存在するポリゴンを除外します。
        /// この方法であれば 10km×10km の地形など巨大なオブジェクトにも対応できます。
        /// </summary>
        [MarshalAs(UnmanagedType.U1)] public bool ExcludeTrianglesOutsideExtent;
        /// <summary>  対象範囲を緯度・経度・高さで指定します。 </summary>
         public Extent Extent;
        
        /// <summary> デフォルト値の設定を返します。 </summary>
        public static MeshExtractOptions DefaultValue()
        {
            var apiResult = NativeMethods.plateau_mesh_extract_options_default_value(out var defaultOptions);
            DLLUtil.CheckDllError(apiResult);
            return defaultOptions;
        }

         /// <summary>
        /// 設定の値が正常なら true, 異常な点があれば false を返します。
         /// <param name="failureMessage">異常な点があれば、それを説明する文字列が入ります。正常なら空文字列になります。</param>
        /// </summary>
        public bool Validate(out string failureMessage)
        {
            failureMessage = "";
            if (this.MinLOD > this.MaxLOD)
            {
                failureMessage = $"Validate failed : {nameof(this.MinLOD)} should not greater than {nameof(this.MaxLOD)}.";
                return false;
            }

            if (this.GridCountOfSide <= 0)
            {
                failureMessage = $"Validate failed : {nameof(this.GridCountOfSide)} should be positive number.";
                return false;
            }

            if (Math.Abs(this.UnitScale) < 0.00000001)
            {
                failureMessage = $"Validate failed : {nameof(this.UnitScale)} is too small.";
                return false;
            }

            return true;
        }
         
         private static class NativeMethods
         {
             [DllImport(DLLUtil.DllName)]
             internal static extern APIResult plateau_mesh_extract_options_default_value(
                 out MeshExtractOptions outDefaultOptions);
         }
    }
}
