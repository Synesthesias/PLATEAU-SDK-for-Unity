using PLATEAU.RoadNetwork.Structure;

namespace PLATEAU.Editor.RoadNetwork.EditingSystemSubMod
{
    /// <summary>
    /// 信号情報を編集します。
    /// 現在は使われていません。
    /// </summary>
    public class TrafficSignalEditor
    {
        // 選択中の信号制御器のパターン
        private TrafficSignalControllerPattern selectedSignalPattern;
        
        public TrafficSignalControllerPattern SelectedSignalControllerPattern
        {
            get => selectedSignalPattern;
            set
            {
                if (selectedSignalPattern == value)
                    return;
                selectedSignalPattern = value;

                SelectedSignalPhase = null;
            }
        }

        // 選択中の信号制御器のパターンのフェーズ
        public TrafficSignalControllerPhase SelectedSignalPhase { get; set; }
    }
}