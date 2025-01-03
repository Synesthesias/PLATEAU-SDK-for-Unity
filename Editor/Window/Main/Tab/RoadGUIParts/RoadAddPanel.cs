using PLATEAU.Editor.RoadNetwork;
using PLATEAU.Editor.RoadNetwork.EditingSystem;
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

        private readonly Button roadAddStartButton;
        private readonly Button roadAddEndButton;

        public RoadAddPanel(VisualElement rootVisualElement) : base(name, rootVisualElement)
        {
            roadAddStartButton = rootVisualElement.Q<Button>("RoadAddStartButton");
            roadAddEndButton = rootVisualElement.Q<Button>("RoadAddEndButton");

            roadAddStartButton.clicked += OnRoadAddStartButtonClicked;
            roadAddEndButton.clicked += OnRoadAddEndButtonClicked;

        }

        protected override void OnTabSelected(VisualElement root)
        {
            base.OnTabSelected(root);
            var initialized = RoadNetworkAddSystem.TryInitializeGlobal();
            if (!initialized)
            {
                Debug.LogError("Failed to initialize RoadNetworkAddSystem.");
                // TODO: RoadNetworkAddSystemの初期化に失敗した場合はUIを無効化する
            }

            UpdateRoadAddButtonVisual(false);
        }

        protected override void OnTabUnselected()
        {
            base.OnTabUnselected();

            RoadNetworkAddSystem.TerminateGlobal();
        }

        private void OnRoadAddStartButtonClicked()
        {
            UpdateRoadAddButtonVisual(true);

            RoadNetworkAddSystem.Active.RoadAddSystem.Activate();
        }

        private void OnRoadAddEndButtonClicked()
        {
            UpdateRoadAddButtonVisual(false);

            var system = RoadNetworkEditingSystem.SingletonInstance;
            RoadNetworkAddSystem.Active.RoadAddSystem.Deactivate();
        }

        private void UpdateRoadAddButtonVisual(bool isActive)
        {
            ToggleDisplay(!isActive, roadAddStartButton);
            ToggleDisplay(isActive, roadAddEndButton);
        }

        private static void ToggleDisplay(bool isActive, VisualElement element)
        {
            element.style.display = isActive ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}
