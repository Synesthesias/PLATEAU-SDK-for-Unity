using UnityEngine;

/// PropertyDrawersEditorとセットで使用する
namespace PLATEAU.Util
{
    /// <summary>
    /// プロパティが特定の条件を満たす場合にのみ表示されるようにする属性。
    /// </summary>
    public class ConditionalShowAttribute : PropertyAttribute
    {
        public string conditionField;

        public ConditionalShowAttribute(string conditionField)
        {
            this.conditionField = conditionField;
        }
    }


    public class ConditionalShowBoolAttribute : PropertyAttribute
    {
        public bool show;

        public ConditionalShowBoolAttribute(bool show)
        {
            this.show = show;
        }
    }

    /// <summary>
    /// プロパティを読み取り専用にする属性。
    /// </summary>
    public class ReadOnlyAttribute : PropertyAttribute { }

}
