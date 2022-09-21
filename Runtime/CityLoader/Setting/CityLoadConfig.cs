using System;
using System.Collections.Generic;
using PLATEAU.IO;
using PLATEAU.Udx;
using PLATEAU.Util;
using UnityEngine;

namespace PLATEAU.CityLoader.Setting
{
    /// <summary>
    /// <see cref="PLATEAUCityModelLoader"/> の設定です。
    /// </summary>
    [Serializable]
    internal class CityLoadConfig : ISerializationCallbackReceiver
    {
        private Dictionary<PredefinedCityModelPackage, PackageLoadSetting> perPackagePairSettings = new Dictionary<PredefinedCityModelPackage, PackageLoadSetting>();
        
        [SerializeField] private List<PredefinedCityModelPackage> perPackageSettingKeys = new List<PredefinedCityModelPackage>();
        [SerializeField] private List<PackageLoadSetting> perPackageSettingValues = new List<PackageLoadSetting>();

        public CityLoadConfig()
        {
            // InitWithPredefined();
        }

        public IEnumerable<KeyValuePair<PredefinedCityModelPackage, PackageLoadSetting>> ForEachPackagePair =>
            this.perPackagePairSettings;

        public PackageLoadSetting GetConfigForPackage(PredefinedCityModelPackage package)
        {
            return this.perPackagePairSettings[package];
        }

        public void InitWithPackageFlags(PredefinedCityModelPackage packageFlags)
        {
            this.perPackagePairSettings.Clear();
            foreach (var package in EnumUtil.EachFlags(packageFlags))
            {
                var predefined = CityModelPackageInfo.GetPredefined(package);
                var val = new PackageLoadSetting(predefined.hasAppearance, predefined.minLOD, predefined.maxLOD,
                    MeshGranularity.PerCityModelArea);
                this.perPackagePairSettings.Add(package, val);
            }
        }

        // private void InitWithPredefined()
        // {
        //     this.perPackageSettings = new Dictionary<PredefinedCityModelPackage, PackageLoadSetting>();
        //     foreach (PredefinedCityModelPackage package in Enum.GetValues(typeof(PredefinedCityModelPackage)))
        //     {
        //         var predefined = CityModelPackageInfo.GetPredefined(package);
        //         var val = new PackageLoadSetting(predefined.hasAppearance, predefined.minLOD, predefined.maxLOD,
        //             MeshGranularity.PerCityModelArea);
        //         this.perPackageSettings.Add(package, val);
        //     }
        // }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            DictionarySerializer.OnBeforeSerialize(this.perPackagePairSettings, this.perPackageSettingKeys, this.perPackageSettingValues);    
        }

        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            this.perPackagePairSettings = DictionarySerializer.OnAfterSerialize(this.perPackageSettingKeys, this.perPackageSettingValues);
        }
    }
}
