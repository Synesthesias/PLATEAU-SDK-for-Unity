using PLATEAU.PolygonMesh;
using PLATEAU.Util.Async;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
        private SynchronizationContext mainThreadContext;
        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            GUIStyle gizmoLabelStyle = new GUIStyle() { fontStyle = FontStyle.Bold, fontSize = 12 };

            Gizmos.color = gizmoLabelStyle.normal.textColor =　Color.green;
            Gizmos.DrawWireCube(childBounds.center, childBounds.size);
            Handles.Label(childBounds.center, childId, gizmoLabelStyle);

            Gizmos.color = gizmoLabelStyle.normal.textColor = Color.magenta;
            Gizmos.DrawWireCube(parentBounds.center, parentBounds.size);
            Handles.Label(parentBounds.center, panrentId, gizmoLabelStyle);            
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
                var verts = mesh.vertices;
                mainThreadContext = SynchronizationContext.Current;
                Task.Run(() => FindSelectionTask(true, index, uv, verts, (v) => {
                    parentBounds = GeometryUtility.CalculateBounds(v, trans.localToWorldMatrix);
                },cancellationTokenSource.Token)).ContinueWithErrorCatch();
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
            var verts = mesh.vertices;
            mainThreadContext = SynchronizationContext.Current;
            Task.Run(() => FindSelectionTask(false, index, uv, verts, (v) => {
                childBounds = GeometryUtility.CalculateBounds(v, trans.localToWorldMatrix);
            },cancellationTokenSource.Token)).ContinueWithErrorCatch();
        }

        public void ClearParentSelection()
        {
            parentBounds.size = Vector3.zero;
            panrentId = null;
        }

        public void ClearChildSelection()
        {
            childBounds.size = Vector3.zero;
            childId = null;
        }

        private void FindSelectionTask(bool primaryOnly, CityObjectIndex index, List<Vector2> uv, Vector3[] verts, Action<Vector3[]> callback, CancellationToken token)
        {
            bool primaryCheck(Vector2 vec) { return (int)Mathf.Round(vec.x) == index.PrimaryIndex; }
            bool atomicCheck(Vector2 vec) { return (int)Mathf.Round(vec.x) == index.PrimaryIndex && (int)Mathf.Round(vec.y) == index.AtomicIndex; }
            Func<Vector2, bool> indexCheck = primaryOnly ? primaryCheck : atomicCheck;
            List<Vector3> vertices = new List<Vector3>();
            foreach (var (vec, i) in uv.Select((vec, i) => (vec, i)))
            {
                if (indexCheck(vec))
                {
                    vertices.Add(verts[i]);
                }
                token.ThrowIfCancellationRequested();
            }
            this.mainThreadContext?.Post(_ =>
            {
                callback.Invoke(vertices.ToArray());
            }, null);
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

        private void OnDestroy()
        {
            mainThreadContext = null;
            cancellationTokenSource?.Cancel();
        }
    }
}