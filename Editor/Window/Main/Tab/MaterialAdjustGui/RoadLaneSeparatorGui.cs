using PLATEAU.Editor.Window.Common;
using UnityEditor;
using TargetT = UnityEngine.GameObject;

namespace PLATEAU.Editor.Window.Main.Tab
{
    public class RoadLaneSeparatorGui : ITabContent
    {
        private ElementGroup guis;
        private TargetT target;
        
        private static readonly string[] sidewalkWidthSelectionsStr = {"2m 歩道", "3m 自転車歩行者道", "3.5m 交通量の多い歩道", "4m 交通量の多い自転車歩行者道"};
        private static readonly float[] sidewalkWidthSelectionsFloat = { 2, 3, 3.5f, 4 };
        private int sidewalkWidthIndex = 0;
        private float sidewalkWidth = 2.0f;

        public RoadLaneSeparatorGui()
        {
            guis = new ElementGroup("", 0,
                new HeaderElementGroup("", "道路を歩道と各車道に分割します。", HeaderType.Subtitle),
                new ObjectFieldElement<TargetT>("", "対象", OnTargetChanged),
                new GeneralElement("", DrawSidewalkWidthSelector),
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

        /// <summary> 歩道の幅を選択します。 </summary>
        private void DrawSidewalkWidthSelector()
        {
            sidewalkWidthIndex = EditorGUILayout.Popup("歩道の幅", sidewalkWidthIndex, sidewalkWidthSelectionsStr);
            sidewalkWidth = sidewalkWidthSelectionsFloat[sidewalkWidthIndex];
        }

        public void OnTabUnselect()
        {
        }
        
        public void Dispose()
        {
        }
    }
}