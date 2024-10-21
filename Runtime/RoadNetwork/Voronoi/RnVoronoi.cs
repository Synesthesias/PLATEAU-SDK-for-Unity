using PLATEAU.Util;
using PLATEAU.Util.GeoGraph;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using static PLATEAU.RoadNetwork.Factory.RoadNetworkFactory;
using Vector2 = UnityEngine.Vector2;

namespace PLATEAU.RoadNetwork.Voronoi
{
    public class VPoint
    {
        public Vector2 V;

        public float x
        {
            get { return V.x; }
            set { V.x = value; }
        }

        public float y
        {
            get { return V.y; }
            set { V.y = value; }
        }

        public VPoint(Vector2 v)
        {
            V = v;
        }

        public VPoint(float x, float y)
        {
            V = new Vector2(x, y);
        }

        public VPoint() { }

        public static implicit operator Vector2(VPoint p)
        {
            return p.V;
        }
    }

    public class VEdge
    {
        /// <summary>
        /// 開始点
        /// </summary>
        public VPoint Start { get; set; }

        /// <summary>
        /// 終了点(半直線の場合はnull)
        /// </summary>
        public VPoint End { get; set; }

        /// <summary>
        /// 左側のサイトポイントインデックス
        /// </summary>
        public int LeftSiteIndex { get; set; }

        /// <summary>
        /// 右側のサイトポイント
        /// </summary>
        public int RightSiteIndex { get; set; }

        /// <summary>
        /// 辺の方向
        /// </summary>
        public Vector2 Direction { get; set; }

        /// <summary>
        /// 最後のエッジの開始地点調整用. 近傍のエッジ.最終結果には入らない
        /// </summary>
        public VEdge Neighbor { get; set; }

        /// <summary>
        /// sを開始地点として, サイトポイントl,rからなるエッジ
        /// </summary>
        /// <param name="s"></param>
        /// <param name="l"></param>
        /// <param name="r"></param>
        public VEdge(VPoint s, Vector2 direction, int leftSiteIndex, int rightSiteIndex)
        {
            Start = s;
            LeftSiteIndex = leftSiteIndex;
            RightSiteIndex = rightSiteIndex;
            Direction = direction;
            End = null;
        }

        public bool TryGetCrossPoint(VEdge other, out Vector2 p)
        {
            var ret = LineUtil.LineIntersection(
                new Ray2D(Start, Direction)
                , new Ray2D(other.Start, other.Direction)
                , out p
                , out var t1
                , out var t2
            );

            return ret && t1 >= 0 && t2 >= 0;
        }
    }

    /// <summary>
    /// 二分木のノード
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class VTreeNode<T> where T : VTreeNode<T>
    {
        /// <summary>
        /// 葉かどうか
        /// </summary>
        public bool IsLeaf { get; set; }

        /// <summary>
        /// 自身の親ノード
        /// </summary>
        public T Parent { get; set; }

        /// <summary>
        /// 左子ノード
        /// </summary>
        public T Left { get; private set; }

        /// <summary>
        /// 右子ノード
        /// </summary>
        public T Right { get; private set; }

        /// <summary>
        /// 左子ノードを設定する
        /// </summary>
        /// <param name="p"></param>
        public void SetLeft(T p)
        {
            Left = p;
            p.Parent = (T)this;
        }

        /// <summary>
        /// 右子ノードを設定する
        /// </summary>
        /// <param name="p"></param>
        public void SetRight(T p)
        {
            Right = p;
            p.Parent = (T)this;
        }


        /// <summary>
        /// pから親をたどって行く.
        /// p.Parentとpがpredicateを満たすまで親をたどる
        /// </summary>
        /// <param name="p"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        private static T FindParent(T p, Func<T, T, bool> predicate)
        {
            T node = p.Parent;
            T pLast = p;
            while (node != null && predicate(node, pLast) == false)
            {
                pLast = node;
                node = node.Parent;
            }

            return node;
        }

        /// <summary>
        /// 左側の親を取得する.
        /// fの左親はa. fの右親はc
        ///      a
        ///     /  \
        ///    b    c
        ///   / \  /  \
        ///  d   e f   g
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public static T GetLeftParent(T p)
        {
            return FindParent(p, (parent, child) => parent.Left != child);
        }


        /// <summary>
        /// 右側の親を取得する.
        /// fの左親はa. fの右親はc
        ///      a
        ///     /  \
        ///    b    c
        ///   / \  /  \
        ///  d   e f   g
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public static T GetRightParent(T p)
        {
            return FindParent(p, (parent, child) => parent.Right != child);
        }

        /// <summary>
        /// 自分の左の子供を取得する(child leftの中で最も右にある葉要素)
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public static T GetLeftChild(T p)
        {
            if (p == null)
                return null;
            var par = p.Left;
            while (!par.IsLeaf)
                par = par.Right;
            return par;
        }

        /// <summary>
        /// 自分の右の子供を取得する(child rightの中で最も左にある葉要素)
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public static T GetRightChild(T p)
        {
            if (p == null)
                return null;
            var par = p.Right;
            while (!par.IsLeaf)
                par = par.Left;
            return par;
        }
    }

