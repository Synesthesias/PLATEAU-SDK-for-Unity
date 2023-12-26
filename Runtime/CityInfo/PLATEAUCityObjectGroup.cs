using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System;
using System.Linq;
using PLATEAU.PolygonMesh;
using static PLATEAU.CityInfo.CityObjectList;
using System.Threading.Tasks;

namespace PLATEAU.CityInfo
{
    /// <summary>
    /// シリアライズ可能なCityObjectデータの管理用コンポーネントです
    /// </summary>
    public class PLATEAUCityObjectGroup : MonoBehaviour
    {
        
        /// <summary>
        /// <see cref="CityObject"/>に関する情報はここに収められます。
        /// </summary>
        [HideInInspector][SerializeField] private string serializedCityObjects;
        
        private CityObjectList cityObjects;
        private CityObject outsideParent;
        private UnityEngine.Mesh currentMesh;
        
        /// <summary> Toolkits向けの情報です。 </summary>
        [SerializeField] private CityObjectGroupInfoForToolkits infoForToolkits;

        [SerializeField] private MeshGranularity granularity;
        public CityObjectGroupInfoForToolkits InfoForToolkits => infoForToolkits;
        public MeshGranularity Granularity => granularity;
        
        public CityObjectList CityObjects
        {
            get
            {
                if (this.cityObjects != null)
                    return this.cityObjects;

                if (!string.IsNullOrEmpty(serializedCityObjects))
                    this.cityObjects = JsonConvert.DeserializeObject<CityObjectList>(serializedCityObjects);
                else
                    this.cityObjects = new CityObjectList();
                return this.cityObjects;               
            }
        }
        
        public void Init(CityObjectList cityObjectSerializable, CityObjectGroupInfoForToolkits cogInfoForToolkits, MeshGranularity granularityArg)
        {
            serializedCityObjects = JsonConvert.SerializeObject(cityObjectSerializable, Formatting.Indented);
            infoForToolkits = cogInfoForToolkits; 
            granularity = granularityArg;
        }

        /// <summary>
        /// RaycastHitからPrimary CityObjectを取得します
        /// </summary>
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

        /// <summary>
        /// RaycastHitからAtomic CityObjectを取得します
        /// </summary>
        public CityObject GetAtomicCityObject(RaycastHit hit)
        {
            if (GetUV4FromTriangleIndex(hit.triangleIndex, out var vec))
            {
                return GetCityObject(vec);
            }
            return null;
        }

        /// <summary>
        /// 座標からAtomic CityObjectを取得します
        /// </summary>
        public CityObject GetCityObject(Vector2 uv)
        {
            CityObjectIndex index = new CityObjectIndex();
            index.PrimaryIndex = (int)Mathf.Round(uv.x);
            index.AtomicIndex = (int)Mathf.Round(uv.y);
            return GetCityObject(index);
        }

