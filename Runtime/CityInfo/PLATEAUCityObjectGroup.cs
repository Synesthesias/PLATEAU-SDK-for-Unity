using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System;
using System.Linq;
using PLATEAU.PolygonMesh;
using UnityEditor;

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

        public void ShowSelected(CityObjectIndex index)
        {
            Debug.Log($"ShowSelected {index.PrimaryIndex}, {index.AtomicIndex}");

            var mesh = CurrentMesh;
            if (mesh == null) return;

            //Vector2[] selection = Array.FindAll<Vector2>(mesh.uv4, vec => (int)Mathf.Round(vec.x) == index.PrimaryIndex && (int)Mathf.Round(vec.y) == index.AtomicIndex);
            //Debug.Log($"selection : {selection.Length}");


            List<Vector2> uv = new List<Vector2>();
            mesh.GetUVs(3, uv);

            List<Vector2> selection = uv.FindAll(vec => (int)Mathf.Round(vec.x) == index.PrimaryIndex && (int)Mathf.Round(vec.y) == index.AtomicIndex);
            Debug.Log($"selection : {selection.Count}");
            Debug.Log($"uv : {uv.Count} vertex : {mesh.vertexCount}");


            //UVのIndex
            List<int> indices = new List<int>();
            //var vertindex = new List<int>();
            //List<Vector3> vertices = new List<Vector3>();
            vertices.Clear();
            normals.Clear();
            triangles.Clear();
            //int triIndex = 0; 
            foreach (var (vec,i) in uv.Select((vec, i) => (vec, i)))
            {
                if((int)Mathf.Round(vec.x) == index.PrimaryIndex && (int)Mathf.Round(vec.y) == index.AtomicIndex)
                {
                    indices.Add(i);
                    vertices.Add(mesh.vertices[i]);
                    normals.Add(mesh.normals[i]);
                    
                    if(i <  uv.Count -1)
                    {
                        /* */
                        triangles.Add(mesh.triangles[i * 3]);
                        triangles.Add(mesh.triangles[i * 3 + 1]);
                        triangles.Add(mesh.triangles[i * 3 + 2]);
                        
                        /*
                        triangles.Add(mesh.triangles[triIndex * 3]);
                        triangles.Add(mesh.triangles[triIndex * 3 + 1]);
                        triangles.Add(mesh.triangles[triIndex * 3 + 2]);
                        */

                        //vertindex.Add(triIndex);
                        //triIndex++;
                        
                    }
                    
                }        
            }

            //Triangles index を 0スタートに書き換える
            ConvertTriangleIndexes(ref triangles);

            //triangles.Clear();

            //sDebug.Log($"vertindex : {JsonConvert.SerializeObject(vertindex)} ");
            /*
            for (int i = 0; i * 3 < vertindex.Count; i++)
            {
                if ((3 * i) < vertindex.Count)
                    triangles.Add(vertindex[3 * i]);
                if ((3 * i + 1) < vertindex.Count)
                    triangles.Add(vertindex[3 * i + 1]);
                if((3 * i + 2) < vertindex.Count)
                    triangles.Add(vertindex[3 * i + 2]);
            }
            */

            //Dammy
            //triangles = new List<int>() { 0, 2, 1, 2, 3, 1, 0, 2, 1, 2, 3, 1 };
            triangles = new List<int>() { 2, 1, 0, 3, 2, 0, 1, 3, 0, 1, 2, 0 };


            Debug.Log($"indices : {JsonConvert.SerializeObject(indices)} ");

            Debug.Log($"triangles : {JsonConvert.SerializeObject(triangles)} ");
#if UNITY_EDITOR
            Handles.DrawLines(vertices.ToArray());
#endif
            selectedMesh = new UnityEngine.Mesh();
            selectedMesh.SetVertices(vertices.ToArray());
            selectedMesh.SetNormals(normals.ToArray());
            selectedMesh.SetIndices(triangles.ToArray(), MeshTopology.Triangles, 0);
            selectedMesh.RecalculateBounds();
            selectedMesh.RecalculateNormals();

            /*
            Gizmos.color = Color.blue;
            for (int i = 0; i < vertices.Count; i++)
            {
                if (i > vertices.Count - 1)
                    //Handles.DrawLine(vertices[i], vertices[i + 1], 5.0f);
                    Gizmos.DrawLine(vertices[i], vertices[i + 1]);

                // Debug.Log($"vertices: {vertices[i].ToString()} ");
            }

            Gizmos.DrawLine(Vector3.zero, new Vector3(0,0,300));
            */
        }

        //Trianglesの各 indexの値を　０スタート (0,1,2,3..)に置き換える
        private void ConvertTriangleIndexes (ref List<int> triangles)
        {
            IEnumerable<int> distinct = triangles.Distinct(); //重複削除
            List<int> sorted = distinct.ToList<int>();
            sorted.Sort(); //少ない方から並べる

            Debug.Log($"sorted : {JsonConvert.SerializeObject(sorted)} ");

            for(int i =0; i < triangles.Count; i++)
            {
                triangles[i] = sorted.IndexOf(triangles[i]);
            }
        }


        List<Vector3> vertices = new List<Vector3>();
        List<Vector3> normals = new List<Vector3>();
        List<int> triangles = new List<int>();

        UnityEngine.Mesh selectedMesh;
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;

            for (int i = 0; i < vertices.Count; i++)
            {
                if (i < vertices.Count - 1)
                {
                    //Handles.DrawLine(vertices[i], vertices[i + 1], 1.0f);
                    Gizmos.DrawLine(vertices[i], vertices[i + 1]);
                }    
                 //Debug.Log($"vertices: {vertices[i].ToString()} ");
            }

            /*
            UnityEngine.Mesh m  = new UnityEngine.Mesh();
            m.SetVertices(vertices.ToArray());
            m.SetNormals(normals.ToArray());
            m.SetTriangles(triangles.ToArray(), 0);
            m.RecalculateBounds();
            m.RecalculateNormals();
            Gizmos.DrawMesh(m);
            */

            Gizmos.color = Color.red;
            Gizmos.DrawMesh(selectedMesh);
            //Gizmos.DrawMesh(currentMesh);
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
