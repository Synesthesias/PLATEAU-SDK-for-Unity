using PLATEAU.PolygonMesh;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace PLATEAU
{
    /// <summary>
    /// 属性情報パネル表示時にシーン上に選択中値物の情報を表示します
    /// </summary>
    public class CityObjectGizmoDrawer : MonoBehaviour
    {
        private Bounds parentBounds;
        private Bounds childBounds;
        private string panrentId;
        private string childId;
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            GUIStyle gizmoLableStyle = new GUIStyle() { fontStyle = FontStyle.Bold, fontSize = 12 };

            Gizmos.color = gizmoLableStyle.normal.textColor =　Color.green;
            Gizmos.DrawWireCube(childBounds.center, childBounds.size);
            Handles.Label(childBounds.center, childId, gizmoLableStyle);

            Gizmos.color = gizmoLableStyle.normal.textColor = Color.magenta;
            Gizmos.DrawWireCube(parentBounds.center, parentBounds.size);
            Handles.Label(parentBounds.center, panrentId, gizmoLableStyle);            
        }
#endif
        public void ShowParentSelection(Transform trans, CityObjectIndex index, string id)
        {
            panrentId = id;
            if(trans.gameObject.TryGetComponent<MeshFilter>(out var meshFilter))
            {
                var mesh = meshFilter.sharedMesh;
                if (mesh == null) return;

                List<Vector2> uv = new List<Vector2>();
                mesh.GetUVs(3, uv);

                List<Vector3> vertices = new List<Vector3>();
                foreach (var (vec, i) in uv.Select((vec, i) => (vec, i)))
                {
                    if ((int)Mathf.Round(vec.x) == index.PrimaryIndex)
                    {
                        vertices.Add(mesh.vertices[i]);
                    }
                }
                parentBounds = GeometryUtility.CalculateBounds(vertices.ToArray(), trans.localToWorldMatrix);
            }
            else
            {
                //最小値物の場合
                parentBounds = GetBounds(trans);
            }
        }

        public void ShowChildSelection(Transform trans, CityObjectIndex index, string id)
        {
            childId = id;
            var mesh = trans.gameObject.GetComponent<MeshFilter>()?.sharedMesh;
            if (mesh == null) return;

            List<Vector2> uv = new List<Vector2>();
            mesh.GetUVs(3, uv);

            List<Vector3> vertices = new List<Vector3>();
            foreach (var (vec, i) in uv.Select((vec, i) => (vec, i)))
            {
                if ((int)Mathf.Round(vec.x) == index.PrimaryIndex && (int)Mathf.Round(vec.y) == index.AtomicIndex)
                {
                    vertices.Add(mesh.vertices[i]);
                }
            }
            childBounds = GeometryUtility.CalculateBounds(vertices.ToArray(), trans.localToWorldMatrix);
        }

        private Bounds GetBounds(Transform transform)
        {
            Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);
            var filters = transform.GetComponentsInChildren<MeshFilter>();
            foreach (var filter in filters)
            {
                if (filter.sharedMesh == null) continue;
                if (bounds.center == Vector3.zero && bounds.size == Vector3.zero)
                    bounds = GeometryUtility.CalculateBounds(filter.sharedMesh.vertices, transform.localToWorldMatrix);
                else
                    bounds.Encapsulate(GeometryUtility.CalculateBounds(filter.sharedMesh.vertices, transform.localToWorldMatrix));
            }
            return bounds;
        }
    }
}