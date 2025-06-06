using PLATEAU.CityInfo;
using PLATEAU.Editor.Window.Common;
using PLATEAU.PolygonMesh;
using PLATEAU.Util;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

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
                    if (GUILayout.Button("属性情報をクリップボードにコピー"))
                    {
                        string json = GetCachedJson();
                        EditorGUIUtility.systemCopyBuffer = json;
                        Dialogue.Display("属性情報をクリップボードにコピーしました", "OK");
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
        
        /// <summary>
        /// 属性情報のJSONをキャッシュから取得します。一度取得したらキャッシュを使用し、長い情報の繰り返し取得を避けます。
        /// </summary>
        private string GetCachedJson()
        {
            // 初回のみprop.stringValueから取得
            if (!jsonLoaded)
            {
                SerializedProperty prop = serializedObject.FindProperty("serializedCityObjects");
                cachedJson = prop.stringValue;
                jsonLoaded = true;
            }
            
            return cachedJson ?? string.Empty;
        }
    }
}
