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
    /// <summary>RoadAdjustGuiの子要素としての基底クラス
    /// 共有インターフェイス、共通処理はここに記載する
    /// </summary>
    public abstract class RoadAdjustGuiPartBase
    {
        // 対応するVisualElementのキー
        private readonly string rootKey;
        protected VisualElement self { get; private set; }

        protected RoadAdjustGuiPartBase(string name)
        {
            rootKey = name;
        }

        /// <summary>
        /// 「道路調整」タブ内の各子タブ「生成」「編集」「追加」が選択された時に呼ばれます
        /// </summary>
        public void OnRoadChildTabSelected(VisualElement root)
        {
            self = GetRoot(root);
            if (self == null)
                return;
            self.style.display = DisplayStyle.Flex;

            OnTabSelected(self);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="root"></param>
        public void InitUXMLState(VisualElement root)
        {
            var s = GetRoot(root);
            if (s == null)
                return;
            s.style.display = DisplayStyle.None;
        }

        /// <summary>
        /// タブが選択された時の初期化処理をサブクラスで記述できるようにします。
        /// 終了処理がされていない状態での呼び出しも考慮する
        /// </summary>
        protected virtual void OnTabSelected(VisualElement root)
        {
        }

        /// <summary>
        /// 道路調整タブの各子タブ「生成」「編集」「追加」の選択が解除された時に呼ばれます。
        /// 終了処理を行います。
        /// 初期化されていない状態での呼び出しも考慮します。
        /// </summary>
        public void OnRoadChildTabUnselected(VisualElement root)
        {
            if (root.Contains(self) == false)
                return;

            self.style.display = DisplayStyle.None;

            OnTabUnselected(self);
            self = null;
        }
        
        /// <summary>
        /// タブの選択が解除されたときの終了処理をサブクラスで記述できるようにします。
        /// </summary>
        protected virtual void OnTabUnselected(VisualElement root)
        {
        }

        /// <summary>
        /// 該当する
        /// </summary>
        /// <param name="root"></param>
        /// <returns></returns>
        private VisualElement GetRoot(VisualElement root)
        {
            return root.Q<VisualElement>(rootKey);
        }

        /// <summary>
        /// 値を取得する
        /// </summary>
        /// <typeparam name="_Type"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        protected _Type Get<_Type>(string key)
                    where _Type : VisualElement
        {
            var v = self.Q<_Type>(key);
            if (v == null)
            {
                Debug.LogError($"Can't find {key}");
            }
            return v;
        }

        protected IntegerField GetI(string key)
        {
            return Get<IntegerField>(key);
        }

        protected FloatField GetF(string key)
        {
            return Get<FloatField>(key);
        }

        protected Toggle GetT(string key)
        {
            return Get<Toggle>(key);
        }

    }

    /// <summary>
    /// RoadNetwork_GeneratePanel.uxmlのバインドや挙動の制御を行うクラス
    /// </summary>
    public class RoadGenerate : RoadAdjustGuiPartBase
    {
        static readonly string name = "RoadNetwork_GeneratePanel";
        
        private Action setupMethod;

        public RoadGenerate() : base(name)
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
                var target = new RnmTargetModel(model);
                

                
                // 道路の白線、停止線などを生成します。
                GenerateRoadMarking(target);
                
                // 道路ネットワークに沿った道路メッシュを作成します。
                GenerateRoadMesh(target);

            };
        }

        /// <summary>
        /// 道路の白線、停止線などを生成します。
        /// </summary>
        private void GenerateRoadMarking(IRnmTarget target)
        {
            if (target == null)
            {
                Debug.LogError("道路ネットワークがnullです。");
            }
            var generator = new RoadMarkingGenerator(target);
            generator.Generate();
        }

        /// <summary>
        /// 道路ネットワークに沿った道路メッシュを作成します。
        /// </summary>
        private void GenerateRoadMesh(IRnmTarget target)
        {
            if (target == null)
            {
                Debug.LogError("道路ネットワークがありません。");
                return;
            }

            // 「車線をまとめる」「車線ごとに分ける」の選択肢は、現状はまとめるのみ対応するとします。
            // 分けたあとにまとめることが現状動かないためです。
            
            // var splitStr = Get<DropdownField>("Split_Pedestrian").value;
            // var separateType = splitStr switch
            // {
            //     "車線をまとめる" => RnmLineSeparateType.Combine,
            //     "車線ごとに分ける" => RnmLineSeparateType.Separate,
            //     _ => throw new ArgumentOutOfRangeException()
            // };
            new RoadNetworkToMesh(target, RnmLineSeparateType.Combine).Generate();
        }

        protected override void OnTabUnselected(VisualElement root)
        {
            var generateButton = self.Q<Button>("GenerateButton");
            if (generateButton != null)
            {
                generateButton.clicked -= setupMethod;
                setupMethod = null;
            }
        

            base.OnTabUnselected(root);
        }


    }
}