    public class VoronoiWork
    {

        public static class TreeNodeEx
        { }


        /// <summary>
        /// 2分木要素である. 放物線クラス
        /// </summary>
        public class ArcParabola : VTreeNode<ArcParabola>
        {
            private VoronoiWork Work { get; }

            public Vector2 site => Work.SitePoints[SiteIndex];

            public int SiteIndex { get; set; } = -1;

            /// <summary>
            /// 右の
            /// </summary>
            public VEdge Edge { get; set; }

            /// <summary>
            /// この弧に含まれる円イベント
            /// </summary>
            public Event cEvent { get; set; }


            //public ArcParabola()
            //{
            //    IsLeaf = false;
            //}

            public ArcParabola(VoronoiWork work, int siteIndex)
            {
                Work = work;
                SiteIndex = siteIndex;
                IsLeaf = true;
            }

            public ArcParabola(VoronoiWork work, VEdge edge, ArcParabola left, ArcParabola right)
            {
                Work = work;
                Edge = edge;
                SetLeft(left);
                SetRight(right);
                IsLeaf = false;

            }
        }

        public class Event
        {
            public VPoint point { get; set; }

            public bool pe { get; set; }

            public ArcParabola arch { get; set; }

            public int SiteIndex { get; }

            public Event(VPoint v, bool isPlaceEvent, int siteIndex)
            {
                point = v;
                pe = isPlaceEvent;
                //y = v.V.y;
                arch = null;
                SiteIndex = siteIndex;
            }

        }

        /// <summary>
        /// イベントキュー
        /// </summary>
        private readonly SortedDictionary<float, Queue<Event>> EventQueue = new();

        /// <summary>
        /// 構成エッジリスト
        /// </summary>
        public List<VEdge> Edges { get; } = new List<VEdge>();

        /// <summary>
        /// 構成頂点リスト
        /// </summary>
        public List<VPoint> Points { get; } = new List<VPoint>();

        /// <summary>
        /// ビーチラインの根
        /// </summary>
        public ArcParabola Root { get; set; }

        /// <summary>
        /// 削除予定のイベント
        /// </summary>
        public HashSet<Event> DeleteEvents { get; } = new HashSet<Event>();


        public float ly = 0f;

        /// <summary>
        /// キューにイベントがある
        /// </summary>
        public bool AnyEvent => EventQueue.Any();

        public List<VPoint> SitePoints { get; private set; }

