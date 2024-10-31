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
        protected VisualElement Gui { get; private set; }

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
            if (root.Contains(Gui))
            {
                Terminate(Gui);
            }
            
            Gui = CreateGUI();
            root.Add(Gui);
        }

        /// <summary>
        /// 終了処理
        /// 初期化されていない状態での呼び出しも考慮する
        /// </summary>
        /// <param name="root"></param>
        public virtual void Terminate(VisualElement root)
        {
            if (root.Contains(Gui) == false)
                return;

            root.Remove(Gui);
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
        }

        public override void Terminate(VisualElement root)
        {
            base.Terminate(root);
        }


    }
}
