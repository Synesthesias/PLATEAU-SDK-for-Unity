namespace PLATEAU.CityImport.Config
{
    /// <summary>
    /// インポート形式を表すenum
    /// </summary>
    public enum ImportType
    {
        /// <summary>
        /// シーンに配置
        /// </summary>
        Scene = 0,
        
        /// <summary>
        /// 動的タイル（Addressable出力）
        /// </summary>
        DynamicTile = 1
    }
    
    public class DynamicTileImportConfig
    {
        /// <summary>
        /// インポート形式
        /// </summary>
        public ImportType ImportType { get; set; } = ImportType.Scene;
        
        /// <summary>
        /// インポート形式のインデックス（互換性のため）
        /// </summary>
        public int ImportTypeIndex 
        { 
            get => (int)ImportType; 
            set => ImportType = (ImportType)value; 
        }

        /// <summary>
        /// 出力先パス
        /// </summary>
        public string OutputPath { get; set; } = string.Empty;

        /// <summary>
        /// LOD1の建物にテクスチャを貼るか
        /// </summary>
        public bool Lod1Texture { get; set; } = false;

        /// <summary>
        /// ZoomLevel11のテクスチャ解像度分母(1倍, 1/2倍)
        /// </summary>
        public int ZoomLevel11TextureResolutionDenominator { get; set; } = 1;

        /// <summary>
        /// ZoomLevel10のテクスチャ解像度分母(1/2, 1/4倍)
        /// </summary>
        public int ZoomLevel10TextureResolutionDenominator { get; set; } = 2;

        public DynamicTileImportConfig() { }

        public DynamicTileImportConfig(ImportType importType, string outputPath, bool lod1Texture)
        {
            ImportType = importType;
            OutputPath = outputPath;
            Lod1Texture = lod1Texture;
        }

        /// <summary>
        /// 出力先パスが有効かどうかを判定します
        /// Assetsフォルダ直下（ルート）の場合は無効とします
        /// </summary>
        public bool IsValidOutputPath => !PLATEAU.Util.AssetPathUtil.IsAssetsFolderRoot(OutputPath);
    }
}