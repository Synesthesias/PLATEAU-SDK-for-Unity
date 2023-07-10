using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using PLATEAU.CityGML;
using System;

namespace PLATEAU.CityInfo
{
    /// <summary>
    /// シリアライズ可能なCityObjectデータの管理用コンポーネントです
    /// </summary>
    public class PLATEAUCityObjectGroup : MonoBehaviour
    {

        public string SerializedCityObjects { get; private set; }
        public CityInfo.CityObject DeserializedCityObjects { get; private set; }

        public void SetSerializableCityObject(CityInfo.CityObject cityObjectSerializable)
        {
            SerializedCityObjects = JsonConvert.SerializeObject(cityObjectSerializable, Formatting.Indented);
            DeserializedCityObjects = cityObjectSerializable;
        }

        public CityInfo.CityObject GetCityObject(RaycastHit hit)
        {
            throw new NotImplementedException();
        }
        public CityInfo.CityObject GetCityObject(Vector2 UV)
        {
            throw new NotImplementedException();
        }
        public CityInfo.CityObject GetCityObject(int MeshContainedCityObjectIndex)
        {
            throw new NotImplementedException();
        }
        public IEnumerable<CityInfo.CityObject> GetAllCityObjects()
        {
            throw new NotImplementedException();
        }
    }
}
