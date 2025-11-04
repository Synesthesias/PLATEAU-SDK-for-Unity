using PLATEAU.DynamicTile;
using PLATEAU.Util;
using PLATEAU.Util.Async;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace PLATEAU.Editor.Window.Main.Tab.MaterialAdjustGui.Parts
{
    /// <summary>
    /// タイル選択ウィンドウです。
    /// </summary>
    internal class TileSelectWindow : PlateauWindowBase, IPackageSelectResultReceiver
    {
        private bool isInitialized;

        private PLATEAUTileManager tileManager;
        private TreeViewItemBuilder treeViewItemBuilder;

        // UI要素
        private VisualElement root;
        private Button executeButton;
        private Button selectAllButton;
        private Button deselectAllButton;
        private Button tileFromSelectionButton;
        private Button fromSelectionButton;
        private Button fromPackageButton;
        private ScrollView scrollView;

        protected override bool UseScrollView => false; // スクロールビューを使用しない

        private List<TileNameElementContainer> tileNameElements; // スクロールバー内のタイル名要素リスト

        /// <summary>
        /// Windowから返されるタイル選択結果のコールバック
        /// </summary>
        private Action<TileSelectResult> onSelectCallback;
        

        /// <summary>
        /// Windowから返されるタイル選択結果
        /// </summary>
        internal class TileSelectResult
        {
            /// <summary>
            /// 選択されたタイル名リスト
            /// </summary>
            public List<string> Selection { get; set; }

            public List<TileSelectionItem> ToTileSelectionItems()
            {
                return Selection.Select(address => new TileSelectionItem(address)).ToList();
            }

            public TileSelectResult(List<string> selection)
            {
                Selection = selection;
            }
        }

        public static TileSelectWindow Open(PLATEAUTileManager manager, Action<TileSelectResult> callback)
        {
            var window = GetWindow<TileSelectWindow>("タイル選択");
            window.Init(manager, callback);
            window.Show();
            return window;
        }

        public void Init(PLATEAUTileManager manager, Action<TileSelectResult> callback)
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
            tileFromSelectionButton = root.Q<Button>("Btn_TileFromSelection");
            fromSelectionButton = root.Q<Button>("Btn_FromSelection");
            fromPackageButton = root.Q<Button>("Btn_FromPackage");

            // イベント登録
            executeButton.clicked += OnExecute;
            selectAllButton.clicked += SelectAll;
            deselectAllButton.clicked += DeselectAll;
            tileFromSelectionButton.clicked += AddTileFromSelection;
            fromSelectionButton.clicked += AddFromSelection;
            fromPackageButton.clicked += AddFromPackage;
            Selection.selectionChanged += DrawNumSelection;
            DrawNumSelection();

            return new VisualElementDisposable(root, Dispose);
        }

        /// <summary>
        /// TileManagerのDynamicTilesリストを表示
        /// </summary>
        /// <param name="manager"></param>
        private void DrawTileList(PLATEAUTileManager manager)
        {
            if(manager == null || !isInitialized)
            {
                Debug.LogError("タイルマネージャーが初期化されていません。");
                return;
            }

            // 既存リソースをクリーンアップ
            treeViewItemBuilder?.Dispose();
            if (tileNameElements != null)
            {
                foreach (var elem in tileNameElements)
                    elem.Dispose();
            }

            treeViewItemBuilder = new TreeViewItemBuilder(tileManager, this);
            tileNameElements = new List<TileNameElementContainer>();

            // ScrollViewの設定
            var viewElementTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"{PathUtil.SdkBasePath}/Resources/PlateauUIDocument/DynamicTile/TileName.uxml");
            scrollView = root.Q<UnityEngine.UIElements.ScrollView>("TileScrollView");
            scrollView.contentContainer.Clear();
            foreach (var item in this.tileManager.DynamicTiles)
            {
                var elem = viewElementTree.CloneTree();
                var container = new TileNameElementContainer(elem);
                container.SetOnSelectCallback(ShowTileHierarchy);
                tileNameElements.Add(container);
                container.TileName = item.Address;
                scrollView.contentContainer.Add(elem);
            }
        }

        /// <summary>
        /// ボタンの有効/無効を設定
        /// </summary>
        /// <param name="enabled"></param>
        private void SetButtonsEnabled(bool enabled)
        {
            selectAllButton?.SetEnabled(enabled);
            deselectAllButton?.SetEnabled(enabled);
            tileFromSelectionButton?.SetEnabled(enabled);
            fromSelectionButton?.SetEnabled(enabled);
            fromPackageButton?.SetEnabled(enabled);
            executeButton?.SetEnabled(enabled);
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
            tileNameElements.ForEach(e => { e.IsSelected = false; e.SetTreeViewState(TileNameElementContainer.TreeViewState.Hide); });
        }
     
        /// <summary>
        /// 選択中のオブジェクトに対応するタイルを選択状態にする
        /// </summary>
        private void AddTileFromSelection()
        {
            var gos = Selection.gameObjects ?? Array.Empty<GameObject>();
            var selectionNameSet = new HashSet<string>(
            gos.Select(x => x.transform.FindChildOfNamedParent(PLATEAUTileManager.TileParentName))
                       .Where(t => t != null)
                       .Select(t => t.name)
                );

            foreach (var e in tileNameElements)
            {
                if (!e.IsSelected && selectionNameSet.Contains(e.TileName))
                    e.IsSelected = true;
            }
        }

        /// <summary>
        /// 選択中のオブジェクトに対応するタイルの子要素を選択状態にする
        /// </summary>
        private void AddFromSelection()
        {
            AddFromSelectionAsync().ContinueWithErrorCatch();
        }

        private async Task AddFromSelectionAsync()
        {
            SetButtonsEnabled(false);
            await treeViewItemBuilder.AddFromSelectionAsync(tileNameElements);
            SetButtonsEnabled(true);
        }

        /// <summary>
        /// 選択中のオブジェクト数を表示
        /// </summary>
        private void DrawNumSelection()
        {
            var numSelection = (Selection.gameObjects?.Length ?? 0).ToString();
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
        /// タイルの子を選択するTreeViewを開く 
        /// タイルを読み込んでから表示するため非同期処理
        /// </summary>
        /// <param name="tileName"></param>
        private void ShowTileHierarchy(TileNameElementContainer elem)
        {
            treeViewItemBuilder.LoadAndDisyplayHierarchy(elem);
        }

        /// <summary>
        /// パッケージ種選択ウィンドウからのコールバック
        /// </summary>
        /// <param name="result"></param>
        public void ReceivePackageSelectResult(PackageSelectResult result)
        {
            var selectedTileAddresses = new HashSet<string>();
            foreach (var package in result.SelectedDict)
            {
                if (!package.Value) continue;
                var tilesInPackage = tileManager.DynamicTiles.Where(t => t.Package == package.Key).Select(t => t.Address).ToList();
                foreach (var address in tilesInPackage)
                {
                    selectedTileAddresses.Add(address);
                }
            }
            tileNameElements.ForEach(e =>
            {
                if (selectedTileAddresses.Contains(e.TileName)) e.IsSelected = true;
            });
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
            List<string> selectedChildAddresses = treeViewItemBuilder.GetSelectedChildrenPath(tileNameElements);
            selectedAddresses.AddRange(selectedChildAddresses);
            onSelectCallback?.Invoke(new TileSelectResult(selectedAddresses));

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
            if (tileFromSelectionButton != null)
                tileFromSelectionButton.clicked -= AddTileFromSelection;
            if (fromSelectionButton != null)
                fromSelectionButton.clicked -= AddFromSelection;
            if (fromPackageButton != null)
                fromPackageButton.clicked -= AddFromPackage;

            if(tileNameElements != null)
            {
                foreach (var elem in tileNameElements)
                    elem.Dispose();
                tileNameElements.Clear();
            }

            Selection.selectionChanged -= DrawNumSelection;

            treeViewItemBuilder?.Dispose();
        }
    }

    /// <summary>
    /// タイル名表示と選択用のコンテナ
    /// </summary>
    internal class TileNameElementContainer : IDisposable
    {
        internal enum TreeViewState
        {
            None,
            Show,
            Hide,
            Loading
        }

        private VisualElement root;
        private Toggle toggle;
        private Label nameLabel;
        private Button selectChildButton;

        private VisualElement tileNameContent;
        private TreeView treeView;
        private Action<TileNameElementContainer> OnSelectCallback;

        private List<VisualElement> treeViewElements;

        // TreeViewのUXMLリソースパス
        public const string TREEVIEW_UXML_RESOURCE_PATH = "Resources/PlateauUIDocument/DynamicTile/TileChildName.uxml";
        //TreeView要素名
        public const string TREEVIEW_TOGGLE = "Tgl_TreeSelect";
        public const string TREEVIEW_LABEL = "Lbl_TreeTileName";
        public const string TREEVIEW_BUTTON_HIDE = "Btn_TreeHide";

        public bool IsSelected
        {
            get => toggle.value;
            set
            {
                if (toggle.value == value) return;
                toggle.value = value;
            }
        }

        public string TileName
        {
            get => nameLabel.text;
            set => nameLabel.text = value;
        }

        public TreeView TreeView => treeView;
        public Button ButtonSelectChild => selectChildButton;

        public TileNameElementContainer(VisualElement root)
        {
            this.root = root;
            this.tileNameContent = root.Q<VisualElement>("TileNameContent");
            this.toggle = root.Q<Toggle>("Tgl_Select");
            this.treeView = root.Q<TreeView>("Tree_Children");
            this.nameLabel = root.Q<Label>("Lbl_TileName");
            this.selectChildButton = root.Q<Button>("Btn_SelectChild");
            this.toggle.RegisterValueChangedCallback(OnToggleValueChanged);
            this.nameLabel.RegisterCallback<ClickEvent>(ToggleSelection);    
            this.selectChildButton.clicked += OnSelectChild;
        }

        /// <summary>
        /// Tile/ Tile内子要素TreeViewの表示を切り替えます。
        /// </summary>
        /// <param name="state"></param>
        public void SetTreeViewState(TreeViewState state)
        {
            selectChildButton.text = state switch
            {
                TreeViewState.Loading => "読み込み中...",
                _ => "階層を表示",
            };
            treeView.style.display = state switch
            {
                TreeViewState.Show => DisplayStyle.Flex,
                _ => DisplayStyle.None,
            };
            tileNameContent.style.display = state switch
            {
                TreeViewState.Show => DisplayStyle.None,
                _ => DisplayStyle.Flex,
            };
        }

        /// <summary>
        /// 外部参照用：TreeViewのアイテムデータ
        /// </summary>
        public List<TreeViewItemData<TreeViewItemBuilder.TransformTreeItem>> treeViewItemData { get; set; }

        /// <summary>
        /// TreeView内の子要素選択を再帰的にクリアします。
        /// </summary>
        /// <param name="item"></param>
        private void ClearChildSelectionRecursive(TreeViewItemData<TreeViewItemBuilder.TransformTreeItem> item)
        {
            item.data.IsSelected = false;
            foreach (var child in item.children)
            {
                ClearChildSelectionRecursive(child);
            }
        }

        /// <summary>
        /// treeView.makeItemに渡す要素生成処理
        /// </summary>
        /// <returns></returns>
        public Func<VisualElement> GetTreeViewMakeItem()
        {
            var tileChildVisualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"{PathUtil.SdkBasePath}/{TREEVIEW_UXML_RESOURCE_PATH}");
            return () => tileChildVisualTree.CloneTree();
        }

        /// <summary>
        /// View要素生成処理
        /// </summary>
        /// <returns></returns>
        public Action<VisualElement,int> GetTreeViewBindItem()
        {
            return (element, index) =>
            {
                var treeItem = treeView.GetItemDataForIndex<TreeViewItemBuilder.TransformTreeItem>(index);
                var treeLabel = element.Q<Label>(TREEVIEW_LABEL);
                var treeToggle = element.Q<Toggle>(TREEVIEW_TOGGLE);
                var treeHideButton = element.Q<Button>(TREEVIEW_BUTTON_HIDE);

                // 既存のコールバックをクリア（リサイクル対策）
                if (element.userData is System.Action cleanup)
                    cleanup();
                
                treeLabel.text = treeItem.name;
                //label.text = treeItem.GetFullPath(); // Debug用にフルパス表示

                // データモデル→UIへ同期
                treeToggle.SetValueWithoutNotify(treeItem.IsSelected);

                // 新しいコールバックを登録
                treeLabel.RegisterCallback<ClickEvent>(OnElementClick);
                treeToggle.RegisterValueChangedCallback(OnToggleChanged);

                // トップのみボタン表示
                if (treeItem.parent != null)
                {
                    treeHideButton.visible = false;
                }
                else
                {
                    // 親タイルの選択状態をツリーのルートアイテムに反映
                    treeToggle.SetValueWithoutNotify(toggle.value);
                    treeItem.IsSelected = treeToggle.value;
                    treeHideButton.visible = true;
                    treeHideButton.clicked += OnTreeHideClick;
                }
                
                void OnElementClick(ClickEvent evt)
                {
                    var v = !treeToggle.value;
                    treeToggle.value = v;
                    evt.StopPropagation();
                }

                void OnToggleChanged(ChangeEvent<bool> evt)
                {
                    treeItem.IsSelected = evt.newValue; // UI→データモデルへ
                }

                void OnTreeHideClick()
                {
                    SetTreeViewState(TreeViewState.Hide);
                    toggle.value = treeToggle.value;
                }
                
                // クリーンアップ関数を保存
                element.userData = (System.Action)(() =>
                {
                    treeLabel.UnregisterCallback<ClickEvent>(OnElementClick);
                    treeToggle.UnregisterValueChangedCallback(OnToggleChanged);
                    treeHideButton.clicked -= OnTreeHideClick;
                });
            };
        }

        /// <summary>
        /// 子要素選択時の処理を外部で行うためのコールバック設定
        /// </summary>
        /// <param name="callback"></param>
        public void SetOnSelectCallback(Action<TileNameElementContainer> callback)
        {
            OnSelectCallback = callback;
        }

        /// <summary>
        /// ラベルクリック時の選択トグル
        /// </summary>
        /// <param name="evt"></param>
        private void ToggleSelection(ClickEvent evt)
        {
            IsSelected = !IsSelected;
        }

        /// <summary>
        /// Toggle値変更時の処理
        /// </summary>
        /// <param name="evt"></param>
        private void OnToggleValueChanged(ChangeEvent<bool> evt)
        {
            if (evt.newValue)
            {
                SetTreeViewState(TreeViewState.Hide);
                // 親が選択された場合、子要素の選択をクリア
                if (treeViewItemData != null)
                {
                    foreach (var item in treeViewItemData)
                    {
                        ClearChildSelectionRecursive(item);
                    }
                }
            }
        }

        /// <summary>
        /// 子要素選択ボタン押下時の処理
        /// </summary>
        private void OnSelectChild()
        {
            if (treeView.style.display == DisplayStyle.Flex)
            {
                SetTreeViewState(TreeViewState.Hide);
            }
            else
            {
                SetTreeViewState(TreeViewState.Show);
                OnSelectCallback?.Invoke(this);
            }
        }

        public void Dispose()
        {
            if (toggle != null)
                toggle.UnregisterValueChangedCallback(OnToggleValueChanged);

            if (nameLabel != null)
                nameLabel.UnregisterCallback<ClickEvent>(ToggleSelection);

            if (selectChildButton != null)
                selectChildButton.clicked -= OnSelectChild;

            OnSelectCallback = null;
        }
    }

    /// <summary>
    /// TreeView描画処理構築用クラス
    /// </summary>
    internal class TreeViewItemBuilder : IDisposable
    {
        private PLATEAUTileManager tileManager;
        private EditorWindow parentWindow;
        private int nextId = 0;
        private CancellationTokenSource cts;

        public TreeViewItemBuilder(PLATEAUTileManager manager, EditorWindow window)
        {
            this.tileManager = manager;
            this.parentWindow = window;
        }

        public void LoadAndDisyplayHierarchy(TileNameElementContainer container)
        {
            LoadAndDisyplayHierarchyAsync(container).ContinueWithErrorCatch();
        }

        public async Task LoadAndDisyplayHierarchyAsync(TileNameElementContainer container)
        {
            if (cts != null)
            {
                cts.Cancel();
                cts.Dispose();
            }
            var localCts = new CancellationTokenSource();
            cts = localCts;

            var treeView = container.TreeView;
            container.SetTreeViewState(TileNameElementContainer.TreeViewState.Loading);

            try
            {
                var addresses = new List<string>() { container.TileName };
                var loadedTiles = await tileManager.ForceLoadTiles(addresses, localCts.Token);

                await Task.Delay(100); // UI更新のため少し待機
                await Task.Yield();

                foreach (var loadedTile in loadedTiles)
                {
                    if (loadedTile?.LoadedObject != null)
                    {
                        treeView.makeItem = container.GetTreeViewMakeItem();
                        treeView.bindItem = container.GetTreeViewBindItem();
                        nextId = 0;
                        var rootItem = BuildTree(loadedTile.LoadedObject.transform, null);
                        List<TreeViewItemData<TransformTreeItem>> rootItems = new();
                        rootItems.Add(BuildTreeItem(rootItem));
                        treeView.SetRootItems(rootItems);
                        treeView.ExpandAll();
                        treeView.style.display = DisplayStyle.Flex;
                        container.treeViewItemData = rootItems;
                        container.SetTreeViewState(TileNameElementContainer.TreeViewState.Show);
                        parentWindow.Repaint();
                    }
                }
                
            }
            catch (OperationCanceledException)
            {
                Debug.LogWarning("LoadAndDisyplayHierarchy was canceled.");
                container.SetTreeViewState(TileNameElementContainer.TreeViewState.Hide);
            }
            finally
            {
                localCts?.Dispose();
                if (cts == localCts)
                    cts = null;
            }
        }

        /// <summary>
        /// 選択中のオブジェクトに対応するタイルの子要素を選択状態にする
        /// </summary>
        /// <param name="tileNameElements"></param>
        /// <returns></returns>
        public async Task AddFromSelectionAsync(List<TileNameElementContainer> tileNameElements)
        {
            var gos = Selection.gameObjects ?? Array.Empty<GameObject>();
            var tileNames = gos
                    .Select(x => (t: x.transform.FindChildOfNamedParent(PLATEAUTileManager.TileParentName), x.transform))
                    .Where(p => p.t != null)
                    .Select(p => (p.t.name, p.transform))
                    .ToList();

            foreach (var elem in tileNameElements)
            {
                if (elem == null) continue;

                var matches = tileNames.Where(item => item.name == elem.TileName).ToList();
                if(matches.Count == 0) continue;

                await LoadAndDisyplayHierarchyAsync(elem);
                await Task.Yield();
                await Task.Delay(100); // UI更新のため少し待機

                if (elem.treeViewItemData == null) continue;

                foreach (var match in matches)
                {
                    var path = match.transform.GetPathToParent(match.name);
                    if (path == null)
                    {
                        Debug.LogWarning($"親タイル {match.name} が見つかりませんでした。");
                        continue;
                    }
                    // 子要素のうち、選択中のオブジェクトに対応するものを選択状態にする
                    foreach (var item in elem.treeViewItemData)
                    {
                        SelectChildRecursive(item, path, elem);
                    }
                }
            }
        }

        private void SelectChildRecursive(TreeViewItemData<TransformTreeItem> item, string pathToSelect, TileNameElementContainer container)
        {
            var isSelected = string.Equals(pathToSelect, item.data.GetFullPath());
            if (isSelected)
            {
                item.data.IsSelected = isSelected;
                // UI同期
                var visualElem = container.TreeView.GetRootElementForId(item.data.id);
                var toggle = visualElem?.Q<Toggle>(TileNameElementContainer.TREEVIEW_TOGGLE);
                toggle?.SetValueWithoutNotify(true);
                container.IsSelected = false; // 親タイルは解除

            }
            foreach (var childItem in item.children)
            {
                SelectChildRecursive(childItem, pathToSelect, container);
            }
        }

        /// <summary>
        /// 選択された子要素のパスを取得
        /// </summary>
        /// <param name="tileNameElements"></param>
        /// <returns></returns>
        public List<string> GetSelectedChildrenPath(List<TileNameElementContainer> tileNameElements)
        {
            List<string> selected = new();
            foreach (var elem in tileNameElements)
            {
                if (elem.treeViewItemData == null) continue;
                foreach (var item in elem.treeViewItemData)
                {
                    GetSelectedChildrenPathRecursive(item, elem, selected);
                }      
            }
            return selected;
        }

        // 上の処理の再帰部分
        public void GetSelectedChildrenPathRecursive(TreeViewItemData<TransformTreeItem> item, TileNameElementContainer container, List<string> selected)
        {
            bool isRootAndParentSelected = item.data.parent == null && container.IsSelected;
            if (item.data.IsSelected && !isRootAndParentSelected)
                selected.Add(item.data.GetFullPath());

            foreach (var child in item.children)
            {
                GetSelectedChildrenPathRecursive(child, container, selected);
            }
        }

        /// <summary>
        /// TreeView用のアイテムデータ
        /// </summary>
        public class TransformTreeItem
        {
            public int id;
            public string name;
            public TransformTreeItem parent;
            public Transform transform;
            public List<TransformTreeItem> children = new();

            public bool IsSelected { get; set; } = false;

            public string GetFullPath()
            {
                if (parent == null)
                    return name;
                return parent.GetFullPath() + "/" + name;
            }
        }

        /// <summary>
        /// TreeView用のデータをを再帰的に構築します。(純粋なTransform階層)
        /// </summary>
        /// <param name="t"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        TransformTreeItem BuildTree(Transform t, TransformTreeItem parent)
        {
            var item = new TransformTreeItem
            {
                id = nextId++,
                name = t.name,
                transform = t,
                parent = parent
            };

            foreach (Transform child in t)
            {
                item.children.Add(BuildTree(child, item));
            }

            return item;
        }

        /// <summary>
        /// TreeView用のアイテムデータを再帰的に構築します。(TreeViewのAPIに合わせたラッピング処理)
        /// </summary>
        /// <param name="t"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        TreeViewItemData<TransformTreeItem> BuildTreeItem(TransformTreeItem item)
        {
            var children = item.children.Select(BuildTreeItem).ToList();
            return new TreeViewItemData<TransformTreeItem>(item.id, item, children);
        }

        public void Dispose()
        {
            this.tileManager = null;
            this.parentWindow = null;
            cts?.Cancel();
            cts?.Dispose();
        }
    }
}
