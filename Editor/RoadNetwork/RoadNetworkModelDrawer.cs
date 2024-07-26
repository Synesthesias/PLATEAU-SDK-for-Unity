namespace PLATEAU.Editor.RoadNetwork
{
#if false
    [CustomPropertyDrawer(typeof(RoadNetworkModel))]
    public class RoadNetworkModelDrawer : PropertyDrawer
    {

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            return base.CreatePropertyGUI(property);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            base.OnGUI(position, property, label);

            //EditorGUI.BeginProperty(position, label, property);

            var linkProperty = property?.FindPropertyRelative(nameof(RoadNetworkModel.Links));
            //var arraySize = linkProperty?.arraySize ?? 0;
            //EditorGUI.IntField(position, "LinkSize", arraySize);

            //EditorGUI.EndProperty();
        }
    }
#endif
}