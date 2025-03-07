using System;
using UnityEditor;

namespace PLATEAU.Editor.Window.Common
{
    // EditorGUILayout等のシンプルな機能をElementでラップしたクラス群です。
    
    /// <summary>
    /// EditorGUILayout.ToggleLeftをElementでラップしたクラスです。
    /// </summary>
    internal class ToggleLeftElement : Element
    {
        private string labelText;
        private bool value;

        private bool Value
        {
            get
            {
                return value;
            }
            set
            {
                bool isChanged = this.value != value;
                this.value = value;
                if (isChanged)
                {
                    onValueChanged?.Invoke(value);
                }
                
            }
        }

        private Action<bool> onValueChanged;

        public ToggleLeftElement(string elementName, string labelText, bool defaultValue, Action<bool> onValueChanged) : base(elementName)
        {
            this.onValueChanged = onValueChanged;
            this.labelText = labelText;
            Value = defaultValue;
            onValueChanged?.Invoke(defaultValue);
        }
        
        public override void DrawContent()
        {
            Value = EditorGUILayout.ToggleLeft(labelText, Value);
        }
        
        public override void Dispose(){}
    }
    
    /// <summary>
    /// ヘッダーと、それに属するコンテンツをElementGroupでラップしたクラスです。
    /// </summary>
    internal class HeaderElementGroup : ElementGroup
    {
        private string headerText;
        private HeaderType headerType;
        
        public HeaderElementGroup(string elementName, string headerText, HeaderType headerType, params Element[] innerElements) : base(elementName, 0, innerElements)
        {
            this.headerText = headerText;
            this.headerType = headerType;
        }

        public override void DrawContent()
        {
            // ヘッダーの表示
            switch (headerType)
            {
                case HeaderType.Header:
                    PlateauEditorStyle.Heading(headerText, null);
                    break;
                case HeaderType.HeaderNum1:
                    PlateauEditorStyle.Heading(headerText, "num1.png");
                    break;
                case HeaderType.HeaderNum2:
                    PlateauEditorStyle.Heading(headerText, "num2.png");
                    break;
                case HeaderType.HeaderNum3:
                    PlateauEditorStyle.Heading(headerText, "num3.png");
                    break;
                case HeaderType.Subtitle:
                    PlateauEditorStyle.SubTitle(headerText);
                    break;
                case HeaderType.Separator:
                    PlateauEditorStyle.Separator(0);
                    break;
                default:
                    throw new ArgumentException("unknown header type.");
            }
            using var verticalScope = PlateauEditorStyle.VerticalScopeWithPadding(16,0,8,8);
            base.DrawContent();
        }

        public override void Dispose(){}
    }

    internal enum HeaderType
    {
        Header, Subtitle, Separator, HeaderNum1, HeaderNum2, HeaderNum3
    }
    
}