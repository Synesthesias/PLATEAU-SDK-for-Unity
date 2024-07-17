using PLATEAU.Editor.Window.Common;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.Window.Main
{
    internal class PlateauWindowFooterGui : IEditorDrawable
    {
        private static readonly PlateauEditorStyle.ColorLightDark s_questionnaireBtnBackGroundColor = new("#2c9ba6", "#2cffff");
        
        public void Draw()
        {
            using (new EditorGUILayout.VerticalScope())
            {
                GUILayout.Space(10);
                GUILayout.FlexibleSpace();

                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.FlexibleSpace();

                    var defaultBackgroundColor = GUI.backgroundColor;
                    GUI.backgroundColor = s_questionnaireBtnBackGroundColor.Color;
                    if (GUILayout.Button(new GUIContent("SDK利用者アンケートを募集しています！"), ButtonStyle()))
                    {
                        Application.OpenURL("https://docs.google.com/forms/d/e/1FAIpQLSdEU_CjR6-wT9cpvusvqX0bFkPIOE1J-UlJ-oF2JLOLAJoYNQ/viewform?usp=sharing");
                    }
                    GUI.backgroundColor = defaultBackgroundColor;
                    GUILayout.Space(10);
                }

                GUILayout.Space(10);
            }
        }

        private static GUIStyle ButtonStyle()
        {
            return new GUIStyle(EditorStyles.miniButton)
            {
                normal = { textColor = Color.white },
                hover = { textColor = Color.white },
                active = { textColor = Color.white },
                focused = { textColor = Color.white },
                fontStyle = FontStyle.Bold, 
                alignment = TextAnchor.MiddleCenter,
                fixedHeight = 30,
                fixedWidth = 280
            };
        }
        
        public void Dispose() { }
    }
}