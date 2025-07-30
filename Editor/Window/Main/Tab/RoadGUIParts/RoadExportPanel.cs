using System.IO;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;

namespace PLATEAU.Editor.Window.Main.Tab.RoadGuiParts
{
    /// <summary>
    /// RoadNetwork_ExportPanel.uxmlのバインドや挙動の制御を行うクラス
    /// </summary>
    public class RoadExportPanel : RoadAdjustGuiPartBase
    {
        private static readonly string name = "RoadNetwork_ExportPanel";
        
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

            var exportPathField = self.Q<TextField>("ExportPathField");

            blowseButton.clicked += Browse;
            exportButton.clicked += Export;

            exportPathField.SetEnabled(false);
        }

        /// <summary>
        /// エクスポートボタンを押したときの挙動を定義します
        /// </summary>
        private void Export()
        {
            if (!Directory.Exists(exportPath))
            {
                PLATEAU.Util.Dialogue.Display("有効な出力先フォルダを指定してください", "OK");

                return;
            }

            var exporter = new PLATEAU.Editor.RoadNetwork.Exporter.RoadNetworkExporter();

            exporter.ExportRoadNetwork(exportPath);

            EditorUtility.RevealInFinder(exportPath + "/");
        }

        /// <summary>
        /// 参照ボタンを押したときの挙動を定義します
        /// </summary>
        /// <returns></returns>
        private void Browse()
        {
            exportPath = UnityEditor.EditorUtility.OpenFolderPanel("Select Export Folder", exportPath, "");

            var exportPathField = self.Q<TextField>("ExportPathField");

            if (exportPathField == null)
            {
                Debug.LogError("Failed to load ExportPathField");

                return;
            }

            exportPathField.value = exportPath;
        }

        /// <summary>
        /// タブが非選択になった時の処理
        /// </summary>
        protected override void OnTabUnselected()
        {
            var exportButton = self.Q<Button>("ButtonExport");
            if (exportButton != null)
            {
                exportButton.clicked -= Export;
            }

            var blowseButton = self.Q<Button>("ButtonBrowse");
            if (blowseButton != null)
            {
                blowseButton.clicked -= Browse;
            }

            base.OnTabUnselected();
        }
    }
}