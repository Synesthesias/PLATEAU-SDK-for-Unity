using PLATEAU.RoadAdjust;
using PLATEAU.Util.Async;
using PLATEAU.RoadAdjust.RoadMarking;
using PLATEAU.RoadAdjust.RoadNetworkToMesh;
using PLATEAU.RoadNetwork.Structure;
using PLATEAU.RoadNetwork.Factory;
using UnityEngine;
using UnityEngine.UIElements;
// Testerを使わず生成するようにする
using PLATEAU.RoadNetwork.Tester;
using System;
using Object = UnityEngine.Object; // Todo 削除予定

namespace PLATEAU.Editor.Window.Main.Tab.RoadGuiParts
{
    

    /// <summary>
    /// RoadNetwork_GeneratePanel.uxmlのバインドや挙動の制御を行うクラス
    /// </summary>
    public class RoadGeneratePanel : RoadAdjustGuiPartBase
    {
        static readonly string name = "RoadNetwork_GeneratePanel";
        
        private Action setupMethod;

        public RoadGeneratePanel(VisualElement rootVisualElement) : base(name, rootVisualElement)
        {
        }

        protected override void OnTabSelected(VisualElement root)
        {
            base.OnTabSelected(root);

            var generateButton = self.Q<Button>("GenerateButton");
            if (generateButton == null)
            {
                Debug.LogError("Failed to load GenerateButton");
                return;
            }

            var rnMdl = GameObject.FindObjectOfType<PLATEAURnStructureModel>();
            RoadNetworkFactory factory;
            if (rnMdl == null)
            {
                factory = new RoadNetworkFactory();
            }
            else
            {
                factory = rnMdl.GetComponent<PLATEAURoadNetworkTester>().Factory;
            }
            // 初期値の設定
            // Todo (int)でキャストしているものはVisualElementの型が違うので修正依頼を行う
            GetF("RoadSizeField").value = factory.RoadSize;
            GetF("SideWalkSize").value = factory.Lod1SideWalkSize;
            GetT("Add_Lod_Side_Walk").value = factory.AddSideWalk;
            GetT("Check_Median").value = factory.CheckMedian;
            GetT("Add_TrafficSignalLight").value = factory.AddTrafficSignalLights;

            GetF("Merge_Cell_Size").value = factory.GraphFactory.mergeCellSize;
            GetI("Merge_Cell_Length").value = factory.GraphFactory.mergeCellLength;
            GetF("Remove_Mid_Point_Tolerance").value = factory.GraphFactory.removeMidPointTolerance;
            GetF("Terminate_Allow_Edge_Angle").value = factory.TerminateAllowEdgeAngle;
            GetT("Ignore_Highwaay").value = factory.IgnoreHighway;
            

            
            


            // 生成ボタンを押した時の挙動
            setupMethod = CreateSetupMethod();
            generateButton.clicked += setupMethod;

            /// <summary>
            /// 道路ネットワークを作成する
            /// </summary>
            /// <returns></returns>
            // async Task CreateNetwork()
            // {
            //     var go = selfGameObject;
            //     var targets = GetTargetCityObjects();
            //     var req = factory.CreateRequest(targets, go);
            //     await factory.CreateRnModelAsync(req);
            // }

            // List<PLATEAUCityObjectGroup> GetTargetCityObjects()
            // {
            //     List<TestTargetPresets> TargetPresets;
            //     string TargetPresetName;
            //     const bool TargetAll = true;
            //
            //     var ret = TargetAll
            //         ? (IList<PLATEAUCityObjectGroup>)GameObject.FindObjectsOfType<PLATEAUCityObjectGroup>()
            //         : TargetPresets
            //             .FirstOrDefault(s => s.name == TargetPresetName)
            //             ?.targets;
            //     if (ret == null)
            //         return new List<PLATEAUCityObjectGroup>();
            //
            //     return ret
            //         .Where(c => c.transform.childCount == 0)
            //         .Where(c => c.CityObjects.rootCityObjects.Any(a => a.CityObjectType == CityObjectType.COT_Road))
            //         .Distinct()
            //         .ToList();
            // }

        }

        /// <summary>
        /// 生成ボタンを押したときの挙動を定義します。
        /// </summary>
        private Action CreateSetupMethod()
        {
            return () =>
            {
                // 道路ネットワークを生成します。
                var roadSize = GetF("RoadSizeField").value;
                var sideWalklSize = GetF("SideWalkSize").value;
                var lodSideWalk = GetT("Add_Lod_Side_Walk").value;
                var checkMedian = GetT("Check_Median").value;
                var addTrafficSignalLights = GetT("Add_TrafficSignalLight").value;

                var cellSize = GetF("Merge_Cell_Size").value;
                var cellLen = GetI("Merge_Cell_Length").value;
                var midPointTolerrance = GetF("Remove_Mid_Point_Tolerance").value;
                var allowEdgeAngle = GetF("Terminate_Allow_Edge_Angle").value;
                var ignoreHighway = GetT("Ignore_Highwaay").value;

                var tester = Object.FindObjectOfType<PLATEAURoadNetworkTester>();
                if (tester == null)
                {
                    GameObject obj = new GameObject("RoadNetworkTester");
                    tester = obj.AddComponent<PLATEAURoadNetworkTester>();
                    //obj.AddComponent<PLATEAURnModelDrawerDebug>();    // Testerで生成されるため
                }

                if (tester.GetComponent<PLATEAURnStructureModel>() == null)
                {
                    tester.gameObject.AddComponent<PLATEAURnStructureModel>();
                }
                
                var factory = tester.Factory;
                factory.RoadSize = roadSize;
                factory.Lod1SideWalkSize = sideWalklSize;
                factory.AddSideWalk = lodSideWalk;
                factory.CheckMedian = checkMedian;
                factory.AddTrafficSignalLights = addTrafficSignalLights;

                factory.GraphFactory.mergeCellSize = cellSize;
                factory.GraphFactory.mergeCellLength = cellLen;
                factory.GraphFactory.removeMidPointTolerance = midPointTolerrance;
                factory.TerminateAllowEdgeAngle = allowEdgeAngle;
                factory.IgnoreHighway = ignoreHighway;
                tester.Factory = factory;
                
                var model = tester.CreateNetwork().ContinueWithErrorCatch().Result;
                
                // 道路の白線、停止線、道路メッシュを生成します。
                new RoadReproducer().Generate(new RrTargetModel(model));

            };
        }
        

        protected override void OnTabUnselected()
        {
            var generateButton = self.Q<Button>("GenerateButton");
            if (generateButton != null)
            {
                generateButton.clicked -= setupMethod;
                setupMethod = null;
            }
        

            base.OnTabUnselected();
        }


    }
}
