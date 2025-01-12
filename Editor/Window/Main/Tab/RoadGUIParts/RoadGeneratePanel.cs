using PLATEAU.CityInfo;
using PLATEAU.RoadAdjust;
using PLATEAU.RoadAdjust.RoadMarking;
using PLATEAU.RoadAdjust.RoadNetworkToMesh;
using PLATEAU.RoadNetwork.Factory;
using PLATEAU.RoadNetwork.Structure;
using PLATEAU.RoadNetwork.Structure.Drawer;
// Testerを使わず生成するようにする
using PLATEAU.RoadNetwork.Tester;
using PLATEAU.Util;
using PLATEAU.Util.Async;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;
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
                factory = rnMdl.Factory ?? new RoadNetworkFactory();
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

        }

        /// <summary>
        /// 生成ボタンを押したときの挙動を定義します。
        /// </summary>
        private Action CreateSetupMethod()
        {
            return () =>
            {
                using var progressDisplay = new ProgressDisplayDialogue();
                progressDisplay.SetProgress("道路ネットワークを生成中", 5f, "");
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
                var crosswalkFreq = CrosswalkFreq(Get<DropdownField>("Crosswalk").index);

                var modelObject = Object.FindObjectOfType<PLATEAURnStructureModel>();
                if (modelObject == null)
                {
                    GameObject obj = new GameObject("RoadNetwork");
                    modelObject = obj.AddComponent<PLATEAURnStructureModel>();
                }

                if (modelObject.GetComponent<PLATEAURnStructureModel>() == null)
                {
                    modelObject.gameObject.AddComponent<PLATEAURnStructureModel>();
                }

                // 一応デバッグ描画コンポーネントもつけておく事になった
                if (modelObject.GetComponent<PLATEAURnModelDrawerDebug>() == null)
                    modelObject.gameObject.AddComponent<PLATEAURnModelDrawerDebug>();

                var factory = modelObject.Factory;
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
                var objects = GameObject.FindObjectsOfType<PLATEAUCityObjectGroup>()
                    .Where(cog => !RoadNetworkFactory.IsGeneratedRoad(cog.transform) && RoadNetworkFactory.IsConvertTarget(cog))
                    .Distinct()
                    .ToList();
                var model = factory.CreateRnModelAsync(factory.CreateRequest(objects, modelObject.gameObject)).ContinueWithErrorCatch().Result;

                // 道路の白線、停止線、道路メッシュを生成します。
                new RoadReproducer().Generate(new RrTargetModel(model), crosswalkFreq);

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


        /// <summary> 横断歩道のUIの選択肢IDをenumに変換します。 </summary>
        private CrosswalkFrequency CrosswalkFreq(int uiChoiceIndex)
        {
            switch (uiChoiceIndex)
            {
                case 0:
                    return CrosswalkFrequency.BigRoad;
                case 1:
                    return CrosswalkFrequency.All;
                case 2:
                    return CrosswalkFrequency.None;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
