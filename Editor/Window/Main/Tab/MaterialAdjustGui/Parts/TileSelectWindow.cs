using PLATEAU.DynamicTile;
using PLATEAU.Util;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace PLATEAU.Editor.Window.Main.Tab.MaterialAdjustGui.Parts
{
    /// <summary>
    /// 都市モデルに含まれるパッケージ種を選択するウィンドウです。
    /// </summary>
    internal class TileSelectWindow : PlateauWindowBase
    {
        private IPackageSelectResultReceiver resultReceiver;
        private PackageSelectGui gui;
        private bool isInitialized;

        private PLATEAUTileManager tileManager;

        VisualElement root;

        public static TileSelectWindow Open(PLATEAUTileManager manager)
        {
            var window = GetWindow<TileSelectWindow>("タイル選択");
            window.Init(manager);
            window.Show();
            return window;
        }

        public void Init(PLATEAUTileManager manager)
        {
            this.tileManager = manager;
            this.isInitialized = true;
            DrawTileList(manager);
        }

        protected override VisualElementDisposable CreateGui()
        {
            //var container = new IMGUIContainer(() =>
            //{
            //    if (!isInitialized) return;
            //    PlateauEditorStyle.SetCurrentWindow(this);
            //    gui.Draw();
            //});

            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"{PathUtil.SdkBasePath}/Resources/PlateauUIDocument/DynamicTile/TileSelection.uxml");
            root = visualTree.CloneTree();
            rootVisualElement.Add(root);

            // イベント登録など
            var button = root.Q<Button>("Btn_Execute");
            button.clicked += () => Debug.Log("Button clicked!");

            var label = root.Q<Label>("Lbl_Info");

            return new VisualElementDisposable(root, Dispose);
        }

        private void DrawTileList(PLATEAUTileManager manager)
        {
            // ScrollViewの設定
            var viewElementTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"{PathUtil.SdkBasePath}/Resources/PlateauUIDocument/DynamicTile/TileName.uxml");
            var scrollView = root.Q<UnityEngine.UIElements.ScrollView>("TileScrollView");
            scrollView.contentContainer.Clear();
            foreach (var item in this.tileManager.DynamicTiles)
            {
                var elem = viewElementTree.CloneTree();
                var nameLabel = elem.Q<Label>("Lbl_TileName");
                nameLabel.text = item.Address;

                scrollView.contentContainer.Add(elem);
            }
        }

        private void Dispose()
        {
            gui?.Dispose();
        }

    }
}
