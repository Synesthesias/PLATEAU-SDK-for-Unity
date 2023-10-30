using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PLATEAU.PolygonMesh;
using UnityEditor;
using UnityEngine;

namespace PLATEAU.CityInfo
{
    /// <summary>
    /// 属性情報パネル表示時にシーン上に選択中地物の情報を表示します
    /// </summary>
    public class CityObjectGizmoDrawer : MonoBehaviour
    {
        private Bounds parentBounds;
        private Bounds childBounds;
        private string parentId;
        private string childId;
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            GUIStyle gizmoLabelStyle = new GUIStyle() { fontStyle = FontStyle.Bold, fontSize = 12 };

            Gizmos.color = gizmoLabelStyle.normal.textColor =　Color.green;
            Gizmos.DrawWireCube(childBounds.center, childBounds.size);
            Handles.Label(childBounds.center, childId, gizmoLabelStyle);

            Gizmos.color = gizmoLabelStyle.normal.textColor = Color.magenta;
            Gizmos.DrawWireCube(parentBounds.center, parentBounds.size);
            Handles.Label(parentBounds.center, this.parentId, gizmoLabelStyle);
        }
#endif

        public async Task ShowParentSelection(Transform trans, CityObjectIndex index, string id)
        {
            this.parentId = id;
            if (trans.gameObject.TryGetComponent<MeshFilter>(out var meshFilter))
            {
                var mesh = meshFilter.sharedMesh;
                if (mesh == null) return;
                List<Vector2> uv = new List<Vector2>();
                mesh.GetUVs(3, uv);
                var v = await FindSelection(true, index, uv, mesh.vertices, cancellationTokenSource.Token);
                parentBounds = GeometryUtility.CalculateBounds(v, trans.localToWorldMatrix);
            }
            else
            {
                //最小地物の場合
                parentBounds = GetBounds(trans);
            }
        }

        public async Task ShowChildSelection(Transform trans, CityObjectIndex index, string id)
        {
            childId = id;
            var mesh = trans.gameObject.GetComponent<MeshFilter>()?.sharedMesh;
            if (mesh == null) return;
            List<Vector2> uv = new List<Vector2>();
            mesh.GetUVs(3, uv);
            var v = await FindSelection(false, index, uv, mesh.vertices, cancellationTokenSource.Token);
            childBounds = GeometryUtility.CalculateBounds(v, trans.localToWorldMatrix);
        }

        private async Task<Vector3[]> FindSelection(bool primaryOnly, CityObjectIndex index, List<Vector2> uv, Vector3[] verts, CancellationToken token)
        {
            bool primaryCheck(Vector2 vec) { return (int)Mathf.Round(vec.x) == index.PrimaryIndex; }
            bool atomicCheck(Vector2 vec) { return (int)Mathf.Round(vec.x) == index.PrimaryIndex && (int)Mathf.Round(vec.y) == index.AtomicIndex; }
            Func<Vector2, bool> indexCheck = primaryOnly ? primaryCheck : atomicCheck;
            List<Vector3> vertices = new List<Vector3>();
            return await Task.Run(() =>
            {
                foreach (var (vec, i) in uv.Select((vec, i) => (vec, i)))
                {
                    if (indexCheck(vec))
                    {
                        vertices.Add(verts[i]);
                    }
                    token.ThrowIfCancellationRequested();
                }
                return vertices.ToArray();
            });
        }

        public void ClearParentSelection()
        {
            parentBounds.size = Vector3.zero;
            this.parentId = null;
        }

        public void ClearChildSelection()
        {
            childBounds.size = Vector3.zero;
            childId = null;
        }

        private Bounds GetBounds(Transform transformArg)
        {
            Bounds bounds = new Bounds(Vector3.zero, Vector3.zero);
            var filters = transformArg.GetComponentsInChildren<MeshFilter>();
            foreach (var filter in filters)
            {
                if (filter.sharedMesh == null) continue;
                if (bounds.center == Vector3.zero && bounds.size == Vector3.zero)
                    bounds = GeometryUtility.CalculateBounds(filter.sharedMesh.vertices, transformArg.localToWorldMatrix);
                else
                    bounds.Encapsulate(GeometryUtility.CalculateBounds(filter.sharedMesh.vertices, transformArg.localToWorldMatrix));
            }
            return bounds;
        }

        private void OnDestroy()
        {
            cancellationTokenSource?.Cancel();
        }
    }
}