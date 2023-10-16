using PLATEAU.Dataset;
using PLATEAU.Geometries;
using PLATEAU.Native;
using PLATEAU.Util;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

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
            wnd.titleContent = new GUIContent("���b�V���R�[�h����");
            wnd.maxSize = new Vector2(380,220);
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
            EditorGUILayout.LabelField("���b�V���R�[�h����͂��Ă��������B");
            EditorGUILayout.LabelField("�i6���܂��͂W���̐����j");
            code = EditorGUILayout.TextField("", code);
            EditorGUILayout.LabelField(errorText, errorLabelStyle);
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginHorizontal(style);
            if (GUILayout.Button("�L�����Z��", GUILayout.Height(20), GUILayout.MaxWidth(80)))
            {
                this.Close();
            }

            if (GUILayout.Button("Ok", GUILayout.Height(20), GUILayout.MaxWidth(80)))
            {
                if (!int.TryParse(code, out var res))
                {
                    errorText = "��������͂��Ă�������";
                }
                else if (code.Length != 6 && code.Length != 8)
                {
                    errorText = "6���܂��͂W���̐�������͂��Ă�������";
                }   
                else if(!areaSelector.SearchByMeshCode(code))
                {
                    errorText = "���b�V���R�[�h���Ԉ���Ă��܂�";
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
