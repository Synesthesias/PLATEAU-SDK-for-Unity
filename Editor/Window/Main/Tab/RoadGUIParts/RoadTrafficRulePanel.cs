using UnityEngine.UIElements;

namespace PLATEAU.Editor.Window.Main.Tab.RoadGuiParts
{
    /// <summary>
    /// RoadNetwork_TrafficRulePanel.uxmlのバインドや挙動の制御を行うクラス
    /// </summary>
    public class RoadTrafficRulePanel : RoadAdjustGuiPartBase
    {
        static readonly string name = "RoadNetwork_TrafficRulePanel";


        public RoadTrafficRulePanel(VisualElement rootVisualElement) : base(name, rootVisualElement)
        {
            abcd = efgh;
            copilot 自動reviewのテストです。copilotさん、ここはふさわしくないので指摘してね
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