        public VoronoiWork(List<Vector2> vertices)
        {
            SitePoints = vertices.Select(v => new VPoint(v)).ToList();

            for (var i = 0; i < SitePoints.Count; ++i)
            {
                //p.V -= Min;
                var ev = new Event(SitePoints[i], true, i);
                Enqueue(ev);
            }

            while (AnyEvent)
            {
                var e = Dequeue();
                ly = e.point.y;
                if (DeleteEvents.Contains(e))
                {
                    DeleteEvents.Remove(e);
                    continue;
                }
                if (e.pe)
                    InsertParabola(e.SiteIndex);
                else
                    RemoveParabola(e);
            }

            FinishEdge();
        }


        public void Enqueue(Event e)
        {
            var k = -e.point.y; ;
            if (!EventQueue.ContainsKey(k))
                EventQueue.Add(k, new Queue<Event>());
            EventQueue[k].Enqueue(e);
        }

        public Event Dequeue()
        {
            var q = EventQueue.First();
            var e = q.Value.Dequeue();
            if (q.Value.Count == 0)
                EventQueue.Remove(q.Key);
            return e;
        }

        /// <summary>
        /// ビーチラインの中からxxに対応する放物線を取得する
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public ArcParabola GetParabolaByX(float x)
        {
            ArcParabola par = Root;
            while (!par.IsLeaf)
            {
                var tmpX = GetXOfEdge(par, ly);
                par = tmpX > x ? par.Left : par.Right;
            }

            return par;
        }

        private float GetXOfEdge(ArcParabola par, float y)
        {
            var left = ArcParabola.GetLeftChild(par);
            var right = ArcParabola.GetRightChild(par);

            var p = left.site;
            var r = right.site;

            // yがp.V.yと同じ場合放物線にならない(y軸平行な直線になる)
            // その場合の交点はp.V.xのまま
            if (RnVoronoiEx.CalcBeachLine(p, y, out var leftParabola) == false)
                return p.x;


            if (RnVoronoiEx.CalcBeachLine(r, y, out var rightParabola) == false)
                return p.x;

            var n = leftParabola.GetCrossPoint(rightParabola, out var v1, out var v2);
            if (n < 2)
                throw new InvalidDataException("");

            if (p.y < r.y)
                return Mathf.Max(v1.Value.x, v2.Value.x);

            return Mathf.Min(v1.Value.x, v2.Value.x);
        }

        public float GetY(Vector2 p, float x)
        {
            RnVoronoiEx.CalcBeachLine(p, ly, out var parabola);
            return parabola.GetY(x);
        }

        private VEdge Edge(VPoint start, int leftSiteIndex, int rightSiteIndex)
        {
            // l, rのつい直二等分線
            var l = SitePoints[leftSiteIndex];
            var r = SitePoints[rightSiteIndex];
            var direction = (new Vector2(r.y - l.y, -(r.x - l.x))).normalized;
            return new VEdge(start, direction, leftSiteIndex, rightSiteIndex);
        }

        public void InsertParabola(int siteIndex)
        {
            var p = SitePoints[siteIndex];
            if (Root == null)
            {
                Root = new ArcParabola(this, siteIndex);
                return;
            }

            if (Root.IsLeaf && Root.site.y - p.y < 1)
            {
                var fp = Root.site;
                Root.IsLeaf = false;
                Root.SetLeft(new ArcParabola(this, Root.SiteIndex));
                Root.SetRight(new ArcParabola(this, siteIndex));
                // VPoint * s = new VPoint((p->x + fp->x)/2, height);
                VPoint s = new VPoint(p.x, GetY(Root.site, p.x));
                Points.Add(s);
                Root.Edge = p.x > fp.x ? Edge(s, Root.SiteIndex, siteIndex) : Edge(s, siteIndex, Root.SiteIndex);
                Edges.Add(Root.Edge);
                return;
            }

            ArcParabola par = GetParabolaByX(p.x);

            if (par.cEvent != null)
            {
                DeleteEvents.Add(par.cEvent);
                par.cEvent = null;
            }

            VPoint start = new VPoint(p.x, GetY(par.site, p.x));
            Points.Add(start);

            VEdge el = Edge(start, par.SiteIndex, siteIndex);
            VEdge er = Edge(start, siteIndex, par.SiteIndex);

            el.Neighbor = er;
            Edges.Add(el);

            par.Edge = er;
            par.IsLeaf = false;

            var p0 = new ArcParabola(this, par.SiteIndex);
            var p1 = new ArcParabola(this, siteIndex);
            var p2 = new ArcParabola(this, par.SiteIndex);

            par.SetRight(p2);
            par.SetLeft(new ArcParabola(this, el, p0, p1));

            CheckCircle(p0);
            CheckCircle(p2);
        }

