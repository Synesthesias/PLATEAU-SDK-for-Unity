using LibPLATEAU.NET.CityGML;
using PlateauUnitySDK.Runtime.CityMeta;
using PlateauUnitySDK.Runtime.SemanticsLoad;
using UnityEngine;

namespace PlateauUnitySDK.Runtime.Behaviour
{
    public class CityBehaviour : MonoBehaviour
    {
        [SerializeField] private CityMetaData cityMetaData;
        private SemanticsLoader loader = new SemanticsLoader();

        public CityMetaData CityMetaData
        {
            get => this.cityMetaData;
            set => this.cityMetaData = value;
        }

        public CityObject LoadSemantics(string gameObjName)
        {
            return this.loader.Load(gameObjName, this.cityMetaData);
        }
    }
}