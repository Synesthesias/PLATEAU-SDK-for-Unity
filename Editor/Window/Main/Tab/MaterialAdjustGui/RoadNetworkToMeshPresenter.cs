using PLATEAU.Editor.Window.Common;
using PLATEAU.RoadAdjust.RoadNetworkToMesh;
using PLATEAU.RoadNetwork.Structure;
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PLATEAU.Editor.Window.Main.Tab
{
    /// <summary>
    /// 道路ネットワークをメッシュにする機能のPresenter層です。
    /// </summary>
    public class RoadNetworkToMeshPresenter : ITabContent
    {
        private ElementGroup guis;
        private RnmLineSeparateType lineSeparateType;
        

        public RoadNetworkToMeshPresenter()
        {
            guis = new ElementGroup("", 0,
                new HeaderElementGroup("", "道路を変更します。", HeaderType.Subtitle),
                new HeaderElementGroup("", "道路ネットワーク編集", HeaderType.HeaderNum1),
                new GeneralElement("", () => EditorGUILayout.Space(150)),
                new HeaderElementGroup("", "道路ネットワークをメッシュに反映", HeaderType.HeaderNum2),
                new GeneralElement("", DrawLineSeparateTypeSelector),
                new ButtonElement("", "道路ネットワークをメッシュに反映", OnExecButtonPushed)
            );
        }
        
        public VisualElement CreateGui()
        {
            return new IMGUIContainer(Draw);
        }

        private void Draw()
        {
            guis.Draw();
        }

        private void OnExecButtonPushed()
        {
            var roadModel = Object.FindObjectOfType<PLATEAURnStructureModel>();
            if (roadModel == null)
            {
                Debug.LogError("道路ネットワークがありません。");
                return;
            }
            new RoadNetworkToMesh(roadModel.RoadNetwork, lineSeparateType).Generate();
        }
        

        
        
        private void DrawLineSeparateTypeSelector()
        {
            // 車線ごとにゲームオブジェクトを分けるかどうか
            var choices = (RnmLineSeparateType[])Enum.GetValues(typeof(RnmLineSeparateType));
            var choicesStr = choices.Select(t => t.ToJapaneseName()).ToArray();
            int selectedIndex = Array.IndexOf(choices, lineSeparateType);
            lineSeparateType = (RnmLineSeparateType)EditorGUILayout.Popup("車線分割", selectedIndex, choicesStr);
        }

        public void OnTabUnselect()
        {
        }
        
        public void Dispose()
        {
        }
    }
    
    
    public static class LineSeparateTypeExtension
    {
        public static string ToJapaneseName(this RnmLineSeparateType type) => type switch
        {
            RnmLineSeparateType.Combine => "車線をまとめる",
            RnmLineSeparateType.Separate => "車線で分ける",
            _ => throw new System.Exception("Unknown LineSeparateType.")
        };
    }
}