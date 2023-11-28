using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using PLATEAU.CityInfo;
using PLATEAU.Dataset;
using PLATEAU.Editor.EditorWindow.Common;
using UnityEditor;
using Hierarchy = PLATEAU.CityInfo.CityObjectTypeHierarchy;
using PackageLod = System.Collections.Generic.Dictionary<PLATEAU.Dataset.PredefinedCityModelPackage, PLATEAU.Editor.EditorWindow.PlateauWindow.MainTabGUI.AdjustGUIParts.FilterConditionGUI.LodSliderConfig>;

namespace PLATEAU.Editor.EditorWindow.PlateauWindow.MainTabGUI.AdjustGUIParts
{
    /// <summary>
    /// 都市モデル調整でフィルターの条件のGUIを描画します。
    /// </summary>
    internal class FilterConditionGUI
    {
        /// <summary> フィルター条件のうち、地物タイプごとに有効にするかの選択状況を格納します。 </summary>
        private readonly Dictionary<Hierarchy.Node, bool> selectionDict = new Dictionary<Hierarchy.Node, bool>();

        /// <summary> フィルター条件のうち、パッケージごとのLODスライダーでの選択状況を格納します。 </summary>
        public ReadOnlyDictionary<Hierarchy.Node, bool> SelectionDict =>
            new ReadOnlyDictionary<CityObjectTypeHierarchy.Node, bool>(this.selectionDict);
        
        private PackageLod sliderPackageLod;


        public void Draw(CityChangeActiveGUI.PackageToLodMinMax packageToLodMinMax)
        {

            using (new EditorGUILayout.HorizontalScope())
            {
                if(PlateauEditorStyle.MiniButton("全選択", 100))
                {
                    SetSelectionAll(true);
                }

                if (PlateauEditorStyle.MiniButton("全選択解除", 100))
                {
                    SetSelectionAll(false);
                }
            }
            // ルートノードを除いて、ノードごとのトグルを描画します。
            var rootNode = Hierarchy.RootNode;
            if (rootNode == null) throw new NullReferenceException("RootNode is not initialized.");
            foreach (var node in rootNode.Children)
            {
                DrawNodeRecursive(node, packageToLodMinMax, 0);
            }
        }

        private void DrawNodeRecursive(Hierarchy.Node node, CityChangeActiveGUI.PackageToLodMinMax packageToLodMinMax, int depth)
        {
            if (!this.selectionDict.ContainsKey(node))
            {
                this.selectionDict.Add(node, true);
            }
            
            
            var upperPackage = node.UpperPackage;
            bool isPackageExistInScene =
                packageToLodMinMax.Packages.Contains(upperPackage) || upperPackage == PredefinedCityModelPackage.None;

            EditorGUILayout.VerticalScope packageVerticalScope = null;
            
            if (isPackageExistInScene)
            {
                if(depth == 0) EditorGUILayout.Space(5);
                // シーンに存在するパッケージ種であれば、トグルGUIを表示します。                
                this.selectionDict[node] = EditorGUILayout.ToggleLeft(node.NodeName, this.selectionDict[node]);
                
                // 深さ1 (0の次)をボックスで囲みます。
                if (this.selectionDict[node] && depth == 0)
                {
                    packageVerticalScope = PlateauEditorStyle.VerticalScopeLevel2();
                }
                
                // 選択がONであるパッケージでの分類であれば、そのパッケージのLODスライダーを表示します。
                if (this.selectionDict[node] && node.Package != PredefinedCityModelPackage.None)
                {
                    DrawLodSlider(node);
                    // LODのGUIと子のチェックボックス(あれば)の間に少し余白を入れます。
                    if(node.Children.Count > 0) EditorGUILayout.Space(10);
                }
            }
            else
            {
                // シーン上にないパッケージ種であれば、GUIへの描画をスキップして自動的に false が選択されたものとみなします。
                this.selectionDict[node] = false;
            }

            if (this.selectionDict[node])
            {
                EditorGUI.indentLevel++;
                foreach (var child in node.Children)
                {
                    DrawNodeRecursive(child, packageToLodMinMax, depth + 1);
                }
                EditorGUI.indentLevel--;
            }

            packageVerticalScope?.Dispose();
        }

        private void DrawLodSlider(Hierarchy.Node node)
        {
            // パッケージによる分類に関して、LOD指定スライダーを表示します。
            var selfPackage = node.Package;
            if (selfPackage == PredefinedCityModelPackage.None) return;
            var sliderLod = this.sliderPackageLod[selfPackage];
            if (sliderLod.AvailableMaxLod == sliderLod.AvailableMinLod)
            {
                EditorGUILayout.LabelField($"LOD {sliderLod.AvailableMaxLod}");
            }
            else
            {
                PlateauEditorStyle.LODSlider($"LOD {sliderLod.UserMinLod}-{sliderLod.UserMaxLod}", ref sliderLod.UserMinLod, ref sliderLod.UserMaxLod, sliderLod.AvailableMinLod, sliderLod.AvailableMaxLod);
            }

            this.sliderPackageLod[selfPackage] = sliderLod;
        }

        private void SetSelectionAll(bool isActive)
        {
            foreach (var node in this.selectionDict.Keys.ToArray()) this.selectionDict[node] = isActive;
        }
        
        /// <summary>
        /// 初期化時と対象の都市モデルが変わったときに呼びます。
        /// </summary>
        public void RefreshPackageAndLods(CityChangeActiveGUI.PackageToLodMinMax packageToLodMinMax)
        {
            // data を初期化します。
            this.sliderPackageLod = new Dictionary<PredefinedCityModelPackage, LodSliderConfig>();
            var packages = Enum.GetValues(typeof(PredefinedCityModelPackage));
            foreach (PredefinedCityModelPackage package in packages)
            {
                if (!packageToLodMinMax.Contains(package)) continue;
                int availableMinLod = packageToLodMinMax.GetMinLod(package);
                int availableMaxLod = packageToLodMinMax.GetMaxLod(package);
                this.sliderPackageLod.Add(package, new LodSliderConfig(availableMinLod, availableMaxLod, availableMinLod, availableMaxLod));
            }
        }
        
        /// <summary>
        /// パッケージごとのLODスライダーに関して、ユーザーの選択結果を返します。
        /// </summary>
        public ReadOnlyDictionary<PredefinedCityModelPackage, (int minLod, int maxLod)> PackageLodSliderResult {
             get
             {
                 var ret = new Dictionary<PredefinedCityModelPackage, (int minLod, int maxLod)>();
                 foreach (var pair in this.sliderPackageLod)
                 {
                     var package = pair.Key;
                     var lodConf = pair.Value;
                     ret.Add(package, (lodConf.UserMinLod, lodConf.UserMaxLod));
                 }
                 return new ReadOnlyDictionary<PredefinedCityModelPackage, (int minLod, int maxLod)>(ret);
             }
         }
        
        public struct LodSliderConfig
        {
            // シーン中に存在するの最大・最小LOD と、その範囲内でユーザーがGUIで選択した最大・最小LODです。
            public readonly int AvailableMinLod;
            public readonly int AvailableMaxLod;
            public int UserMinLod;
            public int UserMaxLod;

            public LodSliderConfig(int availableMinLod, int availableMaxLod, int userMinLod, int userMaxLod)
            {
                this.AvailableMinLod = availableMinLod;
                this.AvailableMaxLod = availableMaxLod;
                this.UserMinLod = userMinLod;
                this.UserMaxLod = userMaxLod;
            }
        }
    }
}
