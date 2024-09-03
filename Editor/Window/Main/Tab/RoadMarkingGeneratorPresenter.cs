using PLATEAU.CityInfo;
using PLATEAU.Editor.Window.Common;
using PLATEAU.RoadAdjust.RoadMarking;
using PLATEAU.RoadNetwork.Structure;
using UnityEngine;
using TargetT = UnityEngine.GameObject;

namespace PLATEAU.Editor.Window.Main.Tab
{
    /// <summary>
    /// 路面標示機能のPresenterクラスです。
    /// </summary>
    public class RoadMarkingGeneratorPresenter : ITabContent
    {
        private ElementGroup guis;
        private TargetT target;

        public RoadMarkingGeneratorPresenter()
        {
            guis = new ElementGroup("", 0,
                new HeaderElementGroup("", "路面に描かれた車線と横断歩道を生成します。", HeaderType.Subtitle),
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
            var modelComponent = Object.FindObjectOfType<PLATEAURnStructureModel>();
            var model = modelComponent.RoadNetwork;
            var generator = new RoadMarkingGenerator(model);
            generator.Generate();
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