        private void CheckCircle(ArcParabola b)
        {
            var lp = ArcParabola.GetLeftParent(b);
            var rp = ArcParabola.GetRightParent(b);

            var leftChild = ArcParabola.GetLeftChild(lp);
            var rightChild = ArcParabola.GetRightChild(rp);

            // 
            if (leftChild == null || rightChild == null || leftChild.SiteIndex == rightChild.SiteIndex)
                return;

            var s = GetEdgeIntersection(lp.Edge, rp.Edge);
            if (s == null)
                return;

            float dx = leftChild.site.x - s.x;
            float dy = leftChild.site.y - s.y;

            float d = Mathf.Sqrt((dx * dx) + (dy * dy));

            if (s.y - d >= ly) { return; }

            Event e = new Event(new VPoint(s.x, s.y - d), false, -1);
            Points.Add(e.point);
            b.cEvent = e;
            e.arch = b;
            Enqueue(e);
        }

        private VPoint GetEdgeIntersection(VEdge a, VEdge b)
        {
            if (a.TryGetCrossPoint(b, out var v) == false)
                return null;

            VPoint p = new VPoint(v.x, v.y);
            Points.Add(p);
            return p;
        }

        public void RemoveParabola(Event e)
        {
            ArcParabola p1 = e.arch;

            var xl = ArcParabola.GetLeftParent(p1);
            var xr = ArcParabola.GetRightParent(p1);
            var p0 = ArcParabola.GetLeftChild(xl);
            var p2 = ArcParabola.GetRightChild(xr);

            if (p0.cEvent != null)
            {
                DeleteEvents.Add(p0.cEvent);
                p0.cEvent = null;
            }

            if (p2.cEvent != null)
            {
                DeleteEvents.Add(p2.cEvent);
                p2.cEvent = null;
            }

            VPoint p = new VPoint(e.point.x, GetY(p1.site, e.point.x));
            Points.Add(p);

            xl.Edge.End = p;
            xr.Edge.End = p;

            ArcParabola higher = null;
            ArcParabola par = p1;
            while (par != Root)
            {
                par = par.Parent;
                if (par == xl)
                    higher = xl;
                if (par == xr)
                    higher = xr;
            }

            higher.Edge = Edge(p, p0.SiteIndex, p2.SiteIndex);
            Edges.Add(higher.Edge);

            ArcParabola gparent = p1.Parent.Parent;
            if (p1.Parent.Left == p1)
            {
                if (gparent.Left == p1.Parent)
                    gparent.SetLeft(p1.Parent.Right);
                if (gparent.Right == p1.Parent)
                    gparent.SetRight(p1.Parent.Right);
            }
            else
            {
                if (gparent.Left == p1.Parent)
                    gparent.SetLeft(p1.Parent.Left);
                if (gparent.Right == p1.Parent)
                    gparent.SetRight(p1.Parent.Left);
            }


            CheckCircle(p0);
            CheckCircle(p2);
        }

        public void FinishEdge()
        {
            foreach (var e in Edges)
            {
                if (e.Neighbor != null)
                {
                    e.Start = e.Neighbor.End;
                    e.Neighbor = null;
                }
            }
        }
    }

    public class VoronoiData<T>
    {
        public class Edge
        {
            /// <summary>
            /// 開始点
            /// </summary>
            public Vector2? Start { get; set; }

