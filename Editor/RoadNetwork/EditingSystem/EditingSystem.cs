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
        internal class EditingSystem
        {
            private readonly RoadNetworkEditingSystem system;
            public event Action OnChangedSelectRoadNetworkElement;
            
            
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
                }

            }

            public RnModel RoadNetwork
            {
                get => system.roadNetworkModel;
            }
            
            
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
            

            public TrafficSignalControllerPattern SelectedSignalControllerPattern
            {
                get => system.selectedSignalPattern;
                set
                {
                    if (system.selectedSignalPattern == value)
                        return;
                    system.selectedSignalPattern = value;

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
                }
            }


            public RoadNetworkEditSceneViewGui EditSceneViewGui => system.editSceneViewGui;

            public RoadNetworkEditTargetSelectButton EditTargetSelectButton => system.EditTargetSelectButton;

            public bool EnableLimitSceneViewDefaultControl
            {
                get => system.EditTargetSelectButton.EnableLimitSceneViewDefaultContorl;
                set => system.EditTargetSelectButton.EnableLimitSceneViewDefaultContorl = value;
            }

            public void NotifyChangedRoadNetworkObject2Editor()
            {
                if (system.roadNetworkObject == null)
                    return;
                EditorUtility.SetDirty(system.roadNetworkObject);

            }

        }
}