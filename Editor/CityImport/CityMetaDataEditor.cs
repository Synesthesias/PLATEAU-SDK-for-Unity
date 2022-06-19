﻿using PlateauUnitySDK.Editor.EditorWindowCommon;
using PlateauUnitySDK.Runtime.CityMeta;
using UnityEditor;

namespace PlateauUnitySDK.Editor.CityImport
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
        private readonly CityImportGUI importGUI = new CityImportGUI();
        public override void OnInspectorGUI()
        {
            HeaderDrawer.Reset();
            var metaData = target as CityMetaData;
            if (metaData == null)
            {
                EditorGUILayout.HelpBox($"{nameof(metaData)} が null です。", MessageType.Error);
                return;
            }
            
            HeaderDrawer.Draw("IDとGMLファイルの情報");
            this.foldOutIdGmlTable = EditorGUILayout.Foldout(this.foldOutIdGmlTable, "IDとGMLファイルの紐付け");
            if (this.foldOutIdGmlTable)
            {
                foreach (var pair in metaData.idToGmlTable)
                {
                    var str = $"{pair.Key}\n=> {pair.Value}";
                    EditorGUILayout.TextArea(str);
                }
            }

            EditorGUILayout.Space(10);
            
            HeaderDrawer.Draw("再変換画面");
            HeaderDrawer.IncrementDepth();

            this.foldOutReconvert = EditorGUILayout.Foldout(this.foldOutReconvert, "再変換");
            if (this.foldOutReconvert)
            {
                this.importGUI.Draw(metaData.cityImporterConfig);
                
            }
            
            // base.OnInspectorGUI();
        }
    }
}