            /// <summary>
            /// 終了点(半直線の場合はnull)
            /// </summary>
            public Vector2? End { get; set; }

            /// <summary>
            /// 左側のサイトポイントインデックス
            /// </summary>
            public T LeftSitePoint { get; set; }

            /// <summary>
            /// 右側のサイトポイント
            /// </summary>
            public T RightSitePoint { get; set; }

            /// <summary>
            /// 辺の方向
            /// </summary>
            public Vector2 Direction { get; set; }
        }

        public List<T> SitePoints { get; set; }

        /// <summary>
        /// 構成エッジリスト
        /// </summary>
        public List<Edge> Edges { get; set; } = new List<Edge>();

        /// <summary>
        /// 構成頂点リスト
        /// </summary>
        public List<VPoint> Points { get; set; } = new List<VPoint>();

    }

    public static class RnVoronoiEx
    {

        public static VoronoiData<T> CalcVoronoiData<T>(List<T> points, Func<T, Vector2> toVec2)
        {
            var sitePoints = points.Select(p => toVec2(p)).ToList();
            var work = new VoronoiWork(sitePoints);

            return new VoronoiData<T>
            {
                SitePoints = points,
                Edges = work.Edges.Select(e => new VoronoiData<T>.Edge
                {
                    Start = e.Start?.V,
                    End = e.End?.V,
                    LeftSitePoint = points[e.LeftSiteIndex],
                    RightSitePoint = points[e.RightSiteIndex],
                    Direction = e.Direction
                }).ToList(),
                Points = work.Points
            };
        }

        public struct Parabola
        {
            public float a;
            public float b;
            public float c;

            public Parabola(float a, float b, float c)
            {
                this.a = a;
                this.b = b;
                this.c = c;
            }

            public float GetY(float x)
            {
                return a * x * x + b * x + c;
            }

            /// <summary>
            /// y = 0となるxを求める
            /// </summary>
            /// <returns></returns>
            public int Solve(out float? x1, out float? x2)
            {
                return SolveQuadratic(a, b, c, out x1, out x2);
            }

            /// <summary>
            /// 交点を求める
            /// </summary>
            /// <param name="other"></param>
            /// <param name="p1"></param>
            /// <param name="p2"></param>
            /// <returns></returns>
            public int GetCrossPoint(Parabola other, out Vector2? p1, out Vector2? p2)
            {
                var p = new Parabola(a - other.a, b - other.b, c - other.c);
                var n = p.Solve(out var x1, out var x2);
                p1 = x1 != null ? new Vector2(x1.Value, GetY(x1.Value)) : null;
                p2 = x2 != null ? new Vector2(x2.Value, GetY(x2.Value)) : null;
                return n;
            }

        }

        // 2次方程式の解を返す関数
        // 戻り値は解の個数
        public static int SolveQuadratic(float a, float b, float c, out float? x1, out float? x2)
        {
            x1 = x2 = null;
            // 判別式の計算
            float discriminant = b * b - 4 * a * c;

            // 判別式が負の場合、実数解は存在しない
            if (discriminant < 0)
                return 0;

            // 判別式が0の時、重解
            if (discriminant == 0)
            {
                x1 = -b / (2 * a);
                return 1;
            }

            // 解の計算
            float sqrtDiscriminant = Mathf.Sqrt(discriminant);
            x1 = (-b + sqrtDiscriminant) / (2 * a);
            x2 = (-b - sqrtDiscriminant) / (2 * a);
            return 2;
        }

        public static bool CalcBeachLine(Vector2 p, float lineY, out Parabola ret)
        {
            float dp = 2 * (p.y - lineY);
            if (dp <= 0)
            {
                ret = new Parabola();
                return false;
            }
            dp = 1 / dp;
            var a = dp;
            var b = -2 * p.x * dp;
            var c = (p.x * p.x - lineY * lineY + p.y * p.y) * dp;
            ret = new Parabola(a, b, c);
            return true;
        }
    }

}