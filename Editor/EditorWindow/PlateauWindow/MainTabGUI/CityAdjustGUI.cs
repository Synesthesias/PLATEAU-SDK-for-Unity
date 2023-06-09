using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PLATEAU.CityAdjust;
using PLATEAU.CityInfo;
using PLATEAU.Dataset;
using PLATEAU.Editor.EditorWindow.Common;
using PLATEAU.Editor.EditorWindow.PlateauWindow.MainTabGUI.AdjustGUIParts;
using PLATEAU.Util.Async;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.EditorWindow.PlateauWindow.MainTabGUI
{
    /// <summary>
    /// PLATEAU SDK ウィンドウで「モデル調整」タブが選択されている時のGUIです。
    /// </summary>
    internal class CityAdjustGUI : IEditorDrawable
    {
        private PLATEAUInstancedCityModel adjustTarget;
        private readonly FilterConditionGUI filterConditionGUI = new FilterConditionGUI();
        // private readonly AdjustPackageLodGUI adjustPackageLodGUI = new AdjustPackageLodGUI();
        private bool disableDuplicate = true;
        private static bool isFilterTaskRunning;
        
        /// <summary>
        /// 与えられた <see cref="PredefinedCityModelPackage"/> のうち、
        /// シーン上にゲームオブジェクトとして存在するパッケージとそのLODです。
        /// </summary>
        private PackageToLodMinMax packageToLodMinMax;
        
        public void Draw()
        {
            PlateauEditorStyle.SubTitle("配置済みモデルデータの調整を行います。");
            using (PlateauEditorStyle.VerticalScopeLevel1())
            {
                EditorGUI.BeginChangeCheck();
                this.adjustTarget =
                    (PLATEAUInstancedCityModel)EditorGUILayout.ObjectField(
                        "調整対象", this.adjustTarget,
                        typeof(PLATEAUInstancedCityModel), true);
                if(EditorGUI.EndChangeCheck()) OnChangeTargetCityModel(this.adjustTarget);
                
                if (this.adjustTarget == null) return;

                PlateauEditorStyle.Separator(0);

                using (PlateauEditorStyle.VerticalScopeLevel1())
                {
                    PlateauEditorStyle.SubTitle("フィルタリング");
                    EditorGUILayout.LabelField("条件に応じてゲームオブジェクトのON/OFFを切り替えます。");
                    

                    PlateauEditorStyle.Heading("フィルタ条件指定", null);
                    
                    var duplicateToggleContent =
                        new GUIContent("重複する地物を非表示", "有効な場合、重複した地物オブジェクトのうちLODが最大のもののみ残してそれ以外を非表示にします。"); 
                    this.disableDuplicate = EditorGUILayout.Toggle(duplicateToggleContent, this.disableDuplicate);
                    
                    this.filterConditionGUI.Draw(this.packageToLodMinMax);
                    

                    using (new EditorGUI.DisabledScope(isFilterTaskRunning))
                    {
                        if (PlateauEditorStyle.MainButton(isFilterTaskRunning ? "フィルタリング中..." : "フィルタリング実行"))
                        {
                            isFilterTaskRunning = true;
                            this.adjustTarget.FilterByCityObjectTypeAsync(this.filterConditionGUI.SelectionDict)
                                .ContinueWithErrorCatch()
                                .ContinueWith(_ =>
                                {
                                    this.adjustTarget.FilterByLod(this.filterConditionGUI.PackageLodSliderResult);
                                }, TaskScheduler.FromCurrentSynchronizationContext())
                                .ContinueWithErrorCatch()
                                .ContinueWith(_ =>
                                {
                                    if(this.disableDuplicate) CityDuplicateProcessor.EnableOnlyLargestLODInDuplicate(this.adjustTarget);    
                                    SceneView.RepaintAll();
                                    isFilterTaskRunning = false;
                                }, TaskScheduler.FromCurrentSynchronizationContext()).ContinueWithErrorCatch();
                        }
                    }

                }
            }
        }
        
        /// <summary>
        /// GUI上で調整対象の都市モデルが新たに選択されたときに呼ばれます。
        /// </summary>
        private void OnChangeTargetCityModel(PLATEAUInstancedCityModel cityModel)
        {
            if (cityModel == null) return;
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

        public const string NameOfAdjustTarget = nameof(adjustTarget);
    }
}
