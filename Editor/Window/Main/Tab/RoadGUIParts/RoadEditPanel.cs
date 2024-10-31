using PLATEAU.RoadNetwork.Structure.Drawer;
using PLATEAU.RoadNetwork.Structure;
using PLATEAU.RoadNetwork.Tester;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace PLATEAU.Editor.Window.Main.Tab.RoadGuiParts
{
    /// <summary>
    /// RoadNetwork_EditPanel.uxmlのバインドや挙動の制御を行うクラス
    /// </summary>
    public class RoadEditPanel : RoadAdjustGuiPartBase
    {
        static readonly string name = "RoadNetwork_EditPanel";
        GameObject selfGameObject = null;

        public RoadEditPanel() : base(name)
        {
        }

        public override void Init(VisualElement root)
        {
            base.Init(root);

            var rnMdl = GameObject.FindObjectOfType<PLATEAURnStructureModel>();
            if (rnMdl == null)
            {
                Debug.LogError("RoadNetworkStructureModel is not found.");
                return;
            }
            selfGameObject = rnMdl.gameObject;

            // 初期値の設定

            // ボタン類の設定
            var editModeToggle = self.Q<Toggle>("EditModeButton");
            if (editModeToggle == null)
            {
                Debug.LogError("EditModeToggle is not found.");
                return;
            }
            editModeToggle.RegisterCallback<ChangeEvent<bool>>((evt) =>
            {
                if (evt.newValue)
                {

                }
                else
                {

                }
            });

            var applyRoadButton = self.Q<Button>("ApplyRoadButton");
            if (applyRoadButton == null)
            {
                Debug.LogError("ApplyRoadButton is not found.");
                return;
            }
            applyRoadButton.clicked += () =>
            {

            };

            var applyIntersectionButton = self.Q<Button>("ApplyIntersectionButton");
            if (applyIntersectionButton == null)
            {
                Debug.LogError("ApplyIntersectionButton is not found.");
                return;
            }
            applyIntersectionButton.clicked += () =>
            {

            };
        }

        public override void Terminate(VisualElement root)
        {
            base.Terminate(root);
        }
    }
}
