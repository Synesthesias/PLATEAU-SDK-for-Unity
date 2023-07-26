using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System;
using System.Linq;
using PLATEAU.PolygonMesh;
using static PLATEAU.CityInfo.CityObjectList;

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

        public CityObject GetPrimaryCityObject(RaycastHit hit)
        {
            if (GetUV4FromTriangleIndex(hit.triangleIndex, out var uv))
            {
                CityObjectIndex index = new CityObjectIndex();
                index.PrimaryIndex = (int)Mathf.Round(uv.x);
                index.AtomicIndex = -1;
                return GetCityObject(index);
            }
            return null;
        }

        public CityObject GetAtomicCityObject(RaycastHit hit)
        {
            if (GetUV4FromTriangleIndex(hit.triangleIndex, out var vec))
            {
                return GetCityObject(vec);
            }
            return null;
        }

        public CityObject GetCityObject(Vector2 uv)
        {
            CityObjectIndex index = new CityObjectIndex();
            index.PrimaryIndex = (int)Mathf.Round(uv.x);
            index.AtomicIndex = (int)Mathf.Round(uv.y);
            return GetCityObject(index);
        }

        public CityObject GetCityObject(CityObjectIndex index)
        {
            var des = CityObjects;
            //最小値物時の outsideParent 設定
            if (index.AtomicIndex == -1 && !string.IsNullOrEmpty(des.outsideParent))
            {
                GameObject parentObj = (transform.parent.gameObject.name == des.outsideParent) ? transform.parent.gameObject : GameObject.Find(des.outsideParent);
                PLATEAUCityObjectGroup parentComp = parentObj?.GetComponent<PLATEAUCityObjectGroup>();   
                if (parentComp != null)
                {
                    var parent = parentComp.CityObjects.rootCityObjects[0];

                    //outsideChildrenをchildrenとしてセット
                    foreach (var id in parentComp.CityObjects.outsideChildren)
                    {
                        var child = parentComp.transform.Find(id);
                        if (child != null && child.TryGetComponent<PLATEAUCityObjectGroup>(out var childCityObj))
                        {
                            parent.Children.AddRange(childCityObj.CityObjects.rootCityObjects);
                        }   
                    }
                    return parent;
                }
            }

            foreach (var co in des.rootCityObjects)
            {
                //Primary/最小値物時のAtomic
                if (co.IndexInMesh.PrimaryIndex == index.PrimaryIndex && co.IndexInMesh.AtomicIndex == index.AtomicIndex)
                {
                    return co;
                }

                //主要、範囲時はchildrenのindexから検索
                if (co.IndexInMesh.PrimaryIndex == index.PrimaryIndex)
                {
                    foreach (var ch in co.Children)
                    {
                        if (ch.IndexInMesh.PrimaryIndex == index.PrimaryIndex && ch.IndexInMesh.AtomicIndex == index.AtomicIndex)
                        {
                            return ch;
                        }
                    }
                }
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
