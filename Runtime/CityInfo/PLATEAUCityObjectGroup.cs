using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System;
using PLATEAU.PolygonMesh;
using Unity.VisualScripting.YamlDotNet.Serialization;

namespace PLATEAU.CityInfo
{
    /// <summary>
    /// シリアライズ可能なCityObjectデータの管理用コンポーネントです
    /// </summary>
    public class PLATEAUCityObjectGroup : MonoBehaviour
    {
        [HideInInspector]
        public string SerializedCityObjects;

        public CityInfo.CityObject DeserializedCityObjects
        {
            get
            {
                if (!string.IsNullOrEmpty(SerializedCityObjects))
                    return JsonConvert.DeserializeObject<CityInfo.CityObject>(SerializedCityObjects);
                return new CityInfo.CityObject();
            }
        }

        public void SetSerializableCityObject(CityInfo.CityObject cityObjectSerializable)
        {
            SerializedCityObjects = JsonConvert.SerializeObject(cityObjectSerializable, Formatting.Indented);
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
            index.PrimaryIndex = (int)Mathf.Round(uv.x);
            index.AtomicIndex = (int)Mathf.Round(uv.y);
            return GetCityObject(index);
        }

        public CityInfo.CityObject GetCityObject(CityObjectIndex index)
        {
            CityObject obj = new CityObject();
            var des = DeserializedCityObjects;
            obj.parent = des.parent;
            foreach (var co in des.cityObjects)
            {
                if (co.IndexInMesh.PrimaryIndex == index.PrimaryIndex && co.IndexInMesh.AtomicIndex == index.AtomicIndex)
                {
                    Debug.Log($"<color=magenta>Selected : {co.gmlID} : [{co.IndexInMesh.PrimaryIndex},{co.IndexInMesh.AtomicIndex}]</color>");
                    co.children.Clear();
                    obj.cityObjects.Add(co);
                    return obj;
                }

                if (co.IndexInMesh.PrimaryIndex == index.PrimaryIndex)
                {
                    foreach (var ch in co.children)
                    {
                        if (ch.IndexInMesh.PrimaryIndex == index.PrimaryIndex && ch.IndexInMesh.AtomicIndex == index.AtomicIndex)
                        {
                            Debug.Log($"<color=magenta>Selected child : {ch.gmlID} : [{ch.IndexInMesh.PrimaryIndex},{ch.IndexInMesh.AtomicIndex}] \nparent: {co.gmlID}</color>");
                            co.children.Clear();
                            co.children.Add(ch);
                            obj.cityObjects.Add(co);
                            return obj;
                        }
                    }
                }
            }
            return null;
        }

        public IEnumerable<CityInfo.CityObject.CityObjectChildParam> GetAllCityObjects()
        {
            List<CityObject.CityObjectChildParam> objs = new List<CityObject.CityObjectChildParam>();
            var des = DeserializedCityObjects;
            foreach (var co in des.cityObjects)
            {
                objs.Add(co);
                foreach (var ch in co.children)
                    objs.Add(ch);
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
