using PLATEAU.Util;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.UIElements;

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

            generateButton.clicked += () =>
            {
                var roadSize = GetI("RoadSizeField").value;
                var sudeWalklSize = GetI("RoadSizeField").value;
                var lodSideWalk = GetT("Add_Lod_Side_Walk").value;
                var checkMedian = GetT("Check_Median").value;

                var cellSize = GetI("Merge_Cell_Size").value;
                var cellLen = GetI("Merge_Cell_Length").value;
                var midPointTolerrance = GetI("Remove_Mid_Point_Tolerance").value;
                var allowEdgeAngle = GetI("Terminate_Allow_Edge_Angle").value;
                var ignoreHighway = GetT("Ignore_Highwaay").value;
 
                Debug.Log("GenerateButton clicked");

            };
        }

        public override void Terminate(VisualElement root)
        {
            base.Terminate(root);
        }


    }
}
