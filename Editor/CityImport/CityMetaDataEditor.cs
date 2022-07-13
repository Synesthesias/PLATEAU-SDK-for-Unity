using System.Text;
using PLATEAU.Editor.EditorWindowCommon;
using PLATEAU.CityMeta;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.CityImport
{
    /// <summary>
    /// <see cref="CityMetadata"/> のインスペクタでの表示を行います。
    /// 役割は2つあります。
    /// ・<see cref="CityMetadata"/> が保持する情報を（必要なら）インスペクタに表示すること。
    /// ・<see cref="CityMetadata"/> はインポート時の設定を覚えているので、その設定で「再変換」画面をインスペクタに表示すること。
    /// 　再変換の画面では、ユーザーが設定を変えることが可能であること。この機能によりユーザーが「前回から少しだけ設定を変えて変換」する操作をする上で便利になること。
    /// </summary>
    [CustomEditor(typeof(CityMetadata))]
    internal class CityMetaDataEditor : UnityEditor.Editor
    {
        private bool foldOutIdGmlTable;
        private bool foldOutReconvert;
        private bool foldOutReplace;
        private bool foldOutOtherData;
        // private bool foldOutGmlPaths;
        private CityImporterPresenter importerPresenter;
        private readonly CityMeshPlacerPresenter cityMeshPlacerPresenter = new CityMeshPlacerPresenter();
        private Vector2 scrollPosOfIdGmlTable;
        private Vector2 scrollPosOfObjInfo;
        private Vector2 scrollPosOfGmlPaths;
        public override void OnInspectorGUI()
        {
            HeaderDrawer.Reset();
            var metaData = target as CityMetadata;
            if (metaData == null)
            {
                EditorGUILayout.HelpBox($"{nameof(metaData)} が null です。", MessageType.Error);
                return;
            }

            // 初期化
            this.importerPresenter ??= CityImporterPresenter.InitWithConfig(metaData.cityImportConfig);

            EditorGUILayout.Space(10);
            
            HeaderDrawer.Draw("再変換画面");
            
            HeaderDrawer.IncrementDepth();
            
            // 本当はこの部分を using (PlateauEditorStyle.VerticalScopeLevel1()) で囲って
            // インデントを整えたいところですが、
            // Unityのバグで VerticalScope 内でフォルダ選択ダイアログを開くなどの時間のかかる処理をすると
            // エラーメッセージが出てしまうので囲っていません。
            this.foldOutReconvert = EditorGUILayout.Foldout(this.foldOutReconvert, "再変換");
            if (this.foldOutReconvert)
            {
                this.importerPresenter.Draw();
            
            }
            HeaderDrawer.DecrementDepth();

            EditorGUILayout.Space(10);
            
            HeaderDrawer.Draw("シーン配置");
            using (PlateauEditorStyle.VerticalScopeLevel1())
            {
                this.foldOutReplace = EditorGUILayout.Foldout(this.foldOutReplace, "シーンへ再配置");
                if (this.foldOutReplace)
                {
                    HeaderDrawer.IncrementDepth();
                    this.cityMeshPlacerPresenter.Draw(metaData);
                    HeaderDrawer.DecrementDepth();
                }
            }
            
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
                    
                    
                    // this.foldOutGmlPaths = EditorGUILayout.Foldout(this.foldOutGmlPaths, "gmlファイルパス");
                    // if (this.foldOutGmlPaths)
                    // {
                    EditorGUILayout.LabelField("gmlファイルパス");
                    var gmlSb = new StringBuilder();
                    foreach (var gmlPath in metaData.gmlRelativePaths)
                    {
                        gmlSb.Append(gmlPath + "\n");
                    }

                    this.scrollPosOfGmlPaths =
                        PlateauEditorStyle.ScrollableMultiLineLabel(gmlSb.ToString(), 300, this.scrollPosOfGmlPaths);
                    // }

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