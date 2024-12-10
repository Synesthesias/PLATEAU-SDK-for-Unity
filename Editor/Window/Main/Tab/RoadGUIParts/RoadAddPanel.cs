using UnityEngine.UIElements;

namespace PLATEAU.Editor.Window.Main.Tab.RoadGuiParts
{
    /// <summary>
    /// RoadNetwork_AddPanel.uxmlのバインドや挙動の制御を行うクラス
    /// </summary>
    public class RoadAddPanel : RoadAdjustGuiPartBase
    {
        static readonly string name = "RoadNetwork_AddPanel";


        public RoadAddPanel() : base(name)
        {
        }

        protected override void OnTabSelected(VisualElement root)
        {
            base.OnTabSelected(root);
        }

        protected override void OnTabUnselected(VisualElement root)
        {
            base.OnTabUnselected(root);
        }
    }
}
