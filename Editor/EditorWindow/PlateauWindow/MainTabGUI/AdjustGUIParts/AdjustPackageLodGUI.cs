using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using PLATEAU.Dataset;
using PLATEAU.Editor.EditorWindow.Common;
using UnityEditor;
using UnityEngine;
using DataT = System.Collections.Generic.Dictionary<PLATEAU.Dataset.PredefinedCityModelPackage, PLATEAU.Editor.EditorWindow.PlateauWindow.MainTabGUI.AdjustGUIParts.AdjustPackageLodGUI.LodSliderConfig>;

namespace PLATEAU.Editor.EditorWindow.PlateauWindow.MainTabGUI.AdjustGUIParts
{
    internal class AdjustPackageLodGUI
    {
        private DataT data;

        /// <summary>
        /// 初期化時と対象の都市モデルが変わったときに呼びます。
        /// </summary>
        public void RefreshPackageAndLods(CityAdjustGUI.PackageToLodMinMax packageToLodMinMax)
        {
            // data を初期化します。
            this.data = new Dictionary<PredefinedCityModelPackage, LodSliderConfig>();
            var packages = Enum.GetValues(typeof(PredefinedCityModelPackage));
            foreach (PredefinedCityModelPackage package in packages)
            {
                if (!packageToLodMinMax.Contains(package)) continue;
                uint availableMinLod = packageToLodMinMax.GetMinLod(package);
                uint availableMaxLod = packageToLodMinMax.GetMaxLod(package);
                this.data.Add(package, new LodSliderConfig(availableMinLod, availableMaxLod, availableMinLod, availableMaxLod));
            }
        }
        
        public ReadOnlyDictionary<PredefinedCityModelPackage, (uint minLod, uint maxLod)> Result {
            get
            {
                var ret = new Dictionary<PredefinedCityModelPackage, (uint minLod, uint maxLod)>();
                foreach (var pair in this.data)
                {
                    var package = pair.Key;
                    var lodConf = pair.Value;
                    ret.Add(package, (lodConf.UserMinLod, lodConf.UserMaxLod));
                }
                return new ReadOnlyDictionary<PredefinedCityModelPackage, (uint minLod, uint maxLod)>(ret);
            }
        }
        
        /// <summary>
        /// 都市フィルタリング機能のLOD指定のGUIを描画します。
        /// </summary>
        /// <param name="packageToLodMinMax">対象の都市モデルの中で存在するパッケージとLODを渡します。</param>
        public void Draw(CityAdjustGUI.PackageToLodMinMax packageToLodMinMax)
        {
            foreach (var pair in this.data.ToArray())
            {
                var package = pair.Key;
                var lodConf = pair.Value;

                if (package == PredefinedCityModelPackage.None) continue;
                if (!packageToLodMinMax.Packages.Contains(package)) continue;
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField(package.ToJapaneseName(), GUILayout.Width(100));
                    if (lodConf.AvailableMaxLod == lodConf.AvailableMinLod)
                    {
                        EditorGUILayout.LabelField($"LOD {lodConf.AvailableMaxLod}");
                    }
                    else
                    {
                        // TODO lodConf を直接渡すほうがシンプル
                        PlateauEditorStyle.LODSlider($"LOD {lodConf.UserMinLod}-{lodConf.UserMaxLod}", ref lodConf.UserMinLod, ref lodConf.UserMaxLod, lodConf.AvailableMinLod, lodConf.AvailableMaxLod);
                    }
                    
                }

                this.data[package] = lodConf;
            }
        }
        
        public struct LodSliderConfig
        {
            // シーン中に存在するの最大・最小LOD と、その範囲内でユーザーがGUIで選択した最大・最小LODです。
            public readonly uint AvailableMinLod;
            public readonly uint AvailableMaxLod;
            public uint UserMinLod;
            public uint UserMaxLod;

            public LodSliderConfig(uint availableMinLod, uint availableMaxLod, uint userMinLod, uint userMaxLod)
            {
                this.AvailableMinLod = availableMinLod;
                this.AvailableMaxLod = availableMaxLod;
                this.UserMinLod = userMinLod;
                this.UserMaxLod = userMaxLod;
            }
        }
    }
}
