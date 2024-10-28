using PLATEAU.Util;
using PLATEAU.Util.GeoGraph;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Assertions;
using static Codice.Client.Commands.WkTree.WorkspaceTreeNode;
using static PLATEAU.RoadNetwork.Util.LineCrossPointResult;
using static PLATEAU.RoadNetwork.Voronoi.RnVoronoiEx;

namespace PLATEAU.RoadNetwork.Voronoi
{
    public struct Vector2d
    {
        public double x;

        public double y;

        public Vector2d(double x, double y)
        {
            this.x = x;
            this.y = y;
        }

        public Vector2d(Vector2 v)
        {
            x = v.x;
            y = v.y;
        }

        public double magnitude => Math.Sqrt(x * x + y * y);

        public Vector2d normalized
        {
            get
            {
                var m = magnitude;
                return new Vector2d(x / m, y / m);
            }
        }

        public static Vector2d operator +(Vector2d a, Vector2d b)
        {
            return new Vector2d(a.x + b.x, a.y + b.y);
        }

        public static Vector2d operator -(Vector2d a, Vector2d b)
        {

            return new Vector2d(a.x - b.x, a.y - b.y);
        }

        public static Vector2d operator *(Vector2d a, double x)
        {
            return new Vector2d(a.x * x, a.y * x);
        }

        public static Vector2d operator *(Vector2d a, int x)
        {
            return new Vector2d(a.x * x, a.y * x);
        }

        public static Vector2d operator *(Vector2d a, float x)
        {
            return new Vector2d(a.x * x, a.y * x);
        }

        public static Vector2d operator -(Vector2d a)
        {
            return new Vector2d(-a.x, -a.y);
        }


        public static bool operator ==(Vector2d a, Vector2d b)
        {
            return a.x == b.x && a.y == b.y;
        }

        public static bool operator !=(Vector2d a, Vector2d b)
        {
            return !(a == b);
        }

        public Vector2 ToVector2() => new Vector2((float)x, (float)y);

        /// <summary>
        /// Vector2の外積. ret = a.x * b.y - a.y * b.x
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static double Cross(Vector2d a, Vector2d b)
        {
            return a.x * b.y - a.y * b.x;
        }

        public static Vector2d LerpUnclamped(Vector2d a, Vector2d b, double t) => new Vector2d(a.x + (b.x - a.x) * t, a.y + (b.y - a.y) * t);
    }

    public class VPoint
    {
        public Vector2d V;

        public double x
        {
            get { return V.x; }
            set { V.x = value; }
        }

        public double y
        {
            get { return V.y; }
            set { V.y = value; }
        }

        public VPoint(Vector2d v)
        {
            V = v;
        }

        public VPoint(double x, double y)
        {
            V = new Vector2d(x, y);
        }

        public VPoint() { }

        public static implicit operator Vector2d(VPoint p)
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
        public Vector2d Direction { get; }

        /// <summary>
        /// 最後のエッジの開始地点調整用. 近傍のエッジ.最終結果には入らない
        /// </summary>
        public VEdge Neighbor { get; set; }

        /// <summary>
        /// sを開始地点として, サイトポイントl,rからなるエッジ
        /// </summary>
        /// <param name="start"></param>
        /// <param name="direction"></param>
        /// <param name="leftSiteIndex"></param>
        /// <param name="rightSiteIndex"></param>
        public VEdge(VPoint start, Vector2d direction, int leftSiteIndex, int rightSiteIndex)
        {
            Start = start;
            LeftSiteIndex = leftSiteIndex;
            RightSiteIndex = rightSiteIndex;
            Direction = direction.normalized;
            End = null;
        }