        /// <summary>
        /// CityObjectIndexからAtomic CityObjectを取得します
        /// </summary>
        public CityObject GetCityObject(CityObjectIndex index)
        {
            var des = CityObjects;
            //最小地物時の outsideParent 設定
            if (index.AtomicIndex == -1 && !string.IsNullOrEmpty(des.outsideParent))
            {
                var parent = GetOutsideParent(des.outsideParent);
                if (parent != null)
                    return parent;
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
            Debug.LogError($"GetCityObject index Not Found [{index.PrimaryIndex}:{index.AtomicIndex}]");
            return null;
        }

        /// <summary>
        /// RaycastHitから非同期でPrimary CityObjectを取得します
        /// </summary>
        public async Task<CityObject> GetPrimaryCityObjectAsync(RaycastHit hit)
        {
            if (GetUV4FromTriangleIndex(hit.triangleIndex, out var uv))
            {
                CityObjectIndex index = new CityObjectIndex();
                index.PrimaryIndex = (int)Mathf.Round(uv.x);
                index.AtomicIndex = -1;
                return await GetCityObjectAsync(index);
            }
            return null;
        }

        /// <summary>
        /// RaycastHitから非同期でAtomic CityObjectを取得します
        /// </summary>
        public async Task<CityObject> GetAtomicCityObjectAsync(RaycastHit hit)
        {
            if (GetUV4FromTriangleIndex(hit.triangleIndex, out var uv))
            {
                CityObjectIndex index = new CityObjectIndex();
                index.PrimaryIndex = (int)Mathf.Round(uv.x);
                index.AtomicIndex = (int)Mathf.Round(uv.y);
                return await GetCityObjectAsync(index);
            }
            return null;
        }

        /// <summary>
        /// CityObjectIndexから非同期でCityObjectを取得します
        /// </summary>
        public async Task<CityObject> GetCityObjectAsync(CityObjectIndex index)
        {
            var des = await Task.Run(() =>{ return CityObjects; });

            //最小値物時の outsideParent 設定
            if (index.AtomicIndex == -1 && !string.IsNullOrEmpty(des.outsideParent))
            {
                var parent = GetOutsideParent(des.outsideParent);
                if(parent != null)
                    return parent;
            }

            return await Task.Run(() =>
            {
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
                Debug.LogError($"GetCityObject index Not Found [{index.PrimaryIndex}:{index.AtomicIndex}]");
                return null;
            });
        }

        /// <summary>
        /// RaycastHitから非同期でPrimary Atomic CityObjectを両方取得します
        /// </summary>
        public async Task<CityObject[]> GetPrimaryAndAtomicCityObjectsAsync(RaycastHit hit)
        {
            if (GetUV4FromTriangleIndex(hit.triangleIndex, out var uv))
            {
                CityObjectIndex index = new CityObjectIndex();
                index.PrimaryIndex = (int)Mathf.Round(uv.x);
                index.AtomicIndex = (int)Mathf.Round(uv.y);
                return await GetPrimaryAndAtomicCityObjectsAsync(index);
            }
            return null;
        }

        /// <summary>
        /// CityObjectIndexから非同期でPrimary Atomic CityObjectを両方取得します
        /// </summary>
        public async Task<CityObject[]> GetPrimaryAndAtomicCityObjectsAsync(CityObjectIndex index)
        {
            CityObject[] result = new CityObject[2];
            var des = await Task.Run(() =>{ return CityObjects; });

            //最小値物時の outsideParent 設定
            if (!string.IsNullOrEmpty(des.outsideParent))
            {
                 result[0] = GetOutsideParent(des.outsideParent) ;
            }

            return await Task.Run(() =>
            {
                foreach (var co in des.rootCityObjects)
                {
                    //Primary/最小値物時のAtomic
                    if (co.IndexInMesh.PrimaryIndex == index.PrimaryIndex && co.IndexInMesh.AtomicIndex == index.AtomicIndex)
                    {
                        if(result[0] != null)
                        {
                            result[1] = co;
                            return result;
                        }
                        else
                        {
                            result[0] = co;
                            return result;
                        }
                    }

                    //主要、範囲時はchildrenのindexから検索
                    if (co.IndexInMesh.PrimaryIndex == index.PrimaryIndex)
                    {
                        foreach (var ch in co.Children)
                        {
                            if (ch.IndexInMesh.PrimaryIndex == index.PrimaryIndex && ch.IndexInMesh.AtomicIndex == index.AtomicIndex)
                            {
                                result[0] = co;
                                result[1] = ch;
                                return result;
                            }
                        }
                    }
                }

                if (result[0] != null)
                    return result;

                Debug.LogError($"GetCityObjectsAsync index Not Found [{index.PrimaryIndex}:{index.AtomicIndex}]");
                return null;
            });
        }

        public IEnumerable<CityObjectList.CityObject> PrimaryCityObjects
        {
            get
            {
                return GetAllCityObjects().Where(obj => obj.CityObjectIndex[1] < 0);
            }
        }

        /// <summary>
        /// コンポーネントが保持する<see cref="CityObjectList"/>から、
        /// すべての<see cref="CityObject"/>を返します。
        /// </summary>
        public IEnumerable<CityObjectList.CityObject> GetAllCityObjects()
        {
            List<CityObjectList.CityObject> objs = new List<CityObjectList.CityObject>();
            var des = CityObjects;
            foreach (var co in des.rootCityObjects)
            {
                objs.Add(co);
                foreach (var ch in co.Children)
                {
                    objs.Add(ch); 
                }
                    
            }
            return objs;
        }

        

        /// <summary>
        /// 最小地物の場合、親となるPLATEAUCityObjectGroupを検索しCityObjectを取得します
        /// </summary>  
        private CityObject GetOutsideParent(string parentId)
        {
            if (this.outsideParent != null) return this.outsideParent;

            GameObject parentObj = (transform.parent.gameObject.name == parentId) ? transform.parent.gameObject : GameObject.Find(parentId);
            if (parentObj == null) return null;
            PLATEAUCityObjectGroup parentComp = parentObj.GetComponent<PLATEAUCityObjectGroup>();
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
                this.outsideParent = parent;
                return parent;
            }
            return null;
        }

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
