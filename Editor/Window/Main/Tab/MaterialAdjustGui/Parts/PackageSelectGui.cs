using PLATEAU.CityInfo;
using PLATEAU.Dataset;
using PLATEAU.Editor.Window.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.Window.Main.Tab.MaterialAdjustGui.Parts
{
    /// <summary>
    /// パッケージ種選択のGUIです。
    /// </summary>
    internal class PackageSelectGui : IEditorDrawable, IReceiveDatasetChange
    {

        private ElementGroup elements;
        private EditorWindow parentWindow;
        private IPackageSelectResultReceiver resultReceiver;

        private PLATEAUInstancedCityModel SelectedDataset => elements.Get<DatasetSelectGui>().Selected;
        private DatasetSelectGui DatasetSelectGui => elements.Get<DatasetSelectGui>();
        private PackageSearchGui PackageSearchGui => elements.Get<PackageSearchGui>();

        public PackageSelectGui(IPackageSelectResultReceiver resultReceiver, EditorWindow parentWindow)
        {
            this.resultReceiver = resultReceiver;
            this.parentWindow = parentWindow;
            
            // データセット選択　→　パッケージ種選択 → 決定 の流れでGUIを登録します
            elements = new ElementGroup("",0,
                new DatasetSelectGui(this),
                new PackageSearchGui(),
                new EnterAndCancelButtonElement("", OnEnterButtonPushed, CloseWindow)
            );
            ReceiveDatasetChange();
        }
        public void Draw()
        {
            elements.Draw();
            
            // パッケージが1つ以上選択されたときのみ決定ボタンを押せるようにします。
            elements.Get<EnterAndCancelButtonElement>().IsEnterButtonEnabled =
                PackageSearchGui.TargetDict.Any(pair => pair.Value);

            if (resultReceiver == null)
            {
                Debug.Log("パッケージ種選択画面の結果を渡す先が利用不能になったためウィンドウを閉じます。");
                parentWindow.Close();
            }
        }
        

        public void ReceiveDatasetChange()
        {
            // データセットが選択されたときのみ、検索してパッケージ選択を表示します。
            bool isSelected = SelectedDataset != null;
            PackageSearchGui.IsVisible = isSelected;
            if (isSelected)
            {
                PackageSearchGui.Search(SelectedDataset);
            }

        }

        private void OnEnterButtonPushed()
        {
            resultReceiver.ReceivePackageSelectResult(Result);
            CloseWindow();
        }

        private void CloseWindow()
        {
            parentWindow.Close();
        }

        public PackageSelectResult Result
        {
            get
            {
                var dataset = DatasetSelectGui.Selected;
                var targetDict = PackageSearchGui.TargetDict;
                return new PackageSelectResult(dataset, targetDict);
            }
        }

        public void Dispose()
        {
        }
        
        
    }

    /// <summary> <see cref="PackageSelectGui"/>の選択結果を格納するクラスです。 </summary>
    internal class PackageSelectResult
    {
        public PLATEAUInstancedCityModel Dataset { get; }
        public SortedDictionary<PredefinedCityModelPackage, bool> SelectedDict { get; }

        public PackageSelectResult(PLATEAUInstancedCityModel dataset,
            SortedDictionary<PredefinedCityModelPackage, bool> selectedDict)
        {
            Dataset = dataset;
            SelectedDict = selectedDict;
        }
    }

    internal interface IPackageSelectResultReceiver
    {
        void ReceivePackageSelectResult(PackageSelectResult result);
    }
    
    
    
    /// <summary> 1段目、データセット選択 </summary>
    internal class DatasetSelectGui : Element
    {
        private IReceiveDatasetChange changeReceiver;

        public PLATEAUInstancedCityModel Selected { get; private set; }
            
        public DatasetSelectGui(IReceiveDatasetChange changeReceiver) : base("")
        {
            this.changeReceiver = changeReceiver;
        }
            
        public override void DrawContent()
        {
            PlateauEditorStyle.Heading("データセット選択", null);
            using(PlateauEditorStyle.VerticalScopeLevel1())
            {
                var prevSelected = Selected;
                Selected = (PLATEAUInstancedCityModel)EditorGUILayout.ObjectField("データセット", Selected,
                    typeof(PLATEAUInstancedCityModel), true);
                if(Selected != prevSelected) changeReceiver.ReceiveDatasetChange();
            }
        }
            
        public override void Dispose() { }
    }
    
        
    
    /// <summary> 2段目、パッケージ種検索と選択 </summary>
    internal class PackageSearchGui : Element
    {
        /// <summary> パッケージ種ごとに、それが対象であるかの設定値辞書です。 </summary>
        public SortedDictionary<PredefinedCityModelPackage, bool> TargetDict { get; private set; } = new ();
        public override void DrawContent()
        {
            PlateauEditorStyle.Heading("パッケージ種選択", null);
            using (PlateauEditorStyle.VerticalScopeLevel2())
            {
                foreach (var package in TargetDict.Keys.ToArray())
                {
                    if (package == PredefinedCityModelPackage.None) continue;
                    bool nextIsTarget = EditorGUILayout.ToggleLeft(package.ToJapaneseName(), TargetDict[package]);
                    TargetDict[package] = nextIsTarget;
                }
            }
        }

        /// <summary> <paramref name="dataset"/>に含まれるパッケージ種を検索します。 </summary>
        public void Search(PLATEAUInstancedCityModel dataset)
        {
            // 検索します
            var foundPackages = new HashSet<PredefinedCityModelPackage>();
            var cogs = dataset.GetComponentsInChildren<PLATEAUCityObjectGroup>(true);
            foreach (var cog in cogs)
            {
                foundPackages.Add(cog.Package);
            }

            // 検索をもとに設定値辞書を構築します
            TargetDict.Clear();
            foreach (var found in foundPackages)
            {
                TargetDict.Add(found, false);
            }
            PlateauEditorStyle.Separator(0);
        }

        public override void Dispose()
        {
        }
    }

    
    internal interface IReceiveDatasetChange
    {
        void ReceiveDatasetChange();
    }

    /// <summary>
    /// 決定ボタンとキャンセルボタンを横並びで表示する<see cref="Element"/>です。
    /// </summary>
    internal class EnterAndCancelButtonElement : Element
    {
        private Action onEnter;
        private Action onCancel;
        public bool IsEnterButtonEnabled { get; set; } = true;
        
        public EnterAndCancelButtonElement(string name, Action onEnter, Action onCancel) : base(name)
        {
            this.onEnter = onEnter;
            this.onCancel = onCancel;
        }
        public override void DrawContent()
        {
            PlateauEditorStyle.CenterAlignHorizontal(
                () =>
                {
                    if (PlateauEditorStyle.ButtonSubColor("キャンセル", 100)) onCancel();
                    using (new EditorGUI.DisabledScope(!IsEnterButtonEnabled))
                    {
                        if (PlateauEditorStyle.MainButton("決定")) onEnter();
                    }
                }
            );
        }

        public override void Dispose()
        { }
    }
}