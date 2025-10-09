using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PLATEAU.CityAdjust.ChangeActive;
using PLATEAU.CityInfo;
using PLATEAU.Dataset;
using PLATEAU.DynamicTile;
using PLATEAU.Editor.Window.Common;
using PLATEAU.Editor.Window.Main.Tab.AdjustGuiParts;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using PLATEAU.Util.Async;

namespace PLATEAU.Editor.Window.Main.Tab
{
    /// <summary>
    /// PLATEAU SDK ウィンドウで「モデル調整」タブが選択されている時のGUIです。
    /// </summary>
    internal class CityChangeActiveGui : ITabContent
    {
        private PLATEAUInstancedCityModel adjustTarget;
        private PLATEAUTileManager tileManager;
        private readonly FilterConditionGui filterConditionGUI = new FilterConditionGui();
        // private readonly AdjustPackageLodGUI adjustPackageLodGUI = new AdjustPackageLodGUI();
        private bool disableDuplicate = true;
        private static bool isFilterTaskRunning;

        private string[] objectSelectOptions = new string[] { "シーンに配置されたオブジェクト", "動的タイル" };
        private int objectSelectedIndex = 0;
        private string errorMessage = null;

        /// <summary>
        /// 与えられた <see cref="PredefinedCityModelPackage"/> のうち、
        /// シーン上にゲームオブジェクトとして存在するパッケージとそのLODです。
        /// </summary>
        private PackageToLodMinMax packageToLodMinMax;

        public VisualElement CreateGui()
        {
            return new IMGUIContainer(Draw);
        }
        
