using PLATEAU.CityInfo;
using PLATEAU.Util;
using PLATEAU.Util.GeoGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PLATEAU.RoadNetwork.Tester
{
    /// <summary>
    /// 道路ネットワークのテスト用の線分クラス
    /// 子のGameObjectの位置を頂点として扱う
    /// </summary>
    public class PLATEAURnTesterLineString : MonoBehaviour
    {
        /// <summary>
        /// 2dに射影するときの平面
        /// </summary>
        public AxisPlane plane = AxisPlane.Xy;

        /// <summary>
        /// 線分を描画するかどうか
        /// </summary>
        [Header("Draw")]
        public bool visible = true;

        /// <summary>
        /// 線分の法線を描画するかどうか
        /// </summary>
        public bool visibleNormal = false;

        /// <summary>
        /// 線分をループ表示(多角形表示)するかどうか
        /// </summary>
        public bool showLoop = false;

        /// <summary>
        /// 頂点の番号を表示する
        /// </summary>
        public bool showVertexIndex = false;

        /// <summary>
        /// 線分描画色
        /// </summary>
        public Color color = Color.white;

        /// <summary>
        /// 頂点描画時の球の半径
        /// </summary>
        public float sphereRadius = -1f;

        /// <summary>
        /// 矢印のサイズ
        /// </summary>
        public float arrowSize = 0.1f;

        [Serializable]
        public enum CopyMethod
        {
            None,
            //ConvexHull,
            Outline,
        }

        [Header("Copy PLATEAUCityObjectGroup")]
        [SerializeField]
        private PLATEAUCityObjectGroup copyTarget;

        [SerializeField] private CopyMethod copyMethod;

        public void Copy()
        {
            if (!copyTarget)
                return;
            RemoveChildren();

        }


        private IEnumerable<Transform> GetChildren(Transform self)
        {
            for (var i = 0; i < self.childCount; i++)
            {
                var child = self.GetChild(i);
                if (child.gameObject.activeInHierarchy)
                    yield return self.GetChild(i);
            }
        }

        public void RemoveChildren()
        {
            var children = GetChildren(transform).ToList();
            foreach (var child in children)
                Destroy(child.gameObject);
        }

        /// <summary>
        /// Vector2の頂点を取得
        /// </summary>
        /// <returns></returns>
        public List<Vector2> GetVertices()
        {
            return GetChildren(transform).Select(v => v.position.ToVector2(plane)).ToList();
        }

        /// <summary>
        /// Vector3の頂点を取得
        /// </summary>
        /// <returns></returns>
        public List<Vector3> GetVertices3D()
        {
            return GetChildren(transform).Select(v => v.position).ToList();
        }

        /// <summary>
        /// この順番を逆にする
        /// </summary>
        public void ReverseChildrenSibling()
        {
            for (var i = 0; i < transform.childCount; i++)
                transform.GetChild(i).SetAsFirstSibling();
        }

        /// <summary>
        /// 先頭の頂点を最後尾に移動
        /// </summary>
        public void VertexMoveFront()
        {
            if (transform.childCount > 0)
                transform.GetChild(0).SetAsLastSibling();
        }

        /// <summary>
        /// 最後尾の頂点を先頭に移動
        /// </summary>
        public void VertexMoveBack()
        {
            if (transform.childCount > 0)
                transform.GetChild(transform.childCount - 1).SetAsFirstSibling();
        }

        public void OnDrawGizmos()
        {
            if (!gameObject.activeInHierarchy)
                return;

            if (!visible)
                return;

            DebugEx.DrawArrows(GetVertices().Select(v => v.ToVector3(plane)), isLoop: showLoop, color: color, arrowSize: arrowSize);

            if (sphereRadius > 0)
            {
                foreach (var v in GetVertices())
                {
                    Gizmos.DrawSphere(v.ToVector3(plane), sphereRadius);
                }
            }

            if (showVertexIndex)
            {
                var vertices = GetVertices();
                for (var i = 0; i < vertices.Count; i++)
                {
                    var v = vertices[i].ToVector3(plane);
                    DebugEx.DrawString(i.ToString(), v);
                }
            }

            if (visibleNormal)
            {
                var vertices = GetVertices3D();
                for (var i = 0; i < vertices.Count - 1; i++)
                {
                    var v = vertices[i].PutNormal(plane, 0);
                    var next = vertices[(i + 1) % vertices.Count].PutNormal(plane, 0);
                    var p = Vector3.Lerp(v, next, 0.5f);

                    var a = plane.NormalVector();
                    var n = Vector3.Cross(a, next - v).normalized;
                    DebugEx.DrawArrow(p, p + n, bodyColor: Color.blue);
                }
            }
        }
    }
}