using PLATEAU.Editor.Window.Main.Tab.RoadGuiParts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
