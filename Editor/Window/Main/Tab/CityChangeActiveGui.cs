using System;
using System.Collections.Generic;
using System.Linq;
using PLATEAU.CityAdjust.ChangeActive;
using PLATEAU.CityInfo;
using PLATEAU.Dataset;
using PLATEAU.Editor.Window.Common;
using PLATEAU.Editor.Window.Main.Tab.AdjustGuiParts;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace PLATEAU.Editor.Window.Main.Tab
{
    /// <summary>
    /// PLATEAU SDK ウィンドウで「モデル調整」タブが選択されている時のGUIです。
    /// </summary>
    internal class CityChangeActiveGui : ITabContent
    {
        private PLATEAUInstancedCityModel adjustTarget;
        private readonly FilterConditionGui filterConditionGUI = new FilterConditionGui();
        // private readonly AdjustPackageLodGUI adjustPackageLodGUI = new AdjustPackageLodGUI();
        private bool disableDuplicate = true;
        private static bool isFilterTaskRunning;
        
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
                    this.disableDuplicate = PlateauEditorStyle.Toggle(duplicateToggleContent, this.disableDuplicate);
                    
                    this.filterConditionGUI.Draw(this.packageToLodMinMax);
                    

                    using (new EditorGUI.DisabledScope(isFilterTaskRunning))
                    {
                        if (PlateauEditorStyle.MainButton(isFilterTaskRunning ? "フィルタリング中..." : "フィルタリング実行"))
                        {
                            isFilterTaskRunning = true;
                            try
                            {
                                using var progress = new PLATEAU.Util.ProgressBar();
                                progress.Display("実行中...", 0.4f);
                                this.adjustTarget.FilterByCityObjectType(this.filterConditionGUI.SelectionDict);
                                this.adjustTarget.FilterByLod(this.filterConditionGUI.PackageLodSliderResult);
                                if(this.disableDuplicate) CityDuplicateProcessor.EnableOnlyLargestLODInDuplicate(this.adjustTarget);    
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

            // 保存されているフィルタ条件が存在する場合、それをGUIに反映します。
            if (cityModel.CityModelData.FilterCondition != null)
            {
                this.filterConditionGUI.Draw(packageToLodMinMax); // 一度描画してからでないと、SelectionDictが正しく更新されないため。
                this.disableDuplicate = cityModel.CityModelData.FilterCondition.DisableDuplicate;
                this.filterConditionGUI.ApplySavedFilterCondition(cityModel.CityModelData.FilterCondition);
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
