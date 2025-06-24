using MessagePack;
using PLATEAU.CityInfo;
using PLATEAU.Editor.Window.Common;
using PLATEAU.PolygonMesh;
using PLATEAU.Util;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Newtonsoft.Json;
using System;

namespace PLATEAU.Editor.CityInfo
{
    [CustomEditor(typeof(PLATEAUCityObjectGroup))]
    public class PLATEAUCityObjectGroupEditor : UnityEditor.Editor
    {
        private readonly ScrollView scrollView = new (GUILayout.MaxHeight(400));
        
        // 属性情報の文字列は長いことがあるので、繰り返しの取得を避けるためにキャッシュします。
        private string cachedJson;
        private bool jsonLoaded = false;
        
        // 属性情報の表示制御
        private bool showAttributeInfo = false;

        public void OnEnable()
        {
            ComponentUtility.MoveComponentUp(target as Component);
            
            // 選択でキャッシュをリセット
            cachedJson = null;
            jsonLoaded = false;
            showAttributeInfo = false;
        }

        public override void OnInspectorGUI()
        {
            var cog = target as PLATEAUCityObjectGroup;
            if (cog == null) return;
            
            PlateauEditorStyle.Heading("粒度", null);
            EditorGUILayout.LabelField(cog.Granularity.ToJapaneseString());
            
            PlateauEditorStyle.Heading("属性情報", null);
            using (PlateauEditorStyle.VerticalScopeLevel1())
            {
                // 属性情報をクリップボードにコピーするボタン
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("属性情報をクリップボードにコピー(json形式)"))
                    {
                        string json = GetCachedJson();
                        EditorGUIUtility.systemCopyBuffer = json;
                        Dialogue.Display("属性情報をクリップボードにコピーしました", "OK");
                    }
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("属性情報をクリップボードにコピー(MessagePack形式)"))
                    {
                        var messagePack = GetMessagePackData();
                        string base64 = System.Convert.ToBase64String(messagePack);
                        EditorGUIUtility.systemCopyBuffer = base64;
                        Dialogue.Display($"属性情報をクリップボードにコピーしました。({messagePack.Length}byte)", "OK");
                    }
                }
                
                // 属性情報の表示は重いことがあるので、ボタンで切り替えられるようにします。
                using (new EditorGUILayout.HorizontalScope())
                {
                    string buttonText = showAttributeInfo ? "属性情報を非表示" : "属性情報を表示";
                    if (GUILayout.Button(buttonText))
                    {
                        showAttributeInfo = !showAttributeInfo;
                    }
                }
                
                if (showAttributeInfo)
                {
                    // 属性情報文字列をキャッシュから取得
                    string json = GetCachedJson();
                    
                    scrollView.Draw(() =>
                    {
                        EditorGUILayout.TextArea(json);
                    });
                }
            }
           
            using (new EditorGUI.DisabledScope(true))
            {
                base.OnInspectorGUI();
            }
        }

        private byte[] GetMessagePackData()
        {
            SerializedProperty prop = serializedObject.FindProperty("serializedCityObjects");
            if (prop == null || prop.arraySize <= 0)
            {
                return null;
            }

            byte[] messagePackData = new byte[prop.arraySize];
            for (int i = 0; i < prop.arraySize; i++)
            {
                messagePackData[i] = (byte)prop.GetArrayElementAtIndex(i).intValue;
            }

            return messagePackData;

        }
        
        /// <summary>
        /// 属性情報のMessagePackバイナリをキャッシュから取得します。一度取得したらキャッシュを使用し、長い情報の繰り返し取得を避けます。
        /// </summary>
        private string GetCachedJson()
        {
            // 初回のみserializedCityObjectsのbyte[]から取得
            if (!jsonLoaded)
            {
                var messagePackData = GetMessagePackData();
                cachedJson = messagePackData != null ? MessagePackSerializer.ConvertToJson(messagePackData) : "MessagePackデータが見つかりません";
                jsonLoaded = true;
            }
            
            return cachedJson ?? string.Empty;
        }
    }
}
