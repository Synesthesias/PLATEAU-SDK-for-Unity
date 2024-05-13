using PLATEAU.RoadNetwork;
using UnityEditor;
using UnityEngine.UIElements;

namespace PLATEAU.Editor.RoadNetwork
{
    [CustomPropertyDrawer(typeof(RoadNetworkBlock))]
    public class RoadNetworkBlockDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {

            return base.CreatePropertyGUI(property);
        }
    }
}