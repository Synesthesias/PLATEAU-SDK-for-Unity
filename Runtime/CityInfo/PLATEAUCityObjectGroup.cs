using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System;
using System.Linq;
using PLATEAU.PolygonMesh;

namespace PLATEAU.CityInfo
{
    /// <summary>
    /// シリアライズ可能なCityObjectデータの管理用コンポーネントです
    /// </summary>
    public class PLATEAUCityObjectGroup : MonoBehaviour
    {
        [HideInInspector][SerializeField] private string serializedCityObjects;

        public CityObjectList CityObjects
        {
            get
            {
                if (!string.IsNullOrEmpty(serializedCityObjects))
                    return JsonConvert.DeserializeObject<CityObjectList>(serializedCityObjects);
                return new CityObjectList();
            }
        }

        public void SetSerializableCityObject(CityObjectList cityObjectSerializable)
        {
            serializedCityObjects = JsonConvert.SerializeObject(cityObjectSerializable, Formatting.Indented);
        }

        public CityObjectList GetCityObject(RaycastHit hit)
        {
            if(GetUV4FromTriangleIndex(hit.triangleIndex, out var vec))
            {
                return GetCityObject(vec);
            }
            return null;
        }

        public IEnumerable<CityObjectList.CityObject> PrimaryCityObjects
        {
            get
            {
                return GetAllCityObjects().Where(obj => obj.CityObjectIndex[1] < 0);
            }
        }

        public CityObjectList GetCityObject(Vector2 uv)
        {
            CityObjectIndex index = new CityObjectIndex();
            index.PrimaryIndex = (int)Mathf.Round(uv.x);
            index.AtomicIndex = (int)Mathf.Round(uv.y);
            return GetCityObject(index);
        }

        public CityObjectList GetCityObject(CityObjectIndex index)
        {
            CityObjectList obj = new CityObjectList();
            var des = CityObjects;
            obj.outsideParent = des.outsideParent;
            foreach (var co in des.rootCityObjects)
            {
                if (co.IndexInMesh.PrimaryIndex == index.PrimaryIndex && co.IndexInMesh.AtomicIndex == index.AtomicIndex)
                {
                    Debug.Log($"<color=magenta>Selected : {co.GmlID} : [{co.IndexInMesh.PrimaryIndex},{co.IndexInMesh.AtomicIndex}]</color>");
                    co.Children.Clear();
                    obj.rootCityObjects.Add(co);
                    return obj;
                }

                if (co.IndexInMesh.PrimaryIndex == index.PrimaryIndex)
                {
                    foreach (var ch in co.Children)
                    {
                        if (ch.IndexInMesh.PrimaryIndex == index.PrimaryIndex && ch.IndexInMesh.AtomicIndex == index.AtomicIndex)
                        {
                            Debug.Log($"<color=magenta>Selected child : {ch.GmlID} : [{ch.IndexInMesh.PrimaryIndex},{ch.IndexInMesh.AtomicIndex}] \nparent: {co.GmlID}</color>");
                            co.Children.Clear();
                            co.Children.Add(ch);
                            obj.rootCityObjects.Add(co);
                            return obj;
                        }
                    }
                }
            }
            return null;
        }

        public IEnumerable<CityObjectList.CityObject> GetAllCityObjects()
        {
            List<CityObjectList.CityObject> objs = new List<CityObjectList.CityObject>();
            var des = CityObjects;
            foreach (var co in des.rootCityObjects)
            {
                objs.Add(co);
                foreach (var ch in co.Children)
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
