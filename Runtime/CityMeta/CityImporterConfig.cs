using System;
using LibPLATEAU.NET.CityGML;
using UnityEngine;

namespace PlateauUnitySDK.Runtime.CityMeta
{
    /// <summary>
    /// <para>
    /// Plateau元データをインポートする時の設定です。</para>
    /// <para>
    /// 注意事項 1:
    /// このクラスは多くの場所で利用されることに注意してください。
    /// 例えば 変換設定GUI、変換処理 だけでなく、
    /// 変換時の設定を覚えておく目的で ScriptableObject としてファイルに保存されます。
    /// (具体的には <see cref="CityMetaData"/> によって保持され保存されます。)
    /// そのため、考えなしにデータ構造を変えたり、処理の途中でメンバ変数をリセットする処理を組んだりすると、
    /// 変換に失敗したり、過去の設定から再変換する機能に問題が起きる可能性が高まります。</para>
    /// <para>
    ///注意事項 2:
    /// 変換時の設定を覚えておく目的で、クラスは Serializable属性が付いている必要があります。
    /// これはこのクラスが保持するメンバ変数のクラス、例えば <see cref="GmlSearcherConfig"/> も同様です。</para>
    /// </summary>
    [Serializable]
    public class CityImporterConfig
    {
        public GmlSearcherConfig gmlSearcherConfig = new GmlSearcherConfig();
        public string sourceUdxFolderPath = "";
        public string exportFolderPath = "";
        public bool optimizeFlag = true;
        public MeshGranularity meshGranularity = MeshGranularity.PerPrimaryFeatureObject;
        public Vector3 referencePoint = Vector3.zero;
        public DllLogLevel logLevel = DllLogLevel.Error;
    }
}