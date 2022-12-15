using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using PLATEAU.Dataset;
using PLATEAU.Editor.EditorWindow.Common;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.Editor.EditorWindow.PlateauWindow.MainTabGUI.AdjustGUIParts
{
    public class PackageLodSelectGUI
    {
        private Dictionary<PredefinedCityModelPackage, LodSliderConfig> data;
        
        public PackageLodSelectGUI()
        {
            // data を初期化します。
            this.data = new Dictionary<PredefinedCityModelPackage, LodSliderConfig>();
            var packages = Enum.GetValues(typeof(PredefinedCityModelPackage));
            foreach (PredefinedCityModelPackage package in packages)
            {
                // 仕様上ありうる最大・最小LOD
                var (_, specMinLod, specMaxLod) = CityModelPackageInfo.GetPredefined(package);
                // TODO 「仕様上ありうるLOD」の他に、「実際にGameObjectとして存在する最大・最小LOD」を考慮するのが良い
                this.data.Add(package, new LodSliderConfig((uint)specMinLod, (uint)specMaxLod, (uint)specMinLod, (uint)specMaxLod));
                
            }
        }
        
        // TODO 型名が冗長
        public ReadOnlyDictionary<PredefinedCityModelPackage, (uint minLod, uint maxLod)> Result {
            get
            {
                var ret = new Dictionary<PredefinedCityModelPackage, (uint minLod, uint maxLod)>();
                foreach (var pair in this.data)
                {
                    var package = pair.Key;
                    var lodConf = pair.Value;
                    ret.Add(package, (lodConf.userMinLod, lodConf.userMaxLod));
                }
                return new ReadOnlyDictionary<PredefinedCityModelPackage, (uint minLod, uint maxLod)>(ret);
            }
        }
        
        public void Draw()
        {
            foreach (var pair in this.data.ToArray())
            {
                var package = pair.Key;
                var lodConf = pair.Value;

                if (package == PredefinedCityModelPackage.None) continue;
                
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField(package.ToJapaneseName(), GUILayout.Width(100));
                    if (lodConf.specMaxLod == lodConf.specMinLod)
                    {
                        EditorGUILayout.LabelField($"LOD {lodConf.specMaxLod}");
                    }
                    else
                    {
                        // TODO lodConf を直接渡すほうがシンプル
                        PlateauEditorStyle.LODSlider($"LOD {lodConf.userMinLod}-{lodConf.userMaxLod}", ref lodConf.userMinLod, ref lodConf.userMaxLod, lodConf.specMinLod, lodConf.specMaxLod);
                    }
                    
                }

                this.data[package] = lodConf;
            }
        }

        private struct LodSliderConfig
        {
            // 仕様上の最大・最小LOD と、その範囲内でユーザーがGUIで選択した最大・最小LODです。
            public uint specMinLod;
            public uint specMaxLod;
            public uint userMinLod;
            public uint userMaxLod;

            public LodSliderConfig(uint specMinLod, uint specMaxLod, uint userMinLod, uint userMaxLod)
            {
                this.specMinLod = specMinLod;
                this.specMaxLod = specMaxLod;
                this.userMinLod = userMinLod;
                this.userMaxLod = userMaxLod;
            }
        }
    }
}
