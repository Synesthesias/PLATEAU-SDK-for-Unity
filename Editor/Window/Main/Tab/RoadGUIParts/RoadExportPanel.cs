using PLATEAU.RoadAdjust;
using PLATEAU.Util.Async;
using PLATEAU.RoadAdjust.RoadNetworkToMesh;
using PLATEAU.RoadNetwork.Structure;
using PLATEAU.RoadNetwork.Factory;
using UnityEngine;
using UnityEngine.UIElements;

// Testerを使わず生成するようにする
using PLATEAU.RoadNetwork.Tester;
using PLATEAU.Util;
using System;
using Object = UnityEngine.Object; // Todo 削除予定

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

        public RoadExportPanel(VisualElement rootVisualElement) : base(name, rootVisualElement)
        {
        }

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

            // 生成ボタンを押した時の挙動
            blowseMethod = CreateBlowseMethod();
            blowseButton.clicked += blowseMethod;

            exportMethod = CreateExportMethod();
            exportButton.clicked += exportMethod;
        }

        /// <summary>
        /// 生成ボタンを押したときの挙動を定義します。
        /// </summary>
        private Action CreateExportMethod()
        {
            return () =>
            {
                var exporter = new PLATEAU.Editor.RoadNetwork.Exporter.RoadNetworkExporter();

                exporter.ExportRoadNetwork(exportPath);
            };
        }

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