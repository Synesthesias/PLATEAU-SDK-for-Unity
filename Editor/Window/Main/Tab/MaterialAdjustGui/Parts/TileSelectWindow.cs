using PLATEAU.DynamicTile;
using PLATEAU.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace PLATEAU.Editor.Window.Main.Tab.MaterialAdjustGui.Parts
{
    /// <summary>
    /// 都市モデルに含まれるパッケージ種を選択するウィンドウです。
    /// </summary>
    internal class TileSelectWindow : PlateauWindowBase, IPackageSelectResultReceiver
    {
        private IPackageSelectResultReceiver resultReceiver;
        private PackageSelectGui gui;
        private bool isInitialized;

        private PLATEAUTileManager tileManager;

        private VisualElement root;
        private Button executeButton;
        private Button selectAllButton;
        private Button deselectAllButton;
        private Button fromSelectionButton;
        private Button fromPackageButton;
        private ScrollView scrollView;

        private List<TileNameElementContainer> tileNameElements;
        private Action<List<string>> onSelectCallback;

        public static TileSelectWindow Open(PLATEAUTileManager manager, Action<List<string>> callback)
        {
            var window = GetWindow<TileSelectWindow>("タイル選択");
            window.Init(manager, callback);
            window.Show();
            return window;
        }

        public void Init(PLATEAUTileManager manager, Action<List<string>> callback)
        {
            this.tileManager = manager;
            this.onSelectCallback = callback;
            this.isInitialized = true;
            DrawTileList(manager);
        }

        protected override VisualElementDisposable CreateGui()
        {
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"{PathUtil.SdkBasePath}/Resources/PlateauUIDocument/DynamicTile/TileSelection.uxml");
            root = visualTree.CloneTree();
            rootVisualElement.Add(root);
            executeButton = root.Q<Button>("Btn_Execute");
            selectAllButton = root.Q<Button>("Btn_SelectAll");
            deselectAllButton = root.Q<Button>("Btn_ClearAll");
            fromSelectionButton = root.Q<Button>("Btn_FromSelection");
            fromPackageButton = root.Q<Button>("Btn_FromPackage");

            // イベント登録
            executeButton.clicked += OnExecute;
            selectAllButton.clicked += SelectAll;
            deselectAllButton.clicked += DeselectAll;
            fromSelectionButton.clicked += AddFromSelection;
            fromPackageButton.clicked += AddFromPackage;
            Selection.selectionChanged += DrawNumSelection;
            DrawNumSelection();

            return new VisualElementDisposable(root, Dispose);
        }

        private void DrawTileList(PLATEAUTileManager manager)
        {
            if(manager == null || !isInitialized)
            {
                Debug.LogError("タイルマネージャーが初期化されていません。");
                return;
            }

            tileNameElements = new List<TileNameElementContainer>();

            // ScrollViewの設定
            var viewElementTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"{PathUtil.SdkBasePath}/Resources/PlateauUIDocument/DynamicTile/TileName.uxml");
            scrollView = root.Q<UnityEngine.UIElements.ScrollView>("TileScrollView");
            scrollView.contentContainer.Clear();
            foreach (var item in this.tileManager.DynamicTiles)
            {
                var elem = viewElementTree.CloneTree();
                var container = new TileNameElementContainer(elem);
                tileNameElements.Add(container);
                container.TileName = item.Address;
                scrollView.contentContainer.Add(elem);
            }
        }

        /// <summary>
        /// 全選択
        /// </summary>
        private void SelectAll()
        {
            tileNameElements.ForEach(e => e.IsSelected = true);
        }

        /// <summary>
        /// 全選択解除
        /// </summary>
        private void DeselectAll()
        {
            tileNameElements.ForEach(e => e.IsSelected = false);
        }

        /// <summary>
        /// 選択中のオブジェクトに対応するタイルを選択状態にする
        /// </summary>
        private void AddFromSelection()
        {
            var selectionNames = Selection.gameObjects.Select(x => x.transform.FindChildOfNamedParent(PLATEAUTileManager.TileParentName).name).ToList();
            tileNameElements.ForEach(e => e.IsSelected = selectionNames.Contains(e.TileName));
        }

        /// <summary>
        /// 選択中のオブジェクト数を表示
        /// </summary>
        private void DrawNumSelection()
        {
            var numSelection = Selection.gameObjects.Length.ToString();
            fromSelectionButton.text = $"選択中の{numSelection}個を追加";
        }

        /// <summary>
        /// パッケージ種選択ウィンドウを開き、選択されたパッケージ種に対応するタイルを選択状態にする
        /// 現状は、読込済みタイルのパッケージ種のみに対応。
        /// </summary>
        private void AddFromPackage()
        {
            var win = PackageSelectWindow.Open();
            win.Init(tileManager.CityModel, this);
        }

        /// <summary>
        /// パッケージ種選択ウィンドウからのコールバック
        /// </summary>
        /// <param name="result"></param>
        public void ReceivePackageSelectResult(PackageSelectResult result)
        {
            foreach(var package in result.SelectedDict)
            {
                if (!package.Value) continue;
                var tilesInPackage = tileManager.DynamicTiles.Where(t => t.Package == package.Key).Select(t => t.Address).ToList();
                tileNameElements.ForEach(e => e.IsSelected = tilesInPackage.Contains(e.TileName));
            }
        }

        /// <summary>
        /// 決定ボタン押下時の処理
        /// </summary>
        private void OnExecute()
        {
            // 選択されたタイルに基づいて処理を実行
            List<string> selectedAddresses = tileNameElements.Where(e => e.IsSelected)
                                               .Select(e => e.TileName)
                                               .ToList();
            onSelectCallback?.Invoke(selectedAddresses);

            // ウィンドウを閉じる
            this.Close();
        }

        private void Dispose()
        {
            if(executeButton != null)
                executeButton.clicked -= OnExecute;
            if (selectAllButton != null)
                selectAllButton.clicked -= SelectAll;
            if (deselectAllButton != null)
                deselectAllButton.clicked -= DeselectAll;
            if (fromSelectionButton != null)
                fromSelectionButton.clicked -= AddFromSelection;
            if (fromPackageButton != null)
                fromPackageButton.clicked -= AddFromPackage;

            gui?.Dispose();
        }
    }

    /// <summary>
    /// タイル名表示と選択用のコンテナ
    /// </summary>
    internal class TileNameElementContainer
    {
        private VisualElement root;
        private Toggle toggle;
        private Label nameLabel;

        public bool IsSelected
        {
            get => toggle.value;
            set => toggle.value = value;
        }

        public string TileName
        {
            get => nameLabel.text;
            set => nameLabel.text = value;
        }

        public TileNameElementContainer(VisualElement root)
        {
            this.root = root;
            this.toggle = root.Q<Toggle>("Tgl_Select");
            this.nameLabel = root.Q<Label>("Lbl_TileName");
            nameLabel.RegisterCallback<ClickEvent>(evt =>
            {
                toggle.value = !toggle.value;
            });
        }

    }
}
