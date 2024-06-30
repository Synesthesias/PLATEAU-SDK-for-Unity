using PLATEAU.RoadNetwork;
using UnityEditor;
using UnityEngine.UIElements;

namespace PLATEAU.Editor.RoadNetwork
{
    [CustomPropertyDrawer(typeof(RnNode))]
    public class RnNodeDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            return base.CreatePropertyGUI(property);
        }
    }
}