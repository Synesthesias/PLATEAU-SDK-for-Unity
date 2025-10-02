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
        
        public DynamicTileImportConfig() { }

        public DynamicTileImportConfig(ImportType importType, string outputPath, bool lod1Texture)
        {
            ImportType = importType;
            OutputPath = outputPath;
            Lod1Texture = lod1Texture;
        }
    }
}