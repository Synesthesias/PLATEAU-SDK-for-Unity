using PLATEAU.DynamicTile;
using PLATEAU.Util;
using PLATEAU.Util.Async;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;


namespace PLATEAU.Editor.Window.Main.Tab.MaterialAdjustGui.Parts
{
    /// <summary>
    /// タイル選択ウィンドウです。
    /// </summary>
    internal class TileSelectWindow : PlateauWindowBaseNoScroll, IPackageSelectResultReceiver
    {
        private IPackageSelectResultReceiver resultReceiver;
        private PackageSelectGui gui;
        private bool isInitialized;

        private PLATEAUTileManager tileManager;

        private VisualElement root;
        private Button executeButton;
        private Button selectAllButton;
        private Button deselectAllButton;
        private Button tileFromSelectionButton;
        private Button fromSelectionButton;
        private Button fromPackageButton;
        private ScrollView scrollView;

        private List<TileNameElementContainer> tileNameElements;
        private Action<TileSelectResult> onSelectCallback;
        private TreeViewItemBuilder treeViewItemBuilder;

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

        private void DrawTileList(PLATEAUTileManager manager)
        {
            if(manager == null || !isInitialized)
            {
                Debug.LogError("タイルマネージャーが初期化されていません。");
                return;
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
            var selectionNames = Selection.gameObjects?.Select(x => x.transform.FindChildOfNamedParent(PLATEAUTileManager.TileParentName).name)?.ToList();
            tileNameElements.ForEach(e =>
            {
                if (!e.IsSelected && selectionNames.Contains(e.TileName))
                    e.IsSelected = true;
            });

        }

        /// <summary>
        /// 選択中のオブジェクトに対応するタイルの子要素を選択状態にする
        /// </summary>
        private void AddFromSelection()
        {
            treeViewItemBuilder.AddFromSelectionAsync(tileNameElements).ContinueWithErrorCatch();
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
        /// タイルの子を選択するウィンドウを開く  
        /// </summary>
        /// <param name="tileName"></param>
        private void ShowTileHierarchy(TileNameElementContainer elem)
        {
            treeViewItemBuilder.LoadAndDisyplayHierarchy(elem).ContinueWithErrorCatch();
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
            }
            treeViewItemBuilder?.Dispose();

            gui?.Dispose();
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

        //TreeView要素名
        public const string TREEVIEW_TOGGLE = "Tgl_TreeSelect";
        public const string TREEVIEW_LABEL = "Lbl_TreeTileName";
        public const string TREEVIEW_BUTTON_HIDE = "Btn_TreeHide";

        public bool IsSelected
        {
            get => toggle.value;
            set {
                toggle.value = value;
                if (toggle.value == true)
                    SetTreeViewState(TreeViewState.Hide);
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
            nameLabel.RegisterCallback<ClickEvent>(ToggleSelection);
            this.selectChildButton = root.Q<Button>("Btn_SelectChild");
            selectChildButton.clicked += OnSelectChild;
        }

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

        public List<TreeViewItemData<TreeViewItemBuilder.TransformTreeItem>> treeViewItemData { get; set; }

        public VisualElement GetTreeViewVisualElement(TreeViewItemBuilder.TransformTreeItem item)
        {
            if (item == null) return null;
            var id = treeView.GetIdForIndex(item.id);
            return treeView.GetRootElementForId(id);
        }

        public Func<VisualElement> GetTreeViewMakeItem()
        {
            var tileChildVisualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>($"{PathUtil.SdkBasePath}/Resources/PlateauUIDocument/DynamicTile/TileChildName.uxml");
            return () => tileChildVisualTree.CloneTree();
        }

        public Action<VisualElement,int> GetTreeViewBindItem()
        {
            return (element, index) =>
            {
                var treeItem = treeView.GetItemDataForIndex<TreeViewItemBuilder.TransformTreeItem>(index);
                var treeLabel = element.Q<Label>(TREEVIEW_LABEL);
                var treeToggle = element.Q<Toggle>(TREEVIEW_TOGGLE);
                var treeHideButton = element.Q<Button>(TREEVIEW_BUTTON_HIDE);
                treeLabel.text = treeItem.name;
                //label.text = treeItem.GetFullPath();

                if( element.userData == null)
                {
                    treeLabel.RegisterCallback<ClickEvent>(OnElementClick);
                    element.RegisterCallback<DetachFromPanelEvent>(OnDetachFromPanel); // メモリリーク防止

                    // トップのみボタン表示
                    if (treeItem.parent != null)
                        treeHideButton.visible = false;
                    else
                    {
                        treeToggle.value = toggle.value; // 親の選択状態に合わせる
                        treeHideButton.clicked -= OnTreeHideClick;
                        treeHideButton.clicked += OnTreeHideClick;
                    }

                    void OnElementClick(ClickEvent evt)
                    {
                        treeToggle.SetValueWithoutNotify(!treeToggle.value);
                        Debug.Log($"Clicked on {treeItem.name}, selected:{treeToggle.value}");
                        evt.StopPropagation();
                    }

                    void OnTreeHideClick()
                    {
                        SetTreeViewState(TreeViewState.Hide);
                        toggle.value = treeToggle.value; // 親の選択状態に合わせる
                    }

                    void OnDetachFromPanel(DetachFromPanelEvent evt)
                    {
                        element.UnregisterCallback<DetachFromPanelEvent>(OnDetachFromPanel);
                        treeLabel.UnregisterCallback<ClickEvent>(OnElementClick);
                        treeHideButton.clicked -= OnTreeHideClick;
                    }

                    element.userData = true; // 一度だけ登録するためのフラグ
                }
            };
        }

        public void SetOnSelectCallback(Action<TileNameElementContainer> callback)
        {
            OnSelectCallback = callback;
        }

        private void ToggleSelection(ClickEvent evt)
        {
            toggle.SetValueWithoutNotify(!toggle.value);
        }

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
            if(nameLabel != null)
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

        public TreeViewItemBuilder(PLATEAUTileManager manager, EditorWindow window)
        {
            this.tileManager = manager;
            this.parentWindow = window;
        }

        public async Task LoadAndDisyplayHierarchy(TileNameElementContainer container)
        {
            var treeView = container.TreeView;
            container.SetTreeViewState(TileNameElementContainer.TreeViewState.Loading);

            var addresses = new List<string>() { container.TileName };
            var loadedTiles = await tileManager.ForceLoadTiles(addresses);
            foreach (var loadedTile in loadedTiles)
            {
                if (loadedTile.LoadedObject != null)
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

        /// <summary>
        /// 選択中のオブジェクトに対応するタイルの子要素を選択状態にする
        /// </summary>
        /// <param name="tileNameElements"></param>
        /// <returns></returns>
        public async Task AddFromSelectionAsync(List<TileNameElementContainer> tileNameElements)
        {
            var tileNames = Selection.gameObjects?.Select(x => (x.transform.FindChildOfNamedParent(PLATEAUTileManager.TileParentName).name, x.transform))?.ToList();
            foreach (var elem in tileNameElements)
            {
                if (elem == null) continue;

                var matches = tileNames.Where(item => item.name == elem.TileName).ToList();
                foreach (var match in matches)
                {
                    await LoadAndDisyplayHierarchy(elem);
                    await Task.Yield();
                    await Task.Delay(100); // UI更新のため少し待機

                    foreach (var selection in tileNames)
                    {
                        var path = selection.transform.GetPathToParent(selection.name);
                        // 子要素のうち、選択中のオブジェクトに対応するものを選択状態にする
                        foreach (var item in elem.treeViewItemData)
                        {
                            SelectChildRecursive(item, path, elem);
                        }
                    }
                }
            }
        }

        private void SelectChildRecursive(TreeViewItemData<TransformTreeItem> item, string pathToSelect, TileNameElementContainer container)
        {
            var isSelected = string.Equals(pathToSelect, item.data.GetFullPath());
            if (isSelected)
            {
                var id = container.TreeView.GetIdForIndex(item.data.id);
                var visualElem = container.TreeView.GetRootElementForId(id);
                if (visualElem != null)
                {
                    var toggle = visualElem.Q<Toggle>(TileNameElementContainer.TREEVIEW_TOGGLE);
                    toggle.value = isSelected;
                    container.IsSelected = false; // 親のタイルは選択解除
                }
                else
                {
                    Debug.LogWarning($"visualElem is null for id: {item.data.id}");
                }
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

        public void GetSelectedChildrenPathRecursive(TreeViewItemData<TransformTreeItem> item, TileNameElementContainer container, List<string> selected)
        {
            var id = container.TreeView.GetIdForIndex(item.data.id);
            var visualElem = container.TreeView.GetRootElementForId(id);
            if (visualElem != null)
            {
                var toggle = visualElem.Q<Toggle>(TileNameElementContainer.TREEVIEW_TOGGLE);
                if (toggle.value)
                {
                    selected.Add(item.data.GetFullPath());
                }
            }
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
        /// TreeView用のアイテムデータを再帰的に構築します。
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

        TreeViewItemData<TransformTreeItem> BuildTreeItem(TransformTreeItem item)
        {
            var children = item.children.Select(BuildTreeItem).ToList();
            return new TreeViewItemData<TransformTreeItem>(nextId++, item, children);
        }

        public void Dispose()
        {
            this.tileManager = null;
            this.parentWindow = null;
        }
    }
}
