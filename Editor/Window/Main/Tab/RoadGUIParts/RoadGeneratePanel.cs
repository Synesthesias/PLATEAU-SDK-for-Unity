using PLATEAU.Util;
using PLATEAU.Util.Async;
using PLATEAU.CityInfo;
using PLATEAU.CityGML;
using PLATEAU.RoadNetwork.Structure;
using PLATEAU.RoadNetwork.Factory;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

using static PLATEAU.RoadNetwork.Tester.PLATEAURoadNetworkTester;
using PLATEAU.RoadNetwork.Tester;
using PLATEAU.Editor.RoadNetwork.Tester;
using PLATEAU.Editor.RoadNetwork;
using PLATEAU.RoadNetwork.Structure.Drawer;   // Todo 削除予定


namespace PLATEAU.Editor.Window.Main.Tab.RoadGuiParts
{
    /// <summary>
    /// <summary>RoadAdjustGuiの子要素としての基底クラス
    /// 共有インターフェイス、共通処理はここに記載する
    /// </summary>
    public abstract class RoadAdjustGuiPartBase
    {
        private readonly string uxmlPath;
        protected VisualElement self { get; private set; }

        protected RoadAdjustGuiPartBase(string name)
        {
            uxmlPath = CreateUXMLFilePath(name);
        }

        /// <summary>
        /// 初期化
        /// 終了処理がされていない状態での呼び出しも考慮する
        /// </summary>
        /// <param name="root"></param>
        public virtual void Init(VisualElement root)
        {
            if (root.Contains(self))
            {
                Terminate(self);
            }
            
            self = CreateGUI();
            root.Add(self);
        }

        /// <summary>
        /// 終了処理
        /// 初期化されていない状態での呼び出しも考慮する
        /// </summary>
        /// <param name="root"></param>
        public virtual void Terminate(VisualElement root)
        {
            if (root.Contains(self) == false)
                return;

            root.Remove(self);
        }

        protected VisualElement CreateGUI()
        {
            var visualTree =
                AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(uxmlPath);
            if (visualTree == null)
            {
                Debug.LogError("Failed to load gui.");
            }

            return visualTree.CloneTree();
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

        /// <summary>
        /// ファイル名からパスを作成する
        /// ファイル名には拡張子、ディレクトリを除く
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private string CreateUXMLFilePath(string fileName)
        {
            return $"Packages/{PathUtil.packageFormalName}/Resources/PlateauUIDocument/RoadNetwork/{fileName}.uxml";
        }
    }

    /// <summary>
    /// RoadNetwork_GeneratePanel.uxmlのバインドや挙動の制御を行うクラス
    /// </summary>
    public class RoadGenerate : RoadAdjustGuiPartBase
    {
        static readonly string name = "RoadNetwork_GeneratePanel";

        GameObject selfGameObject = null;

        public RoadGenerate() : base(name)
        {
        }

        public override void Init(VisualElement root)
        {
            base.Init(root);

            var generateButton = self.Q<Button>("GenerateButton");
            if (generateButton == null)
            {
                Debug.LogError("Failed to load GenerateButton");
                return;
            }

            var rnMdl = GameObject.FindObjectOfType<PLATEAURnStructureModel>();
            if (rnMdl == null)
            {
                GameObject obj = new GameObject("RoadNetworkTester");
                obj.AddComponent<PLATEAURoadNetworkTester>();   // Todo 現状Testerが必要？
                obj.AddComponent<PLATEAURnStructureModel>();
                obj.AddComponent<PLATEAURnModelDrawerDebug>();
                rnMdl = obj.GetComponent<PLATEAURnStructureModel>();
            }


            selfGameObject = rnMdl.gameObject;
            var factory = rnMdl.GetComponent<PLATEAURoadNetworkTester>().Factory;


            // 初期値の設定
            // Todo (int)でキャストしているものはVisualElementの型が違うので修正依頼を行う
            GetF("RoadSizeField").value = factory.RoadSize;
            GetF("SideWalkSize").value = factory.Lod1SideWalkSize;
            GetT("Add_Lod_Side_Walk").value = factory.AddSideWalk;
            GetT("Check_Median").value = factory.CheckMedian;

            GetF("Merge_Cell_Size").value = factory.GraphFactory.mergeCellSize;
            GetI("Merge_Cell_Length").value = factory.GraphFactory.mergeCellLength;
            GetF("Remove_Mid_Point_Tolerance").value = factory.GraphFactory.removeMidPointTolerance;
            GetF("Terminate_Allow_Edge_Angle").value = factory.TerminateAllowEdgeAngle;
            GetT("Ignore_Highwaay").value = factory.IgnoreHighway;


            // 生成ボタンを押した時の挙動
            generateButton.clicked += () =>
            {
                var roadSize = GetF("RoadSizeField").value;
                var sideWalklSize = GetF("SideWalkSize").value;
                var lodSideWalk = GetT("Add_Lod_Side_Walk").value;
                var checkMedian = GetT("Check_Median").value;

                var cellSize = GetF("Merge_Cell_Size").value;
                var cellLen = GetI("Merge_Cell_Length").value;
                var midPointTolerrance = GetF("Remove_Mid_Point_Tolerance").value;
                var allowEdgeAngle = GetF("Terminate_Allow_Edge_Angle").value;
                var ignoreHighway = GetT("Ignore_Highwaay").value;

                selfGameObject = rnMdl.gameObject;
                factory.RoadSize = roadSize;
                factory.Lod1SideWalkSize = sideWalklSize;
                factory.AddSideWalk = lodSideWalk;
                factory.CheckMedian = checkMedian;

                factory.GraphFactory.mergeCellSize = cellSize;
                factory.GraphFactory.mergeCellLength = cellLen;
                factory.GraphFactory.removeMidPointTolerance = midPointTolerrance;
                factory.TerminateAllowEdgeAngle = allowEdgeAngle;
                factory.IgnoreHighway = ignoreHighway;

                //CreateNetwork().ContinueWithErrorCatch();
                selfGameObject.GetComponent<PLATEAURoadNetworkTester>().CreateNetwork().ContinueWithErrorCatch();


                Debug.Log("GenerateButton clicked");

            };

            /// <summary>
            /// 道路ネットワークを作成する
            /// </summary>
            /// <returns></returns>
            async Task CreateNetwork()
            {
                var go = selfGameObject;
                var targets = GetTargetCityObjects();
                var req = factory.CreateRequest(targets, go);
                await factory.CreateRnModelAsync(req);
            }

            List<PLATEAUCityObjectGroup> GetTargetCityObjects()
            {
                List<TestTargetPresets> TargetPresets;
                string TargetPresetName;
                const bool TargetAll = true;

                var ret = TargetAll
                    ? (IList<PLATEAUCityObjectGroup>)GameObject.FindObjectsOfType<PLATEAUCityObjectGroup>()
                    : TargetPresets
                        .FirstOrDefault(s => s.name == TargetPresetName)
                        ?.targets;
                if (ret == null)
                    return new List<PLATEAUCityObjectGroup>();

                return ret
                    .Where(c => c.transform.childCount == 0)
                    .Where(c => c.CityObjects.rootCityObjects.Any(a => a.CityObjectType == CityObjectType.COT_Road))
                    .Distinct()
                    .ToList();
            }

        }

        public override void Terminate(VisualElement root)
        {
            base.Terminate(root);
        }


    }
}
