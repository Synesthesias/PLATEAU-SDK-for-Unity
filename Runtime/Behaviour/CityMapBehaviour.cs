using Codice.Utils;
using LibPLATEAU.NET.CityGML;
using PlateauUnitySDK.Runtime.CityMapMeta;
using PlateauUnitySDK.Runtime.SemanticsLoad;
using UnityEngine;

namespace PlateauUnitySDK.Runtime.Behaviour
{
    public class CityMapBehaviour : MonoBehaviour
    {
        [SerializeField] private CityMapMetaData cityMapMetaData;
        private SemanticsLoader loader = new SemanticsLoader();

        public CityMapMetaData CityMapMetaData
        {
            get => this.cityMapMetaData;
            set => this.cityMapMetaData = value;
        }

        public CityObject LoadSemantics(string gameObjName)
        {
            return this.loader.Load(gameObjName, this.cityMapMetaData);
        }
    }
}