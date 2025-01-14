using PLATEAU.RoadNetwork.Structure;
using System;
using UnityEditor;

namespace PLATEAU.Editor.RoadNetwork.EditingSystem
{
    /// <summary>
    /// 道路ネットワーク編集の対象を保持します。
    /// </summary>
    public class RoadNetworkEditTarget
    {
        // 対象の道路ネットワーク
        public PLATEAURnStructureModel RoadNetworkComponent { get; set; }
        
        // 選択中の道路ネットワーク要素 Road,Lane,Block...etc
        public System.Object selectedRoadNetworkElement;
        
        public event Action OnChangedSelectRoadNetworkElement;
        
        public RnModel RoadNetwork
        {
            get => RoadNetworkComponent.RoadNetwork;
        }
        
        
        
        public void SetDirty()
        {
            EditorUtility.SetDirty(RoadNetworkComponent);
        }
        
        public object SelectedRoadNetworkElement
        {
            get => selectedRoadNetworkElement;
            set
            {
                if (selectedRoadNetworkElement == value)
                    return;
                selectedRoadNetworkElement = value;
                OnChangedSelectRoadNetworkElement?.Invoke();
            }
        }
    }
}