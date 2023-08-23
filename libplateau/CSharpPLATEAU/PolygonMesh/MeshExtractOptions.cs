﻿using System;
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
        public MeshExtractOptions(PlateauVector3d referencePoint, CoordinateSystem meshAxes, MeshGranularity meshGranularity, uint minLOD, uint maxLOD, bool exportAppearance, int gridCountOfSide, float unitScale, int coordinateZoneID, bool excludeCityObjectOutsideExtent, bool excludePolygonsOutsideExtent, bool enableTexturePacking, uint texturePackingResolution, Extent extent)
        {
            this.ReferencePoint = referencePoint;
            this.MeshAxes = meshAxes;
            this.MeshGranularity = meshGranularity;
            this.ExportAppearance = exportAppearance;
            this.CoordinateZoneID = coordinateZoneID;
            this.ExcludeCityObjectOutsideExtent = excludeCityObjectOutsideExtent;
            this.ExcludePolygonsOutsideExtent = excludePolygonsOutsideExtent;
            this.Extent = extent;
            this.minLOD = minLOD;
            this.maxLOD = maxLOD;
            this.unitScale = unitScale;
            this.gridCountOfSide = gridCountOfSide;
            this.EnableTexturePacking = enableTexturePacking; 
            this.TexturePackingResolution = texturePackingResolution; 
            
            // 上で全てのメンバー変数を設定できてますが、バリデーションをするため念のためメソッドやプロパティも呼びます。
            SetLODRange(minLOD, maxLOD);
            UnitScale = unitScale;
            GridCountOfSide = gridCountOfSide;
        }

        /// <summary> 直交座標系における座標で、3Dモデルの原点をどこに設定するかです。 </summary>
        public  PlateauVector3d ReferencePoint;
        
        /// <summary> 座標軸の向きです。 </summary>
        public CoordinateSystem MeshAxes;
        
        /// <summary> メッシュ結合の粒度です。 </summary>
        public MeshGranularity MeshGranularity;
        
        /// <summary> 出力するLODの範囲上限です。 </summary>
        private uint maxLOD;
        
        /// <summary> 出力するLODの範囲の下限です。 </summary>
        private uint minLOD;

        public void SetLODRange(uint minLODArg, uint maxLODArg)
        {
            if (minLODArg > maxLODArg)
            {
                throw new ArgumentException($"Invalid LOD Range: {nameof(this.minLOD)} should not greater than {nameof(this.maxLOD)}.");
            }
            this.maxLOD = maxLODArg;
            this.minLOD = minLODArg;
        }
        
        /// <summary> テクスチャを含めるかどうかです。 </summary>
        [MarshalAs(UnmanagedType.U1)] public bool ExportAppearance;
        
        /// <summary> メッシュ結合の粒度が「都市モデル単位」の時のみ有効で、この設定では都市を格子状のグリッドに分割するので、その1辺あたりの分割数(縦の数 = 横の数)です。</summary>
        private int gridCountOfSide;

        public int GridCountOfSide
        {
            get => this.gridCountOfSide;
            set
            {
                if (value <= 0 || value > 999) // 999の理由は、普通に考えればこれ以上に分割する理由はないだろうという大雑把な数。
                {
                    throw new Exception($"Invalid number : {nameof(this.gridCountOfSide)} should be positive number and below 1000.");
                }
                this.gridCountOfSide = value;
            }
        }
        
        /// <summary>  大きさ補正です。  </summary>
        private float unitScale;

        public float UnitScale
        {
            get => this.unitScale;
            set
            {
                if (Math.Abs(this.UnitScale) < 0.00000001)
                {
                    throw new ArgumentException($"Validate failed : {nameof(this.UnitScale)} is too small.");
                }
                this.unitScale = value;
            }
        }
        
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
        [MarshalAs(UnmanagedType.U1)] public bool ExcludePolygonsOutsideExtent;
        
        /// <summary>
        /// テクスチャ結合（複数のテクスチャ画像を結合する機能）を有効にするかどうかを bool で指定します。
        /// </summary>
        [MarshalAs(UnmanagedType.U1)] public bool EnableTexturePacking;

        /// <summary> テクスチャ結合時の結合先のテクスチャ画像の解像度（縦：texture_packing_resolution x 横:texture_packing_resolution） </summary>
        public uint TexturePackingResolution;
        
        /// <summary>  対象範囲を緯度・経度・高さで指定します。 </summary>
        public Extent Extent;

        /// <summary> デフォルト値の設定を返します。 </summary>
        internal static MeshExtractOptions DefaultValue()
        {
            var apiResult = NativeMethods.plateau_mesh_extract_options_default_value(out var defaultOptions);
            DLLUtil.CheckDllError(apiResult);
            return defaultOptions;
        }

        private static class NativeMethods
         {
             [DllImport(DLLUtil.DllName)]
             internal static extern APIResult plateau_mesh_extract_options_default_value(
                 out MeshExtractOptions outDefaultOptions);
         }
    }
}
