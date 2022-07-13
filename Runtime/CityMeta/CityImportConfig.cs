using System;
using System.Collections.Generic;
using PLATEAU.Interop;
using PLATEAU.IO;
using UnityEngine;

namespace PLATEAU.CityMeta
{
    /// <summary>
    /// PLATEAU元データをインポートする時の設定です。
    /// </summary>
    
    // 注意事項 1:
    // このクラスは多くの場所で利用されることに注意してください。
    // 例えば 変換設定GUI、変換処理 だけでなく、
    // 変換時の設定を覚えておく目的で ScriptableObject としてファイルに保存されます。
    // (具体的には <see cref="CityMetadata"/> によって保持され保存されます。)
    // そのため、考えなしにデータ構造を変えたり、処理の途中でメンバ変数をリセットする処理を組んだりすると、
    // 変換に失敗したり、過去の設定から再変換する機能に問題が起きる可能性が高まります。
    //
    // 注意事項 2:
    // 変換時の設定を覚えておく目的で、クラスは Serializable属性が付いている必要があります。
    // これはこのクラスが保持するメンバ変数のクラス、例えば <see cref="GmlSearcherConfig"/> も同様です。
    
    [Serializable]
    internal class CityImportConfig
    {
        /// <summary> インポート時の対象gmlファイルの絞り込みの設定 </summary>
        public GmlSearcherConfig gmlSearcherConfig = new GmlSearcherConfig();
        
        /// <summary> インポート元ファイルのパス（コピー後）です。通常 StreamingAssets内を指します。 パスは Assets から始まります。 </summary>
        public PlateauSourcePath sourcePath = new PlateauSourcePath("");

        /// <summary> これはファイルには記録されません。 インポート元ファイルのパス（コピー前）です。 Assetsフォルダ外を指すこともあります。フルパスです。 </summary>
        [NonSerialized] public string SrcRootPathBeforeImport;
        
        /// <summary> インポートの出力先 </summary>
        public ImportDestPath importDestPath = new ImportDestPath();

        /// <summary> インポートした物をシーンに配置する設定 </summary>
        public CityMeshPlacerConfig cityMeshPlacerConfig = new CityMeshPlacerConfig();

        public ObjConvertTypesConfig objConvertTypesConfig = new ObjConvertTypesConfig();

        /// <summary> obj変換時にテクスチャを含めるかどうか </summary>
        public bool exportAppearance = true;

        /// <summary> オブジェクト分けの粒度 </summary>
        public MeshGranularity meshGranularity = MeshGranularity.PerPrimaryFeatureObject;
        
        /// <summary> メッシュ変換の基準座標 </summary>
        public Vector3 referencePoint = Vector3.zero;

        /// <summary> インポートによって生成された objファイル情報のリスト </summary>
        public List<ObjInfo> generatedObjFiles = new List<ObjInfo>();

        /// <summary>
        /// インポート元のルートディレクトリの名前
        /// </summary>
        public string rootDirName = "";
        
        /// <summary> インポート時のログレベル </summary>
        public DllLogLevel logLevel = DllLogLevel.Error;
    }
}