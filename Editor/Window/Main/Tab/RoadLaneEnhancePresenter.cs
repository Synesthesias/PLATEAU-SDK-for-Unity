using PLATEAU.Editor.Window.Common;
using TargetT = UnityEngine.GameObject;

namespace PLATEAU.Editor.Window.Main.Tab
{
    /// <summary>
    /// 道路レーンの見た目補正機能のPresenterクラスです。
    /// </summary>
    public class RoadLaneEnhancePresenter : ITabContent
    {
        private ElementGroup guis;
        private TargetT target;

        public RoadLaneEnhancePresenter()
        {
            guis = new ElementGroup("", 0,
                new HeaderElementGroup("", "道路のレーンの見た目を補正します。", HeaderType.Subtitle),
                new ObjectFieldElement<TargetT>("", "対象", OnTargetChanged),
                new ButtonElement("", "実行", OnExecButtonPushed)
            );
        }

        public void Draw()
        {
            guis.Draw();
        }

        private void OnExecButtonPushed()
        {
        }

        private void OnTargetChanged(TargetT targetArg) => target = targetArg; 

        public void OnTabUnselect()
        {
        }
        
        public void Dispose()
        {
        }
    }
}