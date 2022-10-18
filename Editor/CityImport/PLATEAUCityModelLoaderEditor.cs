using PLATEAU.CityImport;
using PLATEAU.Editor.EditorWindow.Common;
using PLATEAU.Util;
using UnityEditor;

namespace PLATEAU.Editor.CityImport
{
    /// <summary>
    /// 都市モデルの読み込み設定をするGUIをインスペクタに表示し、
    /// ボタン操作に応じて選択画面の表示や読み込みを行います。
    /// </summary>
    [CustomEditor(typeof(PLATEAUCityModelLoader))]
    internal class PLATEAUCityModelLoaderEditor : UnityEditor.Editor
    {
        private bool foldOutDetailData = true;
        public override void OnInspectorGUI()
        {
            var loader = (PLATEAUCityModelLoader)target;



            this.foldOutDetailData = PlateauEditorStyle.FoldOut(this.foldOutDetailData, "詳細データ", () =>
            {
                using (PlateauEditorStyle.VerticalScopeLevel1())
                {
                    EditorGUILayout.LabelField("インポート前パス:");
                    PlateauEditorStyle.MultiLineLabelWithBox(loader.SourcePathBeforeImport);
                    EditorGUILayout.LabelField("インポート後パス:");
                    PlateauEditorStyle.MultiLineLabelWithBox(loader.SourcePathAfterImport);
                    EditorGUILayout.LabelField("地域メッシュコード");
                    PlateauEditorStyle.MultiLineLabelWithBox(DebugUtil.EnumerableToString(loader.AreaMeshCodes));
                    EditorGUILayout.LabelField("範囲");
                    using (PlateauEditorStyle.VerticalScopeLevel2())
                    {
                        var extent = loader.Extent;
                        var min = extent.Min;
                        var max = extent.Max;
                        EditorGUILayout.LabelField($"最小: 緯度 {min.Latitude} , 経度 {min.Longitude}");
                        EditorGUILayout.LabelField($"最大: 緯度 {max.Latitude} , 経度 {max.Longitude}");
                    }
                }
            });
        }
    }
}
