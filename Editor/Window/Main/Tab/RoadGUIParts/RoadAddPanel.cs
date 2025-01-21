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

        private readonly Button intersectionAddStartButton;
        private readonly Button intersectionAddEndButton;

        public RoadAddPanel(VisualElement rootVisualElement) : base(name, rootVisualElement)
        {
            roadAddStartButton = rootVisualElement.Q<Button>("RoadAddStartButton");
            roadAddEndButton = rootVisualElement.Q<Button>("RoadAddEndButton");

            roadAddStartButton.clicked += OnRoadAddStartButtonClicked;
            roadAddEndButton.clicked += OnRoadAddEndButtonClicked;

            intersectionAddStartButton = rootVisualElement.Q<Button>("IntersectionAddStartButton");
            intersectionAddEndButton = rootVisualElement.Q<Button>("IntersectionAddEndButton");

            intersectionAddStartButton.clicked += OnIntersectionAddStartButtonClicked;
            intersectionAddEndButton.clicked += OnIntersectionAddEndButtonClicked;
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
            UpdateIntersectionAddButtonVisual(false);
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

            RoadNetworkAddSystem.Active.RoadAddSystem.Deactivate();
        }

        private void UpdateRoadAddButtonVisual(bool isActive)
        {
            ToggleDisplay(!isActive, roadAddStartButton);
            ToggleDisplay(isActive, roadAddEndButton);
        }

        private void OnIntersectionAddStartButtonClicked()
        {
            UpdateIntersectionAddButtonVisual(true);

            RoadNetworkAddSystem.Active.IntersectionAddSystem.Activate();
        }

        private void OnIntersectionAddEndButtonClicked()
        {
            UpdateIntersectionAddButtonVisual(false);

            RoadNetworkAddSystem.Active.IntersectionAddSystem.Deactivate();
        }

        private void UpdateIntersectionAddButtonVisual(bool isActive)
        {
            ToggleDisplay(!isActive, intersectionAddStartButton);
            ToggleDisplay(isActive, intersectionAddEndButton);
        }

        private static void ToggleDisplay(bool isActive, VisualElement element)
        {
            element.style.display = isActive ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}
