using PLATEAU.Editor.Window.Common;
using System;
using System.Linq;
using UnityEditor;
using TargetT = UnityEngine.GameObject;

namespace PLATEAU.Editor.Window.Main.Tab
{
    public class RoadChangePresenter : ITabContent
    {
        private ElementGroup guis;
        private TargetT target;
        private LineSeparateType lineSeparateType;
        
        private static readonly string[] sidewalkWidthSelectionsStr = {"2m 歩道", "3m 自転車歩行者道", "3.5m 交通量の多い歩道", "4m 交通量の多い自転車歩行者道"};
        private static readonly float[] sidewalkWidthSelectionsFloat = { 2, 3, 3.5f, 4 };
        private int sidewalkWidthIndex = 0;
        private float sidewalkWidth = 2.0f;
        

        public RoadChangePresenter()
        {
            guis = new ElementGroup("", 0,
                new HeaderElementGroup("", "道路を変更します。", HeaderType.Subtitle),
                new HeaderElementGroup("", "道路ネットワーク編集", HeaderType.HeaderNum1),
                new ObjectFieldElement<TargetT>("", "対象", OnTargetChanged),
                new GeneralElement("", DrawSidewalkWidthSelector),
                new GeneralElement("", () => EditorGUILayout.Space(150)),
                new HeaderElementGroup("", "道路ネットワークをメッシュに反映", HeaderType.HeaderNum2),
                new GeneralElement("", DrawLineSeparateTypeSelector),
                new ButtonElement("", "道路ネットワークをメッシュに反映", OnExecButtonPushed)
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
        
        private void DrawLineSeparateTypeSelector()
        {
            // 車線ごとにゲームオブジェクトを分けるかどうか
            var choices = (LineSeparateType[])Enum.GetValues(typeof(LineSeparateType));
            var choicesStr = choices.Select(t => t.ToJapaneseName()).ToArray();
            int selectedIndex = Array.IndexOf(choices, lineSeparateType);
            lineSeparateType = (LineSeparateType)EditorGUILayout.Popup("車線分割", selectedIndex, choicesStr);
        }

        public void OnTabUnselect()
        {
        }
        
        public void Dispose()
        {
        }
    }

    /// <summary> 道路変更で車線ごとにゲームオブジェクトを分けるかどうか </summary>
    public enum LineSeparateType
    {
        /// <summary> 車線で分けない </summary>
        Combine,
        /// <summary> 車線で分ける </summary>
        Separate
    }
    
    public static class LineSeparateTypeExtension
    {
        public static string ToJapaneseName(this LineSeparateType type) => type switch
        {
            LineSeparateType.Combine => "車線をまとめる",
            LineSeparateType.Separate => "車線で分ける",
            _ => throw new System.Exception("Unknown LineSeparateType.")
        };
    }
}