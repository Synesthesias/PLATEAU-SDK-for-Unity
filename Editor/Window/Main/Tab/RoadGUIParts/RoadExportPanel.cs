using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace PLATEAU.Editor.Window.Main.Tab.RoadGuiParts
{
    /// <summary>
    /// RoadNetwork_ExportPanel.uxmlのバインドや挙動の制御を行うクラス
    /// </summary>
    public class RoadExportPanel : RoadAdjustGuiPartBase
    {
        private static readonly string name = "RoadNetwork_ExportPanel";

        private Action exportMethod;
        private Action blowseMethod;

        private string exportPath;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="rootVisualElement"></param>
        public RoadExportPanel(VisualElement rootVisualElement) : base(name, rootVisualElement)
        {
        }

        /// <summary>
        /// タブが選択された時の処理
        /// </summary>
        /// <param name="root"></param>
        protected override void OnTabSelected(VisualElement root)
        {
            base.OnTabSelected(root);

            var blowseButton = self.Q<Button>("ButtonBrowse");
            if (blowseButton == null)
            {
                Debug.LogError("Failed to load ButtonBrowse");
                return;
            }

            var exportButton = self.Q<Button>("ButtonExport");
            if (exportButton == null)
            {
                Debug.LogError("Failed to load ButtonExport");
                return;
            }

            blowseMethod = CreateBlowseMethod();
            blowseButton.clicked += blowseMethod;

            exportMethod = CreateExportMethod();
            exportButton.clicked += exportMethod;
        }

        /// <summary>
        /// エクスポートボタンを押したときの挙動を定義します
        /// </summary>
        private Action CreateExportMethod()
        {
            return () =>
            {
                var exporter = new PLATEAU.Editor.RoadNetwork.Exporter.RoadNetworkExporter();

                exporter.ExportRoadNetwork(exportPath);
            };
        }

        /// <summary>
        /// 参照ボタンを押したときの挙動を定義します
        /// </summary>
        /// <returns></returns>
        private Action CreateBlowseMethod()
        {
            return () =>
            {
                exportPath = UnityEditor.EditorUtility.OpenFolderPanel("Select Export Folder", exportPath, "");

                var exportPathField = self.Q<TextField>("ExportPathField");

                if (exportPathField == null)
                {
                    Debug.LogError("Failed to load ExportPathField");

                    return;
                }

                exportPathField.value = exportPath;
            };
        }

        /// <summary>
        /// タブが非選択になった時の処理
        /// </summary>
        protected override void OnTabUnselected()
        {
            var exportButton = self.Q<Button>("ButtonExport");
            if (exportButton != null)
            {
                exportButton.clicked -= exportMethod;
                exportMethod = null;
            }

            var blowseButton = self.Q<Button>("ButtonBrowse");
            if (blowseButton != null)
            {
                blowseButton.clicked -= blowseMethod;
                blowseMethod = null;
            }

            base.OnTabUnselected();
        }
    }
}