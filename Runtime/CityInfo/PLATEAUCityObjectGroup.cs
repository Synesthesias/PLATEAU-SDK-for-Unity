using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using PLATEAU.CityGML;
using System;
using PLATEAU.PolygonMesh;

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
            if(GetUV4FromTriangleIndex(hit.triangleIndex, out var vec))
            {
                return GetCityObject(vec);
            }
            return null;
        }
        public CityInfo.CityObject GetCityObject(Vector2 uv)
        {
            CityObjectIndex index = new CityObjectIndex();
            index.PrimaryIndex = (int)uv.x;
            index.AtomicIndex = (int)uv.y;
            return GetCityObject(index);
        }

        public CityInfo.CityObject GetCityObject(CityObjectIndex index)
        {
            CityObject obj = new CityObject();
            obj.parent = DeserializedCityObjects.parent;

            foreach (var co in DeserializedCityObjects.cityObjects)
            {
                if (co.IndexInMesh.PrimaryIndex == index.PrimaryIndex && co.IndexInMesh.AtomicIndex == index.AtomicIndex)
                {
                    Debug.Log($"<color=magenta>Selected : {co.gmlID} : [{co.IndexInMesh.PrimaryIndex},{co.IndexInMesh.AtomicIndex}]</color>");

                    var param = co.Clone();
                    param.children.Clear();
                    obj.cityObjects.Add(param);
                    return obj;
                }

                if (co.IndexInMesh.PrimaryIndex == index.PrimaryIndex)
                {
                    foreach (var ch in co.children)
                    {
                        if (ch.IndexInMesh.PrimaryIndex == index.PrimaryIndex && ch.IndexInMesh.AtomicIndex == index.AtomicIndex)
                        {
                            Debug.Log($"<color=magenta>Selected child : {ch.gmlID} : [{ch.IndexInMesh.PrimaryIndex},{ch.IndexInMesh.AtomicIndex}] \nparent: {co.gmlID}</color>");

                            var parent = co.Clone();
                            parent.children.Clear();
                            parent.children.Add(ch.Clone());
                            obj.cityObjects.Add(parent);
                            return obj;
                        }
                    }
                }
            }
            return null;
        }
        public IEnumerable<CityInfo.CityObject.CityObjectChildParam> GetAllCityObjects()
        {
            List<CityObject.CityObjectChildParam> objs = new List<CityObject.CityObjectChildParam> ();
            foreach (var co in DeserializedCityObjects.cityObjects)
            {
                objs.Add(co.Clone());
                foreach (var ch in co.children)
                    objs.Add(ch.Clone());
            }
            return objs;
        }

        private UnityEngine.Mesh currentMesh;
        private UnityEngine.Mesh CurrentMesh
        {
            get
            {
                if(currentMesh == null) currentMesh = GetComponent<MeshFilter>()?.sharedMesh;
                return currentMesh;
            }
        }

        private bool GetUV4FromTriangleIndex(int triangleIndex, out Vector2 vec)
        {
            try
            {
                var mesh = CurrentMesh;
                vec = mesh.uv4[mesh.triangles[triangleIndex * 3]];
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"{ex.Message} index:{triangleIndex}");
            }
            vec = Vector2.zero;
            return false;
        }
    }
}
