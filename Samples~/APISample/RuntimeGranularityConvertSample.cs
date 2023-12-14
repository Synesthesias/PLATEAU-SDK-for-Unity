using PLATEAU.GranularityConvert;
using PLATEAU.PolygonMesh;
using UnityEngine;

namespace PlateauSDKDevSample.Scripts
{
    // ランタイムで結合分割するサンプルです。
    public class RuntimeGranularityConvertSample : MonoBehaviour
    {
        [SerializeField] private GameObject[] targets; 
        private async void Start()
        {
            var conf = new GranularityConvertOptionUnity(new GranularityConvertOption(MeshGranularity.PerCityModelArea, 1), targets, true);
            await new CityGranularityConverter().ConvertAsync(conf);
        }
       
    }
}