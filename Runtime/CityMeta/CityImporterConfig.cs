using System;
using PLATEAU.CityGML;
using UnityEngine;

namespace PLATEAU.CityMeta
{
    /// <summary>
    /// Plateau元データをインポートする時の設定です。
    /// </summary>
    
    // 注意事項 1:
    // このクラスは多くの場所で利用されることに注意してください。
    // 例えば 変換設定GUI、変換処理 だけでなく、
    // 変換時の設定を覚えておく目的で ScriptableObject としてファイルに保存されます。
    // (具体的には <see cref="CityMetaData"/> によって保持され保存されます。)
    // そのため、考えなしにデータ構造を変えたり、処理の途中でメンバ変数をリセットする処理を組んだりすると、
    // 変換に失敗したり、過去の設定から再変換する機能に問題が起きる可能性が高まります。
    //
    // 注意事項 2:
    // 変換時の設定を覚えておく目的で、クラスは Serializable属性が付いている必要があります。
    // これはこのクラスが保持するメンバ変数のクラス、例えば <see cref="GmlSearcherConfig"/> も同様です。
    
    [Serializable]
    public class CityImporterConfig
    {
        /// <value> インポート時の対象gmlファイルの絞り込みの設定 </value>
        public GmlSearcherConfig gmlSearcherConfig = new GmlSearcherConfig();
        
        /// <value> インポート元ファイルのパスです。通常 StreamingAssets内を指します。 </value>
        public string sourceUdxFolderPath = "";
        
        /// <value> インポートの出力先 </value>
        public string exportFolderPath = "";
        
        /// <value> インポート時に最適化するかどうか </value>
        public bool optimizeFlag = true;
        
        /// <value> オブジェクト分けの粒度 </value>
        public MeshGranularity meshGranularity = MeshGranularity.PerPrimaryFeatureObject;
        
        /// <value> メッシュ変換の基準座標 </value>
        public Vector3 referencePoint = Vector3.zero;
        
        /// <value> インポート時のログレベル </value>
        public DllLogLevel logLevel = DllLogLevel.Error;
    }
}