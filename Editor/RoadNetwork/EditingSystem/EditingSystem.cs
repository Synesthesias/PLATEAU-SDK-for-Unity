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
            
            
            public EditingSystem(RoadNetworkEditingSystem system)
            {
                Assert.IsNotNull(system);
                this.system = system;
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
                    system.selectedSignalPhase = value;
                }
            }


            public bool EnableLimitSceneViewDefaultControl
            {
                get => system.EditTargetSelectButton.EnableLimitSceneViewDefaultContorl;
                set => system.EditTargetSelectButton.EnableLimitSceneViewDefaultContorl = value;
            }

            

        }
}