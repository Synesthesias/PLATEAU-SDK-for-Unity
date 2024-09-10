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
        

        public RoadChangePresenter()
        {
            guis = new ElementGroup("", 0,
                new HeaderElementGroup("", "道路を変更します。", HeaderType.Subtitle),
                new HeaderElementGroup("", "道路ネットワーク編集", HeaderType.HeaderNum1),
                new ObjectFieldElement<TargetT>("", "対象", OnTargetChanged),
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