        private void Draw()
        {
            PlateauEditorStyle.SubTitle("配置済みモデルデータの調整を行います。");
            using (PlateauEditorStyle.VerticalScopeLevel1())
            {
                objectSelectedIndex = EditorGUILayout.Popup("調整対象の種類", objectSelectedIndex, objectSelectOptions);

                if (objectSelectedIndex == 0)
                {
                    this.tileManager = null;

                    EditorGUI.BeginChangeCheck();
                    this.adjustTarget =
                        (PLATEAUInstancedCityModel)EditorGUILayout.ObjectField(
                            "調整対象", this.adjustTarget,
                            typeof(PLATEAUInstancedCityModel), true);
                    if (EditorGUI.EndChangeCheck()) OnChangeTargetCityModel(this.adjustTarget);
                }
                else if (objectSelectedIndex == 1)
                {
                    this.adjustTarget = null;
                    errorMessage = null;

                    EditorGUI.BeginChangeCheck();
                    this.tileManager =
                        (PLATEAUTileManager)EditorGUILayout.ObjectField(
                            "調整対象", this.tileManager,
                            typeof(PLATEAUTileManager), true);
                    if (EditorGUI.EndChangeCheck()) OnChangeTargetTileManager(this.tileManager);
                }

                if (!string.IsNullOrEmpty(errorMessage))
                {
                    PlateauEditorStyle.MultiLineLabelWithBox(errorMessage);
                }

                if (this.adjustTarget == null && this.tileManager == null) return;
                errorMessage = null;

                PlateauEditorStyle.Separator(0);

                using (PlateauEditorStyle.VerticalScopeLevel1())
                {
                    PlateauEditorStyle.SubTitle("フィルタリング");
                    EditorGUILayout.LabelField("条件に応じてゲームオブジェクトのON/OFFを切り替えます。");

                    PlateauEditorStyle.Heading("フィルタ条件指定", null);

                    if (this.tileManager == null)
                    {
                        var duplicateToggleContent =
                            new GUIContent("重複する地物を非表示", "有効な場合、重複した地物オブジェクトのうちLODが最大のもののみ残してそれ以外を非表示にします。");
                        this.disableDuplicate = PlateauEditorStyle.Toggle(duplicateToggleContent, this.disableDuplicate);
                    }
        
                    this.filterConditionGUI.Draw(this.packageToLodMinMax);
                    
                    using (new EditorGUI.DisabledScope(isFilterTaskRunning))
                    {
                        if (PlateauEditorStyle.MainButton(isFilterTaskRunning ? "フィルタリング中..." : "フィルタリング実行"))
                        {
                            if (this.tileManager != null)
                            {
                                //TileMangerが存在する場合は、Addressablesの各Assetを読み込んで編集します。
                                FilterTilesAsync().ContinueWithErrorCatch();
                            }
                            else
                            {
                                isFilterTaskRunning = true;
                                try
                                {
                                    using var progress = new PLATEAU.Util.ProgressBar();
                                    progress.Display("実行中...", 0.4f);
                                    this.adjustTarget.FilterByCityObjectType(this.filterConditionGUI.SelectionDict);
                                    this.adjustTarget.FilterByLod(this.filterConditionGUI.PackageLodSliderResult);
                                    if (this.disableDuplicate) CityDuplicateProcessor.EnableOnlyLargestLODInDuplicate(this.adjustTarget);
                                    SceneView.RepaintAll();

                                    // フィルタ条件をシーンに保存します。
                                    this.adjustTarget.SaveFilterCondition(new FilterCondition(
                                        this.disableDuplicate,
                                        this.filterConditionGUI.SelectionDict,
                                        this.filterConditionGUI.PackageLodSliderResult));
                                    EditorUtility.SetDirty(this.adjustTarget);
                                }
                                finally
                                {
                                    isFilterTaskRunning = false;
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Addressablesで読み込んだタイルに対してフィルタリングを行います。
        /// </summary>
        /// <returns></returns>
        private async Task FilterTilesAsync()
        {
            if (this.tileManager == null || isFilterTaskRunning ) return;

            isFilterTaskRunning = true;
            try
            {
                if(this.tileManager.CityModel == null)
                {
                    Debug.LogError("TileManagerにCityModelが設定されていません。");
                    return;
                }
                // フィルタ条件をシーンに保存します。
                // 動的タイルでは重複地物の非表示機能は未対応のため、disableDuplicateはfalseに設定
                this.tileManager.CityModel?.SaveFilterCondition(new FilterCondition(
                    false,
                    this.filterConditionGUI.SelectionDict,
                    this.filterConditionGUI.PackageLodSliderResult));
                EditorUtility.SetDirty(this.tileManager);

                using var cts = new CancellationTokenSource(); // 現状は、キャンセルする手段がないので、キャンセルトークンは渡すだけ。
                await TileFilter.FilterByCityObjectTypeAndLod(this.tileManager, this.filterConditionGUI.SelectionDict, this.filterConditionGUI.PackageLodSliderResult, cts.Token);
            }
            catch (OperationCanceledException)
            {
                Debug.LogWarning("フィルタリングがキャンセルされました。");
            }
            finally
            {
                isFilterTaskRunning = false;
            }
        }

        /// <summary>
        /// GUI上で調整対象の都市モデルが新たに選択されたときに呼ばれます。
        /// </summary>
        private void OnChangeTargetCityModel(PLATEAUInstancedCityModel cityModel)
        {
            if (cityModel == null) return;
            if(cityModel.transform.parent?.GetComponent<PLATEAUTileManager>() != null)
            {
                errorMessage = "動的タイルを対象とするには「調整対象の種類」を動的タイルにしてください。";
                this.adjustTarget = null;
                return;
            }

            // シーン上に存在するパッケージとLODを求めます。
            this.packageToLodMinMax = new PackageToLodMinMax();
            var gmls = cityModel.GmlTransforms;
            foreach (var gml in gmls)
            {
                var gmlFile = GmlFile.Create(gml.name);
                var package = gmlFile.Package;
                var lods = PLATEAUInstancedCityModel.GetLods(gml);
                if (lods.Count > 0)
                {
                    this.packageToLodMinMax.AddOrMerge(package, lods.Min(), lods.Max());
                }
                gmlFile.Dispose();
            }
            // GUIを更新します。
            this.filterConditionGUI.RefreshPackageAndLods(this.packageToLodMinMax);

            // 保存されているフィルタ条件が存在する場合、それをGUIに反映します。
            if (cityModel.CityModelData.FilterCondition != null)
            {
                this.filterConditionGUI.Draw(packageToLodMinMax); // 一度描画してからでないと、SelectionDictが正しく更新されないため。
                this.disableDuplicate = cityModel.CityModelData.FilterCondition.DisableDuplicate;
                this.filterConditionGUI.ApplySavedFilterCondition(cityModel.CityModelData.FilterCondition);
            } 
        }

        /// <summary>
        /// GUI上で調整対象のタイルマネージャーが新たに選択されたときに呼ばれます。
        /// </summary>
        private void OnChangeTargetTileManager(PLATEAUTileManager tileManager)
        {
            if (tileManager == null) return;

            this.packageToLodMinMax = new PackageToLodMinMax();
            var packages = tileManager.DynamicTiles.Where(tile => tile != null).Select(tile => tile.Package).Distinct().ToList();
            foreach (var package in packages)
            {
                if (package == PredefinedCityModelPackage.None)
                    continue;
                var predefined = CityModelPackageInfo.GetPredefined(package);
                this.packageToLodMinMax.AddOrMerge(package, predefined.minLOD, predefined.maxLOD);
            }
            // GUIを更新します。
            this.filterConditionGUI.RefreshPackageAndLods(this.packageToLodMinMax);

            // 保存されているフィルタ条件が存在する場合、それをGUIに反映します。
            if (tileManager.CityModel?.CityModelData.FilterCondition != null)
            {
                this.filterConditionGUI.Draw(packageToLodMinMax); // 一度描画してからでないと、SelectionDictが正しく更新されないため。
                this.disableDuplicate = tileManager.CityModel.CityModelData.FilterCondition.DisableDuplicate;
                this.filterConditionGUI.ApplySavedFilterCondition(tileManager.CityModel.CityModelData.FilterCondition);
            }
        } 

        /// <summary> テストで利用する用 </summary>
        internal const string NameOfOnChangeTargetCityModel = nameof(OnChangeTargetCityModel);

        public class PackageToLodMinMax
        {
            private readonly Dictionary<PredefinedCityModelPackage, (int minLod, int maxLod)> data =
                new Dictionary<PredefinedCityModelPackage, (int minLod, int maxLod)>();

            /// <summary>
            /// 与えられたパッケージに対して、最小・最大LODを登録します。
            /// すでにそのパッケージに関するデータがある場合は、以前のLOD範囲と引数のLOD範囲の和集合にします。
            /// </summary>
            public void AddOrMerge(PredefinedCityModelPackage package, int minLod, int maxLod)
            {
                if (this.data.ContainsKey(package))
                {
                    int nextMinLod = Math.Min(minLod, this.data[package].minLod);
                    int nextMaxLod = Math.Max(maxLod, this.data[package].maxLod);
                    this.data[package] = (nextMinLod, nextMaxLod);
                    return;
                }
                this.data.Add(package, (minLod, maxLod));
            }

            public int GetMinLod(PredefinedCityModelPackage package) => this.data[package].minLod;
            public int GetMaxLod(PredefinedCityModelPackage package) => this.data[package].maxLod;

            public bool Contains(PredefinedCityModelPackage package) => this.data.ContainsKey(package);

            public PredefinedCityModelPackage[] Packages => this.data.Keys.ToArray();

        }

        public void Dispose() { }
        public void OnTabUnselect()
        {
        }

        public const string NameOfAdjustTarget = nameof(adjustTarget);
    }
}
