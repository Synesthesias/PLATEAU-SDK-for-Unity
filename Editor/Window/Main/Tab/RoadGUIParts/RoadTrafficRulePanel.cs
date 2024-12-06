using UnityEngine.UIElements;

namespace PLATEAU.Editor.Window.Main.Tab.RoadGuiParts
{
    /// <summary>
    /// RoadNetwork_TrafficRulePanel.uxmlのバインドや挙動の制御を行うクラス
    /// </summary>
    public class RoadTrafficRulePanel : RoadAdjustGuiPartBase
    {
        static readonly string name = "RoadNetwork_TrafficRulePanel";


        public RoadTrafficRulePanel() : base(name)
        {
        }

        public override void Init(VisualElement root)
        {
            base.Init(root);
        }

        public override void Terminate(VisualElement root)
        {
            base.Terminate(root);
        }
    }
}
