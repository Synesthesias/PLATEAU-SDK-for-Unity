namespace PLATEAU.CityImport.Config
{
    public class DynamicTileImportConfig
    {
        /// <summary>
        /// インポート形式（0: シーンに配置, 1: 動的タイル（Addressable出力））
        /// </summary>
        public int ImportTypeIndex { get; set; } = 0;

        /// <summary>
        /// 出力先パス
        /// </summary>
        public string OutputPath { get; set; } = string.Empty;

        /// <summary>
        /// LOD1の建物にテクスチャを貼るか
        /// </summary>
        public bool Lod1Texture { get; set; } = false;
    }
}