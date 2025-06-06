﻿using PLATEAU.CityExport.Exporters;
using PLATEAU.Geometries;

namespace PLATEAU.Editor.Window.Main.Tab.ExportGuiParts
{
    /// <summary>
    /// あるエクスポート形式について、その形式固有のエクスポート設定とエクスポーターを保持するインターフェイスです。
    /// OBJ, FBX, GLTFのファイル形式ごとにこのインターフェイスを実装します。
    /// それにより形式による設定の差異を吸収します。
    /// </summary>
    internal interface IExportConfigGUI
    {
        /// <summary>
        /// ファイルフォーマットに固有の設定項目のGUIを描画します。
        /// </summary>
        public void Draw();
        
        public void SetCoordinateSystem(CoordinateSystem coordinateSystem);

        public ICityExporter GetExporter();
    }
}
