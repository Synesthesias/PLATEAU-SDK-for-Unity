using UnityEditor;
using UnityEngine.UIElements;

namespace PLATEAU.Editor.RoadNetwork
{
    [CustomPropertyDrawer(typeof(RoadNetworkNodeDrawer))]
    public class RoadNetworkNodeDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {

            return base.CreatePropertyGUI(property);
        }
    }
}