using UnityEditor;
using UnityEngine;

namespace PLATEAU.CityImport.AreaSelector
{
#if UNITY_EDITOR
    internal class MeshCodeSearchWindow : UnityEditor.EditorWindow
    {
        private AreaSelectorBehaviour areaSelector;

        string code = "";
        string errorText = "";

        internal static MeshCodeSearchWindow ShowWindow()
        {
            MeshCodeSearchWindow wnd = GetWindow<MeshCodeSearchWindow>();
            wnd.titleContent = new GUIContent("メッシュコード入力");
            wnd.minSize = wnd.maxSize = new Vector2(380, 140); 
            return wnd;
        }

        internal void Init(AreaSelectorBehaviour areaSelector)
        {
            this.areaSelector = areaSelector;
            code = errorText = "";
            this.autoRepaintOnSceneChange = true;
        }


        private void OnGUI()
        {
            //code = "53395680";
            var style = new GUIStyle
            {
                padding = new RectOffset(8, 8, 4, 4),
                margin = new RectOffset(8, 8, 4, 4)
            };

            var errorLabelStyle = new GUIStyle(EditorStyles.label)
            {
                normal ={ textColor = Color.red }
            };

            EditorGUILayout.BeginVertical(style);
            EditorGUILayout.LabelField("メッシュコードを入力してください。");
            EditorGUILayout.LabelField("（6桁または８桁の数字）");
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(style);
            code = EditorGUILayout.TextField("", code);
            EditorGUILayout.LabelField(errorText, errorLabelStyle);
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginHorizontal(style);
            if (GUILayout.Button("キャンセル", GUILayout.Height(20), GUILayout.MaxWidth(80)))
            {
                this.Close();
            }

            if (GUILayout.Button("Ok", GUILayout.Height(20), GUILayout.MaxWidth(80)))
            {
                if (!int.TryParse(code, out var res))
                {
                    errorText = "数字を入力してください";
                }
                else if (code.Length != 6 && code.Length != 8)
                {
                    errorText = "6桁または８桁の数字を入力してください";
                }   
                else if(!areaSelector.SearchByMeshCode(code))
                {
                    errorText = "メッシュコードが範囲外です";
                }
                else
                {
                    this.Close();
                }
            }
            EditorGUILayout.EndHorizontal();
        }
    }
#endif
}