        /// <summary>
        /// 半直線a,bの交点を求める
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="p"></param>
        /// <param name="epsilon"></param>
        /// <returns></returns>
        public static bool TryGetCrossPoint(VEdge a, VEdge b, out Vector2d p, double epsilon)
        {
            p = new Vector2d();

            // sa + da * t1 = sb + db * t2
            // => sa_x + da_x * t1 = sb_x + db_x * t2
            //    sa_y + da_y * t1 = sb_y + db_y * t2
            // => da_y * sa_x + da_y * da_x * t1 = da_y * sb_x + da_y * db_x * t2
            //    da_x * sa_y + da_x * da_y * t1 = da_x * sb_y + da_x * db_y * t2
            // => da_y * sa_x - da_x * sa_y  = da_y * sb_x - da_x * sb_y + (da_y * db_x - da_x * db_y ) * t2
            // => sa×da = sb×da + (db×da) * t2
            // => t2 = (sa×da - sb×da) / db×da
            // => t2 = ((sa - sb)×da) / db×da = da×(sa - sb) / da×db
            // t1も同様
            var deno = Vector2d.Cross(a.Direction, b.Direction);
            if (Math.Abs(deno) <= 1e-6)
                return false;
            var t1 = Vector2d.Cross(b.Start.V - a.Start.V, b.Direction) / deno;
            var t2 = Vector2d.Cross(a.Direction, a.Start.V - b.Start.V) / deno;

            p = a.Start + a.Direction * t1;
            //return true;
            return (t1 >= -epsilon || Math.Abs(a.Direction.y) <= epsilon) && (t2 >= -epsilon || Math.Abs(b.Direction.y) <= epsilon);
        }

        public Vector2 ToVector2() => new Vector2((float)Start.x, (float)Start.y);
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
        public bool IsLeaf => Left == null && Right == null;

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
        /// 一つ左のLeafを取得する
        /// </summary>
        public T LeftLeaf
        {
            get
            {
                if (IsLeaf == false)
                    return null;
                var lp = GetLeftParent((T)this);

                return GetLeftChild(lp);
            }
        }

        public T RightLeaf
        {
            get
            {
                if (IsLeaf == false)
                    return null;
                var rp = GetRightParent((T)this);
                return GetRightChild(rp);
            }
        }

        /// <summary>
        /// Left側の子の中で最も右にある葉要素)
        /// </summary>
        /// <returns></returns>
        public T ChildLeftLeaf
        {
            get
            {
                return GetLeftChild((T)this);
            }
        }

        /// <summary>
        /// Right側の子の中で最も左にある葉要素)
        /// </summary>
        /// <returns></returns>
        public T ChildRightLeaf
        {
            get
            {
                return GetRightChild((T)this);
            }
        }

        /// <summary>
        /// 左子ノードを設定する
        /// </summary>
        /// <param name="p"></param>
        public void SetLeft(T p)
        {
            Left = p;
            if (p != null)
                p.Parent = (T)this;
        }

        /// <summary>
        /// 右子ノードを設定する
        /// </summary>
        /// <param name="p"></param>
        public void SetRight(T p)
        {
            Right = p;
            if (p != null)
                p.Parent = (T)this;
        }

