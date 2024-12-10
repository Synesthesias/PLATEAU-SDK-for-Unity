using UnityEngine.UIElements;

namespace PLATEAU.Editor.Window.Main.Tab.RoadGuiParts
{
    /// <summary>
    /// RoadNetwork_AddPanel.uxmlのバインドや挙動の制御を行うクラス
    /// </summary>
    public class RoadAddPanel : RoadAdjustGuiPartBase
    {
        static readonly string name = "RoadNetwork_AddPanel";


        public RoadAddPanel(VisualElement rootVisualElement) : base(name, rootVisualElement)
        {
        }

        protected override void OnTabSelected(VisualElement root)
        {
            base.OnTabSelected(root);
        }

        protected override void OnTabUnselected()
        {
            base.OnTabUnselected();
        }
    }
}
