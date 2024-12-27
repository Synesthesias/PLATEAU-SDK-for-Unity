using NUnit.Framework;
using PLATEAU.RoadNetwork.Data;
using PLATEAU.RoadNetwork.Structure;
using System;
using System.Collections.Generic;
using UnityEditor;

namespace PLATEAU.Editor.RoadNetwork.EditingSystem
{
        /// <summary>
        /// <see cref="RoadNetworkEditingSystem"/>内部システムが利用するインスタンス
        /// 内部システム同士が連携する時や共通データにアクセスする際に利用する
        /// </summary>
        internal class EditingSystem : IRoadNetworkEditingSystem
        {
            private readonly RoadNetworkEditingSystem system;
            public event EventHandler OnChangedEditMode;
            public event Action OnChangedSelectRoadNetworkElement;
            public event EventHandler OnChangedSignalControllerPattern;
            public event EventHandler OnChangedSignalControllerPhase;
            
            
            public EditingSystem(RoadNetworkEditingSystem system)
            {
                Assert.IsNotNull(system);
                this.system = system;
            }
            


            public UnityEngine.Object RoadNetworkObject
            {
                get => system.roadNetworkObject;
                set
                {
                    if (system.roadNetworkObject == value)
                        return;

                    var roadNetworkObj = value as IRoadNetworkObject;
                    Assert.IsNotNull(roadNetworkObj);
                    if (roadNetworkObj == null)
                        return;
                    var roadNetwork = roadNetworkObj.RoadNetwork;
                    if (roadNetwork == null)
                        return;

                    system.roadNetworkObject = value;
                    system.roadNetworkModel = roadNetwork;
                    OnChangedRoadNetworkObject?.Invoke(this, EventArgs.Empty);
                }

            }
            public event EventHandler OnChangedRoadNetworkObject;

            public RnModel RoadNetwork
            {
                get => system.roadNetworkModel;
            }

            public RoadNetworkEditMode CurrentEditMode
            {
                get => system.editingMode;
                set
                {
                    if (system.editingMode == value)
                        return;
                    system.editingMode = value;
                    OnChangedEditMode?.Invoke(this, EventArgs.Empty);
                }
            }
            

            public IRoadNetworkEditOperation EditOperation => system.editOperation;

            public object SelectedRoadNetworkElement
            {
                get => system.selectedRoadNetworkElement;
                set
                {
                    if (system.selectedRoadNetworkElement == value)
                        return;
                    system.selectedRoadNetworkElement = value;
                    OnChangedSelectRoadNetworkElement?.Invoke();
                }
            }

            public RoadNetworkEditingSystem.ISystemInstance Instance => system.systemInstance;

            public TrafficSignalControllerPattern SelectedSignalControllerPattern
            {
                get => system.selectedSignalPattern;
                set
                {
                    if (system.selectedSignalPattern == value)
                        return;
                    system.selectedSignalPattern = value;
                    OnChangedSignalControllerPattern?.Invoke(this, EventArgs.Empty);

                    system.selectedSignalPhase = null;
                }
            }

            public TrafficSignalControllerPhase SelectedSignalPhase
            {
                get => system.selectedSignalPhase;
                set
                {
                    if (system.selectedSignalPhase == value)
                        return;
                    system.selectedSignalPhase = value;
                    OnChangedSignalControllerPhase?.Invoke(this, EventArgs.Empty);
                }
            }

            public List<EditorData<RnRoadGroup>> Connections => system.editSceneViewGui?.Connections;

            public RoadNetworkEditSceneViewGui EditSceneViewGui => system.editSceneViewGui;

            public RoadNetworkSceneGUISystem SceneGUISystem => system.SceneGUISystem;

            public bool EnableLimitSceneViewDefaultControl
            {
                get => system.sceneGUISystem.EnableLimitSceneViewDefaultContorl;
                set => system.sceneGUISystem.EnableLimitSceneViewDefaultContorl = value;
            }

            public void NotifyChangedRoadNetworkObject2Editor()
            {
                if (system.roadNetworkObject == null)
                    return;
                EditorUtility.SetDirty(system.roadNetworkObject);

            }

        }
}