        /// <summary>
        /// 自身がLeafの時, 自身をツリーから削除する
        /// </summary>
        public void RemoveLeaf()
        {
            if (IsLeaf == false)
                return;

            // 自分がルートの場合は削除しない(もともと孤立しているから)
            if (Parent == null)
                return;


            // 2世代上
            var grandParent = Parent.Parent;
            // 親がルートの場合は削除で終わり
            if (grandParent == null)
            {
                if (Parent.Left == this)
                    Parent.SetLeft(null);
                else
                    Parent.SetRight(null);
                return;
            }

            // 親がルートでない場合
            if (Parent.Left == this)
            {
                // gからみてLeft -> Leftの位置にp1がある場合
                // pの部分にxを入れる
                //     g            g
                //    /  \         /  \
                //   p    *   ->  x    *
                //  /  \
                // p1   x
                if (grandParent.Left == Parent)
                    grandParent.SetLeft(Parent.Right);

                // gからみてRight -> Rightの位置にp1がある場合
                // pの部分にxを入れる
                //     g            g
                //    /  \         /  \
                //    *   p   ->  *    x
                //       /  \
                //      p1   x
                if (grandParent.Right == Parent)
                    grandParent.SetRight(Parent.Right);
            }
            else
            {
                // gからみてLeft -> Rightの位置にp1がある場合
                // pの部分にxを入れる
                //     g            g
                //    /  \         /  \
                //   p    *   ->  x    *
                //  /  \
                // x   p1
                if (grandParent.Left == Parent)
                    grandParent.SetLeft(Parent.Left);

                // gからLeft -> Leftの形式の場合
                // pの部分にxを入れる
                //     g            g
                //    /  \         /  \
                //    *   p   ->  *    x
                //       /  \
                //      x   p1
                if (grandParent.Right == Parent)
                    grandParent.SetRight(Parent.Left);
            }

            //Parent = null;
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
        /// 自分の左の子供を取得する(p.Left以下で最も右にある葉要素)
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public static T GetLeftChild(T p)
        {
            if (p == null)
                return null;
            var par = p.Left;
            if (par == null)
                return null;
            while (!par.IsLeaf)
                par = par.Right;
            return par;
        }

        /// <summary>
        /// 自分の右の子供を取得する(p.Right以下で左にある葉要素)
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public static T GetRightChild(T p)
        {
            if (p == null)
                return null;
            var par = p.Right;
            if (par == null)
                return null;
            while (!par.IsLeaf)
                par = par.Left;
            return par;
        }



        public IEnumerable<T> GetAll()
        {
            if (IsLeaf)
            {
                yield return (T)this;
            }
            else
            {
                if (Left != null)
                {
                    foreach (var l in Left.GetAll())
                        yield return l;
                }
                if (Right != null)
                {
                    foreach (var r in Right.GetAll())
                        yield return r;
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
            public Vector2d? Start { get; set; }

            /// <summary>
            /// 終了点(半直線の場合はnull)
            /// </summary>
            public Vector2d? End { get; set; }

            /// <summary>
            /// 左側のサイトポイント
            /// </summary>
            public T LeftSitePoint { get; set; }

            /// <summary>
            /// 右側のサイトポイント
            /// </summary>
            public T RightSitePoint { get; set; }

            /// <summary>
            /// 左側のサイトポイントインデックス
            /// </summary>
            public int LeftSiteIndex { get; set; }

            /// <summary>
            /// 右側のサイトポイントインデックス
            /// </summary>
            public int RightSiteIndex { get; set; }

            /// <summary>
            /// 辺の方向
            /// </summary>
            public Vector2d Direction { get; set; }
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
        private const double Epsilon = 1e-8;

        private static bool IsZero(double x) => Math.Abs(x) <= Epsilon;

        /// <summary>
        /// 2分木要素である. 放物線クラス
        /// </summary>
        public class ArcParabola : VTreeNode<ArcParabola>
        {
            private Work Work { get; }

            public Vector2d site => Work.SitePoints[SiteIndex];

            public int SiteIndex { get; set; } = -1;

            /// <summary>
            /// 右の
            /// </summary>
            public VEdge Edge { get; set; }

            /// <summary>
            /// この弧に含まれる円イベント
            /// </summary>
            public Event cEvent { get; set; }

            public ArcParabola(Work work, int siteIndex)
            {
                Work = work;
                SiteIndex = siteIndex;
                //IsLeaf = true;
            }

            public ArcParabola(Work work, VEdge edge, ArcParabola left, ArcParabola right)
            {
                Work = work;
                Edge = edge;
                SetLeft(left);
                SetRight(right);
                // IsLeaf = false;

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

        private class QueueComparer : IComparer<Vector2d>
        {
            public int Compare(Vector2d a, Vector2d b)
            {
                // yは大きい順
                var dy = Math.Sign(b.y - a.y);
                if (dy != 0)
                    return dy;
                // xは小さい順
                return Math.Sign(a.x - b.x);
            }
        }

        public class Work
        {
            /// <summary>
            /// イベントキュー
            /// </summary>
            //private SortedDictionary<double, Queue<Event>> EventQueue { get; } = new();

            private SortedList<Vector2d, Queue<Event>> EventQueue { get; } = new(new QueueComparer());

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


            public double ly = 0f;

            /// <summary>
            /// キューにイベントがある
            /// </summary>
            public bool AnyEvent => EventQueue.Any();

            public List<VPoint> SitePoints { get; private set; }

            public Work(List<Vector2d> vertices)
            {
                SitePoints = vertices.Select(v => new VPoint(v)).ToList();
            }


            public void Enqueue(Event e)
            {
                if (EventQueue.ContainsKey(e.point) == false)
                    EventQueue.Add(e.point, new Queue<Event>());
                EventQueue[e.point].Enqueue(e);
            }

            public Event Dequeue()
            {
                var q = EventQueue.First();
                var ret = q.Value.Dequeue();
                if (q.Value.Count == 0)
                    EventQueue.RemoveAt(0);
                return ret;
            }


            public bool TryGetY(Vector2d p, double x, out double y)
            {
                if (RnVoronoiEx.CalcBeachLine(p, ly, out var parabola))
                {
                    y = parabola.GetY(x);
                    return true;
                }

                y = 0.0;
                return false;
            }

            private VEdge Edge(VPoint start, int leftSiteIndex, int rightSiteIndex)
            {// l, rの垂直二等分線
                var l = SitePoints[leftSiteIndex];
                var r = SitePoints[rightSiteIndex];
                var direction = (new Vector2d(r.y - l.y, -(r.x - l.x))).normalized;
                return new VEdge(start, direction, leftSiteIndex, rightSiteIndex);
            }

            private void Edge(VPoint start, int leftSiteIndex, int rightSiteIndex, out VEdge leftEdge, out VEdge rightEdge)
            {
                // l, rの垂直二等分線
                var l = SitePoints[leftSiteIndex];
                var r = SitePoints[rightSiteIndex];
                var direction = (new Vector2d(r.y - l.y, -(r.x - l.x))).normalized;
                leftEdge = new VEdge(start, direction, leftSiteIndex, rightSiteIndex);
                rightEdge = new VEdge(start, -direction, rightSiteIndex, leftSiteIndex);

                leftEdge.Neighbor = rightEdge;
            }


            private double GetXOfEdge(ArcParabola par, double y)
            {
                var left = ArcParabola.GetLeftChild(par);
                var right = ArcParabola.GetRightChild(par);

                var l = left.site;
                var r = right.site;

                // yがp.V.yと同じ場合放物線にならない(y軸平行な直線になる)
                // その場合の交点はp.V.xのまま
                if (RnVoronoiEx.CalcBeachLine(l, y, out var leftParabola) == false)
                {
                    return l.x;
                }

                if (RnVoronoiEx.CalcBeachLine(r, y, out var rightParabola) == false)
                {
                    return r.x;
                }

                var n = leftParabola.GetCrossPoint(rightParabola, out var v1, out var v2);

                if (n == 0)
                    throw new InvalidDataException("x");

                if (n == 1)
                    return v1.x;

                // rの方が先に出てきた場合lの左右で交わる -> lの右側
                if (l.y < r.y)
                    return Math.Max(v1.x, v2.x);

                // lの方が先に出てきた場合rの左右で交わる -> rの左側
                if (r.y < l.y)
                    return Math.Min(v1.x, v2.x);

                throw new InvalidDataException("x");
            }

            public bool GetCrossPoint(ArcParabola left, ArcParabola right, double y, out double x)
            {
                x = 0.0;
                var l = left.site;
                var r = right.site;
                if (RnVoronoiEx.CalcBeachLine(left.site, y, out var leftParabola) == false)
                {
                    return false;
                }

                if (RnVoronoiEx.CalcBeachLine(r, y, out var rightParabola) == false)
                {
                    return false;
                }

                var n = leftParabola.GetCrossPoint(rightParabola, out var v1, out var v2);
                //if (n < 2)
                //    throw new InvalidDataException("");
                // rの方が先に出てきた場合lの左右で交わる -> lの右側
                if (l.y < r.y)
                    x = Math.Max(v1.x, v2.x);

                // lの方が先に出てきた場合rの左右で交わる -> rの左側
                if (r.y < l.y)
                    x = Math.Min(v1.x, v2.x);
                return true;
            }

            /// <summary>
            /// ビーチラインの中からxに対応する放物線を取得する
            /// </summary>
            /// <param name="x"></param>
            /// <returns></returns>
            public void GetParabolaByX(double x, out ArcParabola centerLeft, out ArcParabola centerRight)
            {
                //ArcParabola ret = null;
                //foreach (var p in Root.GetAll())
                //{
                //    if (RnVoronoiEx.CalcBeachLine(p.site, ly, out var pp) == false)
                //    {
                //        ret = p;
                //        continue;
                //    }

                //    var l = ArcParabola.GetRightChild(ArcParabola.GetLeftParent(p));
                //    var r = ArcParabola.GetLeftChild(ArcParabola.GetRightParent(p));

                //    if (l == null && r == null)
                //        continue;
                //    if (r == null)
                //    {
                //        if (GetCrossPoint(p, l, ly, out var xx))
                //        {
                //            if (xx <= x)
                //                return p;
                //        }
                //    }
                //    else if (l == null)
                //    {
                //        if (GetCrossPoint(p, r, ly, out var xx))
                //        {
                //            if (xx >= x)
                //                return p;
                //        }
                //    }
                //    else
                //    {
                //        if (GetCrossPoint(p, l, ly, out var xx1) && GetCrossPoint(p, r, ly, out var xx2))
                //        {
                //            if (xx1 <= x && x <= xx2)
                //                return p;
                //        }
                //    }
                //}

                //return ret;
                var ret = new List<ArcParabola>();
                centerLeft = centerRight = null;
                ArcParabola par = Root;
                while (!par.IsLeaf)
                {
                    var tmpX = GetXOfEdge(par, ly);
                    if (Math.Abs(tmpX - x) <= Epsilon)
                    {
                        centerLeft = par.ChildLeftLeaf;
                        centerRight = par.ChildRightLeaf;
                        return;
                    }
                    par = tmpX >= x ? par.Left : par.Right;
                }

                centerLeft = centerRight = par;
                return;
            }

            /// <summary>
            /// 放物線を挿入する
            /// </summary>
            /// <param name="siteIndex"></param>
            public void InsertParabola(int siteIndex)
            {
                var newSite = SitePoints[siteIndex];
                if (Root == null)
                {
                    Root = new ArcParabola(this, siteIndex);
                    return;
                }

                // サイトポイントが二つでかつその二つが交わらない(y値が同じ時)
                //if (Root.IsLeaf && Root.site.y - newSite.y <= Epsilon)
                //{
                //    var (leftIndex, rightIndex) = (Root.SiteIndex, siteIndex);
                //    if (Root.site.x > newSite.x)
                //        (leftIndex, rightIndex) = (rightIndex, leftIndex);
                //    // 両者の垂直2等分線はそれぞれの中心点にある
                //    // VPoint * s = new VPoint((p->x + fp->x)/2, height);
                //    VPoint start = new VPoint((newSite + Root.site) * 0.5);
                //    Points.Add(start);
                //    Edge(start, leftIndex, rightIndex, out var el, out var er);
                //    Root.Edge = el;
                //    el.Neighbor = er;
                //    Edges.Add(Root.Edge);
                //    Root.SetLeft(new ArcParabola(this, leftIndex));
                //    Root.SetRight(new ArcParabola(this, rightIndex));
                //}
                //else
                {
                    var allLeaf = Root.GetAll().ToList();
                    var h = allLeaf.FirstOrDefault(x => x.SiteIndex == 51);
                    GetParabolaByX(newSite.x, out var centerLeft, out var centerRight);
                    if (centerLeft != centerRight)
                    {
                        var ps = new List<ArcParabola>();
                        foreach (var center in new[] { centerLeft, centerRight })
                        {
                            if (center.cEvent != null)
                            {
                                DeleteEvents.Add(center.cEvent);
                                center.cEvent = null;
                            }

                            if (TryGetY(center.site, newSite.x, out var newSiteY) == false)
                                throw new InvalidDataException("TryGetY");

                            VPoint start = new VPoint(newSite.x, newSiteY);
                            Points.Add(start);

                            // startを起点に二つの半直線を作成
                            Edge(start, center.SiteIndex, siteIndex, out var elLeft, out var erLeft);
                            //if (er.Direction.y > 0)
                            //     (el, er) = (er, el);
                            //elLeft.Neighbor = erLeft;
                            Edges.Add(elLeft);
                            center.Edge = erLeft;

                            var p0 = new ArcParabola(this, center.SiteIndex);
                            var p1 = new ArcParabola(this, siteIndex);
                            var p2 = new ArcParabola(this, center.SiteIndex);

                            // startを起点に二つの半直線を作成
                            //Edge(start, siteIndex, center.SiteIndex, out var elRight, out var erRight);
                            center.SetRight(p2);
                            center.SetLeft(new ArcParabola(this, elLeft, p0, p1));
                            var allLeaf2 = Root.GetAll().ToList();
                            ps.Add(p0);
                            ps.Add(p2);
                        }
                        foreach (var pp in ps)
                            CheckCircle(pp);
                    }
                    else
                    {
                        var center = centerLeft;
                        if (center.cEvent != null)
                        {
                            DeleteEvents.Add(center.cEvent);
                            center.cEvent = null;
                        }

                        var isSameY = (center.site.y - newSite.y) <= Epsilon;
                        if (isSameY)
                        {
                            var (leftIndex, rightIndex) = (center.SiteIndex, siteIndex);
                            if (center.site.x > newSite.x)
                                (leftIndex, rightIndex) = (rightIndex, leftIndex);

                            VPoint start = new VPoint((newSite + center.site) * 0.5);
                            Points.Add(start);
                            Edge(start, leftIndex, rightIndex, out var el, out var er);
                            //el.Neighbor = er;
                            Edges.Add(el);
                            center.Edge = el;

                            var arcLeft = new ArcParabola(this, leftIndex);
                            var arcRight = new ArcParabola(this, rightIndex);
                            center.SetRight(arcRight);
                            center.SetLeft(arcLeft);

                            CheckCircle(arcRight);
                            CheckCircle(arcLeft);
                        }
                        else
                        {
                            // center.site.y > newSite.yなのでTryGetYは成功する
                            TryGetY(center.site, newSite.x, out var newSiteY);
                            VPoint start = new VPoint(newSite.x, newSiteY);
                            Points.Add(start);

                            // startを起点に二つの半直線を作成
                            Edge(start, center.SiteIndex, siteIndex, out var el, out var er);
                            //if (er.Direction.y > 0)
                            //     (el, er) = (er, el);
                            //el.Neighbor = er;
                            Edges.Add(el);
                            center.Edge = er;

                            var p0 = new ArcParabola(this, center.SiteIndex);
                            var p1 = new ArcParabola(this, siteIndex);
                            var p2 = new ArcParabola(this, center.SiteIndex);

                            center.SetRight(p2);
                            center.SetLeft(new ArcParabola(this, el, p0, p1));
                            var allLeaf2 = Root.GetAll().ToList();
                            CheckCircle(p0);
                            CheckCircle(p2);
                        }
                    }

                }

            }

            /// <summary>
            /// 円イベントチェック
            /// </summary>
            /// <param name="b"></param>
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

                var del = leftChild.site - s;
                double d = del.magnitude;
                if (s.y - d > ly) { return; }
                //if (s.y - d >= ly) { return; }
                Event e = new Event(new VPoint(s.x, s.y - d), false, -1);
                Points.Add(e.point);
                b.cEvent = e;
                e.arch = b;
                Enqueue(e);
            }

            private VPoint GetEdgeIntersection(VEdge a, VEdge b)
            {
                if (VEdge.TryGetCrossPoint(a, b, out var v, Epsilon) == false)
                    return null;

                VPoint p = new VPoint(v.x, v.y);
                Points.Add(p);
                return p;
            }

            /// <summary>
            /// e.archの弧を削除する
            /// </summary>
            /// <param name="e"></param>
            public void RemoveParabola(Event e)
            {
                // p1が消える
                ArcParabola p1 = e.arch;

                var xl = ArcParabola.GetLeftParent(p1);
                var xr = ArcParabola.GetRightParent(p1);
                var p0 = ArcParabola.GetLeftChild(xl);
                var p2 = ArcParabola.GetRightChild(xr);
                if (TryGetY(p1.site, e.point.x, out var pointY) == false)
                    return;
                VPoint p = new VPoint(e.point.x, pointY);

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

                Points.Add(p);

                xl.Edge.End = p;
                xr.Edge.End = p;


                // 自分を削除する
                p1.RemoveLeaf();

                ArcParabola higher = null;
                ArcParabola par = p1.Parent;
                while (par != null)
                {
                    if (par.ChildLeftLeaf == p0 && par.ChildRightLeaf == p2)
                    {
                        higher = par;
                        //break;
                    }
                    par = par.Parent;
                }

                //while (par != Root)
                //{
                //    par = par.Parent;
                //    if (par.ChildLeftLeaf == xl && par.ChildRightLeaf == xr)
                //    {
                //        higher = par;
                //        //break;
                //    }
                //    //if (par == xl)
                //    //    higher = xl;
                //    //if (par == xr)
                //    //    higher = xr;
                //}



                higher.Edge = Edge(p, p0.SiteIndex, p2.SiteIndex);
                Edges.Add(higher.Edge);
                p1.Parent = null;


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


        public static VoronoiData<T> CalcVoronoiData<T>(List<T> points, Func<T, Vector2d> toVec2)
        {
            var sitePoints = points.Select(p => toVec2(p)).ToList();
            var work = new Work(sitePoints);

            for (var i = 0; i < work.SitePoints.Count; ++i)
            {
                //p.V -= Min;
                var ev = new Event(work.SitePoints[i], true, i);
                work.Enqueue(ev);
            }

            while (work.AnyEvent)
            {
                var e = work.Dequeue();
                work.ly = e.point.y;
                if (work.DeleteEvents.Contains(e))
                {
                    work.DeleteEvents.Remove(e);
                    continue;
                }
                if (e.pe)
                    work.InsertParabola(e.SiteIndex);
                else
                    work.RemoveParabola(e);
            }

            work.FinishEdge();

            return new VoronoiData<T>
            {
                SitePoints = points,
                Edges = work.Edges.Select(e => new VoronoiData<T>.Edge
                {
                    Start = e.Start?.V,
                    End = e.End?.V,
                    LeftSitePoint = points[e.LeftSiteIndex],
                    RightSitePoint = points[e.RightSiteIndex],
                    LeftSiteIndex = e.LeftSiteIndex,
                    RightSiteIndex = e.RightSiteIndex,
                    Direction = e.Direction
                }).ToList(),
                Points = work.Points
            };
        }

        /// <summary>
        /// y = a*x^2 + b*x + c
        /// </summary>
        public struct Parabola
        {
            public double a;
            public double b;
            public double c;

            public Parabola(double a, double b, double c)
            {
                this.a = a;
                this.b = b;
                this.c = c;
            }

            public double GetY(double x)
            {
                return a * x * x + b * x + c;
            }

            /// <summary>
            /// y = 0となるxを求める
            /// </summary>
            /// <returns></returns>
            public int Solve(out double x1, out double x2)
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
            public int GetCrossPoint(Parabola other, out Vector2d p1, out Vector2d p2)
            {
                p1 = p2 = new Vector2d();
                var p = new Parabola(a - other.a, b - other.b, c - other.c);
                var n = p.Solve(out var x1, out var x2);
                if (n == 0)
                    return n;
                p1 = new Vector2d(x1, GetY(x1));
                p2 = new Vector2d(x2, GetY(x2));
                return n;
            }

        }

        // 2次方程式の解を返す関数
        // 戻り値は解の個数
        public static int SolveQuadratic(double a, double b, double c, out double x1, out double x2)
        {
            x1 = x2 = 0.0;
            if (IsZero(a))
            {
                if (IsZero(b))
                    return 0;

                x1 = x2 = -c / b;
                return 1;
            }

            if (IsZero(b))
            {
                if (IsZero(c))
                {
                    x1 = x2 = 0.0;
                    return 1;
                }

                if (Math.Sign(b) == Math.Sign(c))
                    return 0;

                x1 = Math.Sqrt(-c / a);
                x2 = -x1;
                return 2;
            }


            // 判別式の計算
            double d = b * b - 4 * a * c;

            // 判別式が負の場合、実数解は存在しない
            if (d < 0)
                return 0;

            // 判別式が0の時、重解
            if (d == 0)
            {
                x1 = x2 = -b / (2 * a);
                return 1;
            }

            // 解の計算
            double sqrtDiscriminant = Math.Sqrt(d);
            x1 = (-b + sqrtDiscriminant) / (2 * a);
            x2 = (-b - sqrtDiscriminant) / (2 * a);
            return 2;
        }

        /// <summary>
        /// ビーチライン計算. p.y &lt; lineYの時は存在しない
        /// </summary>
        /// <param name="p"></param>
        /// <param name="lineY"></param>
        /// <param name="ret"></param>
        /// <returns></returns>
        public static bool CalcBeachLine(Vector2d p, double lineY, out Parabola ret)
        {
            double dp = 2 * (p.y - lineY);
            if (dp <= Epsilon)
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