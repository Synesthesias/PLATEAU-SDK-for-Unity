using PLATEAU.CityAdjust.ConvertToAsset;
using PLATEAU.Editor.EditorWindow.Common;
using PLATEAU.Util;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.EditorWindow.PlateauWindow
{
    internal class ConvertToAssetGUI : IEditorDrawable
    {
        private ConvertToAssetConfig conf = ConvertToAssetConfig.DefaultValue;
        
        // 設定のバリデーションによるエラーメッセージは、ユーザーが1度でも設定を編集したあとに出すようにします。
        // 設定してもないのにエラーアイコンが出ると邪魔なので。
        private bool isSrcGameObjModified = false;
        private bool isAssetPathModified = false;
        public void Draw()
        {   
            // ヘッダー
            PlateauEditorStyle.Heading("Assetsに保存", null);
            PlateauEditorStyle.SubTitle("シーン内に直接保存されている都市モデルを、属性情報等を保ったままFBXに書き出します");

            bool isReadyToConvert = true;
            
            // 元オブジェクト選択
            using (PlateauEditorStyle.VerticalScopeLevel1())
            {
                EditorGUI.BeginChangeCheck();
                conf.SrcGameObj = (GameObject)EditorGUILayout.ObjectField("元オブジェクト", conf.SrcGameObj, typeof(GameObject), true);
                if (EditorGUI.EndChangeCheck()) isSrcGameObjModified = true;
                if (!conf.ValidateSrcGameObj(out var errorMsg) && isSrcGameObjModified)
                {
                    EditorGUILayout.HelpBox(errorMsg, MessageType.Error);
                    isReadyToConvert = false;
                }
            }
            
            
            // 出力先選択
            using (PlateauEditorStyle.VerticalScopeLevel2())
            {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.TextField("出力先Assetsフォルダ", conf.AssetPath);
                EditorGUI.EndDisabledGroup();
                using (PlateauEditorStyle.VerticalScopeLevel1())
                {
                    PlateauEditorStyle.CenterAlignHorizontal(() =>
                    {
                        if (conf.ValidateAssetPath(out var _))
                        {
                            EditorGUILayout.LabelField("OK", GUILayout.Width(50));
                        }
                        else
                        {
                            EditorGUILayout.LabelField("Assets以下の空のフォルダを指定してください。", GUILayout.Width(220));
                        }
                    });
                    PlateauEditorStyle.CenterAlignHorizontal(() =>
                    {
                        if (PlateauEditorStyle.MiniButton("出力先選択", 100))
                        {
                            string selectedPath = EditorUtility.OpenFolderPanel("出力先選択", conf.AssetPath, "");
                            isAssetPathModified = true;
                            if (!string.IsNullOrEmpty(selectedPath))
                            {
                                conf.SetByFullPath(selectedPath);
                            }
                        }
                    });
                }
                

                if (!conf.ValidateAssetPath(out string errorMessage) && isAssetPathModified)
                {
                    EditorGUILayout.HelpBox(errorMessage, MessageType.Error);
                    isReadyToConvert = false;
                }
            }
            
            
            // 抽出ボタン
            using (PlateauEditorStyle.VerticalScopeLevel1())
            {
                if (PlateauEditorStyle.MainButton("Assetsに保存"))
                {
                    if (isReadyToConvert && isSrcGameObjModified && isAssetPathModified)
                    {
                        // ここで実行します
                        new ConvertToAsset().Convert(conf);
                        ResetGUI();
                    }
                    else
                    {
                        Dialogue.Display("設定に正しくない点があるため処理を中止しました。\n設定画面を確認のうえ、再度実行してください。", "OK");
                    }
               
                }
            }
            
        }

        public void Dispose()
        {
        }

        private void ResetGUI()
        {
            conf = ConvertToAssetConfig.DefaultValue;
            isAssetPathModified = false;
            isSrcGameObjModified = false;
        }
    }
}