using PLATEAU.RoadNetwork.Structure;
using System;

namespace PLATEAU.Editor.RoadNetwork.EditingSystem
{
    /// <summary>
    /// 内部システムが利用するインターフェイス
    /// 内部システム同士が連携する時や共通データにアクセスする際に利用する
    /// </summary>
    internal interface IRoadNetworkEditingSystem
    {
        // 仮
        RoadNetworkSceneGUISystem SceneGUISystem { get; }

        /// <summary>
        /// 編集機能のインスタンス
        /// </summary>
        RoadNetworkEditingSystem.ISystemInstance Instance { get; }

        bool EnableLimitSceneViewDefaultControl { get; set; }

        /// <summary>
        /// 道路ネットワークを所持したUnityオブジェクト
        /// </summary>
        UnityEngine.Object RoadNetworkObject { get; set; }

        event EventHandler OnChangedRoadNetworkObject;

        /// <summary>
        /// 道路ネットワーク
        /// </summary>
        RnModel RoadNetwork { get; }

        //HashSet<LinkGroupEditorData> Connections { get; }

        /// <summary>
        /// 現在の編集モード
        /// </summary>
        RoadNetworkEditMode CurrentEditMode { get; set; }

        event EventHandler OnChangedEditMode;

        /// <summary>
        /// 編集機能を提供するインターフェイス
        /// </summary>
        IRoadNetworkEditOperation EditOperation { get; }

        /// <summary>
        /// 選択中の道路ネットワーク要素
        /// </summary>
        System.Object SelectedRoadNetworkElement { get; set; }

        event Action OnChangedSelectRoadNetworkElement;

        /// <summary>
        /// 選択中の信号制御器のパターン
        /// </summary>
        TrafficSignalControllerPattern SelectedSignalControllerPattern { get; set; }

        event EventHandler OnChangedSignalControllerPattern;

        /// <summary>
        /// 選択中の信号制御器のパターンのフェーズ
        /// </summary>
        TrafficSignalControllerPhase SelectedSignalPhase { get; set; }

        event EventHandler OnChangedSignalControllerPhase;

        /// <summary>
        /// 道路ネットワークを所持したオブジェクトに変更があったことをUnityEditorに伝える
        /// </summary>
        void NotifyChangedRoadNetworkObject2Editor();

        RoadNetworkSimpleEditSysModule RoadNetworkSimpleEditModule { get; }
    }
}