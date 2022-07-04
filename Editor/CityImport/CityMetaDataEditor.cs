using System.Text;
using PLATEAU.Editor.EditorWindowCommon;
using PLATEAU.CityMeta;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.CityImport
{
    /// <summary>
    /// <see cref="CityMetaData"/> のインスペクタでの表示を行います。
    /// 役割は2つあります。
    /// ・<see cref="CityMetaData"/> が保持する情報を（必要なら）インスペクタに表示すること。
    /// ・<see cref="CityMetaData"/> はインポート時の設定を覚えているので、その設定で「再変換」画面をインスペクタに表示すること。
    /// 　再変換の画面では、ユーザーが設定を変えることが可能であること。この機能によりユーザーが「前回から少しだけ設定を変えて変換」する操作をする上で便利になること。
    /// </summary>
    [CustomEditor(typeof(CityMetaData))]
    internal class CityMetaDataEditor : UnityEditor.Editor
    {
        private bool foldOutIdGmlTable;
        private bool foldOutReconvert;
        private bool foldOutOtherData;
        private CityImportGUI importGUI;
        private Vector2 scrollPosOfIdGmlTable;
        private Vector2 scrollPosOfObjInfo;
        public override void OnInspectorGUI()
        {
            HeaderDrawer.Reset();
            var metaData = target as CityMetaData;
            if (metaData == null)
            {
                EditorGUILayout.HelpBox($"{nameof(metaData)} が null です。", MessageType.Error);
                return;
            }

            this.importGUI ??= new CityImportGUI(metaData.cityImportConfig); // 初期化

            EditorGUILayout.Space(10);
            
            HeaderDrawer.Draw("再変換画面");
            using (PlateauEditorStyle.VerticalScopeLevel1())
            {
                HeaderDrawer.IncrementDepth();
                this.foldOutReconvert = EditorGUILayout.Foldout(this.foldOutReconvert, "再変換");
                if (this.foldOutReconvert)
                {
                    this.importGUI.Draw(metaData.cityImportConfig);
                
                }
                HeaderDrawer.DecrementDepth();
            }
            
            EditorGUILayout.Space(10);
            
            HeaderDrawer.Draw("その他の情報");
            using (PlateauEditorStyle.VerticalScopeLevel1())
            {
                this.foldOutOtherData = EditorGUILayout.Foldout(this.foldOutOtherData, "その他の情報");
                if (this.foldOutOtherData)
                {
                    var cityConfig = metaData.cityImportConfig;
                    var refPoint = cityConfig.referencePoint;
                    EditorGUILayout.LabelField($"基準点: ( {refPoint.x} , {refPoint.y} , {refPoint.z} )");
                    EditorGUILayout.LabelField($"インポート元ルートフォルダ名: {cityConfig.rootDirName}");
                
                    EditorGUILayout.Space(10);
                    
                    // objファイルの情報を表示します。
                    EditorGUILayout.LabelField("3Dモデルのファイルパス");
                    var objSb = new StringBuilder();
                    foreach (var objInfo in cityConfig.generatedObjFiles)
                    {
                        objSb.Append($"{objInfo}\n");
                    }
                    this.scrollPosOfObjInfo =
                        PlateauEditorStyle.ScrollableMultiLineLabel(objSb.ToString(), 300, this.scrollPosOfObjInfo);
                    
                
                    // IDとGMLの紐付け情報を表示します。
                    this.foldOutIdGmlTable = EditorGUILayout.Foldout(this.foldOutIdGmlTable, "IDとGMLファイルの紐付け");
                    if (this.foldOutIdGmlTable)
                    {
                        using (PlateauEditorStyle.VerticalScopeLevel1(false))
                        {
                            var sb = new StringBuilder();
                            foreach (var pair in metaData.idToGmlTable)
                            {
                                sb.Append($"{pair.Key}\n=> {pair.Value}\n\n");
                            }
                            this.scrollPosOfIdGmlTable = PlateauEditorStyle.ScrollableMultiLineLabel(sb.ToString(), 300, this.scrollPosOfIdGmlTable);
                        }
                    }
                }
            }


            // base.OnInspectorGUI();
        }
    }
}