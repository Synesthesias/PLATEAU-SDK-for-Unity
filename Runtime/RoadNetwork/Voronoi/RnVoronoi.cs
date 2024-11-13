using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PLATEAU.RoadNetwork.Voronoi
{
    /// <summary>
    /// floatの精度が足りないのでdoubleで計算する
    /// </summary>
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
            return a.x.Equals(b.x) && a.y.Equals(b.y);
        }

        public static bool operator !=(Vector2d a, Vector2d b)
        {
            return !(a == b);
        }

        public bool Equals(Vector2d other)
        {
            return x.Equals(other.x) && y.Equals(other.y);
        }

        public override bool Equals(object obj)
        {
            return obj is Vector2d other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(x, y);
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
        public Vector2d V { get; }

        public double x => V.x;

        public double y => V.y;

        public VPoint(Vector2d v)
        {
            V = v;
        }

        public VPoint(double x, double y)
        {
            V = new Vector2d(x, y);
        }

        public static implicit operator Vector2d(VPoint p)
        {
            return p.V;
        }
    }

    public class VEdge
    {
        private VPoint start;

        /// <summary>
        /// 開始点
        /// </summary>
        public VPoint Start
        {
            get => start;
            set => start = value;
        }


        private VPoint end;

        /// <summary>
        /// 終了点(半直線の場合はnull)
        /// </summary>
        public VPoint End
        {
            get => end;
            set => end = value;
        }

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
        /// 辺の最後の終了地点調整用. 半直線の反対側の辺
        /// </summary>
        public VEdge OppositeEdge { get; set; }

        /// <summary>
        /// sを開始地点として, サイトポイントl,rからなるエッジ
        /// </summary>
        /// <param name="start"></param>
        /// <param name="direction"></param>
        /// <param name="leftSiteIndex"></param>
        /// <param name="rightSiteIndex"></param>
        public VEdge(VPoint start, Vector2d direction, int leftSiteIndex, int rightSiteIndex)
        {
            this.start = start;
            this.end = null;
            LeftSiteIndex = leftSiteIndex;
            RightSiteIndex = rightSiteIndex;
            Direction = direction.normalized;
        }

        /// <summary>
        /// Start/Endから成るdirectionともともとのdirectionが一致しているかをチェックする
        /// </summary>
        /// <exception cref="InvalidDataException"></exception>
        private void Check()
        {
            if (Start != null && End != null)
            {
                var d = End.V - Start.V;
                if (d.magnitude > 1e-8)
                {
                    var dir = d.normalized;
                    var dot = Vector2.Dot(dir.ToVector2(), Direction.ToVector2());
                    if (Mathf.Abs(dot) < 0.99f)
                    {
                        throw new InvalidDataException("direction");
                    }
                }

            }
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

            static bool IsTarget(VEdge e, double t, double ep)
            {
                return (t >= -ep /*&& (e.End == null || t <= 1 + epsilon)*/);// || Math.Abs(e.Direction.y) <= ep;
            }

            return IsTarget(a, t1, epsilon) && IsTarget(b, t2, epsilon);
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
        /// この木の高さ
        /// </summary>
        public int Height => GetHeight((T)this);

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

        /// <summary>
        /// このツリーの高さ(子の最大ネスト数)
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public static int GetHeight(T p)
        {
            if (p == null)
                return 0;
            if (p.IsLeaf)
                return 1;

            return Math.Max(GetHeight(p.Left), GetHeight(p.Right)) + 1;
        }
    }

    public static class VTreeNodeEx
    {
        /// <summary>
        /// ノードの葉をすべて取得する(左から右に取得する)
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<T> GetAllLeafs<T>(this T self) where T : VTreeNode<T>
        {
            if (self.IsLeaf)
            {
                yield return self;
            }
            else
            {
                if (self.Left != null)
                {
                    foreach (var l in self.Left.GetAllLeafs())
                        yield return l;
                }
                if (self.Right != null)
                {
                    foreach (var r in self.Right.GetAllLeafs())
                        yield return r;
                }

            }
        }

        /// <summary>
        /// rootを根として木構造を文字列で表示する. strDigitは各ノードを表す文字列の幅
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="root"></param>
        /// <param name="toString"></param>
        /// <returns></returns>
        public static string BuildString<T>(this T root, Func<T, string> toString) where T : VTreeNode<T>
        {
            // widthのサイズの三角形ASCIIアートを返す
            static List<string> Triangle(int width)
            {
                var h = (width - 1) / 2;
                var ret = new List<string>();
                for (var i = 0; i < h; ++i)
                {
                    ret.Add($"/{new string(' ', i * 2 + 1)}\\");
                }

                return ret;
            }

            // 中央ぞろえ
            static string Centering(string text, int length)
            {
                var pad = length - text.Length;
                var left = pad / 2;
                return text.PadLeft(left + text.Length).PadRight(length);
            }

            List<string> Impl(T n)
            {
                if (n.IsLeaf)
                    return new() { toString(n) };

                var leftLines = Impl(n.Left);
                var rightLines = Impl(n.Right);

                var leftLen = leftLines.Any() ? leftLines.Max(x => x.Length) : 0;
                var rightLen = rightLines.Any() ? rightLines.Max(x => x.Length) : 0;

                var childWidth = Math.Max(leftLen, rightLen);

                List<string> ret = new();
                for (var i = 0; i < Mathf.Max(leftLines.Count, rightLines.Count); ++i)
                {
                    var l = Centering(i < leftLines.Count ? leftLines[i] : " ", childWidth);
                    var r = Centering(i < rightLines.Count ? rightLines[i] : " ", childWidth);
                    ret.Add($"{l} {r}");
                }

                var width = 2 * childWidth + 1;
                var triangle = Triangle(childWidth + 2 - 2);
                ret.InsertRange(0, triangle.Select(t => Centering(t, width)));
                ret.Insert(0, Centering(toString(n), width));
                return ret;
            }

            var lines = Impl(root);
            var sb = new StringBuilder();
            foreach (var l in lines)
                sb.AppendLine(l);
            return sb.ToString();
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
            public T LeftSitePoint => LeftSitePoints.FirstOrDefault();

            /// <summary>
            /// 右側のサイトポイント
            /// </summary>
            public T RightSitePoint => RightSitePoints.FirstOrDefault();

            /// <summary>
            /// 辺の左側にあるサイトポイント情報
            /// </summary>
            public List<T> LeftSitePoints => Parent.SitePointGroups[LeftSiteIndex];

            /// <summary>
            /// 辺の右側にあるサイトポイント情報
            /// </summary>
            public List<T> RightSitePoints => Parent.SitePointGroups[RightSiteIndex];

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

            private VoronoiData<T> Parent { get; }

            public Edge(VoronoiData<T> parent, VEdge edge)
            {
                Parent = parent;
                Direction = edge.Direction.normalized;
                LeftSiteIndex = edge.LeftSiteIndex;
                RightSiteIndex = edge.RightSiteIndex;
                Start = edge.Start?.V;
                End = edge.End?.V;
            }
        }

        /// <summary>
        /// サイトポイント
        /// (同じ頂点を持つサイトポイントが複数あったときのためにリストで持っておく)
        /// </summary>
        public List<List<T>> SitePointGroups { get; set; }

        /// <summary>
        /// 構成エッジリスト
        /// </summary>
        public List<Edge> Edges { get; }

        /// <summary>
        /// 構成頂点リスト
        /// </summary>
        public List<Vector2d> Points { get; }

        public VoronoiData(List<List<T>> sitePointGroups, List<VPoint> points, List<VEdge> edges)
        {
            SitePointGroups = sitePointGroups;
            Points = points.Select(v => v.V).ToList();
            Edges = edges.Select(e => new Edge(this, e)).ToList();
        }

    }

    public static class RnVoronoiEx
    {
        private const double Epsilon = 1e-8;

        private static bool IsZero(double x) => Math.Abs(x) <= Epsilon;

        /// <summary>
        /// ビーチライン. (木構造の構成要素)
        /// </summary>
        public class BeachLine : VTreeNode<BeachLine>
        {
            private Work Work { get; }

            /// <summary>
            /// この放物線を構成するサイトポイント
            /// </summary>
            public Vector2d Site => Work.SitePoints[SiteIndex];

            private int siteIndex = -1;

            /// <summary>
            /// この放物線を構成するサイトポイントのインデックス
            /// </summary>
            public int SiteIndex
            {
                get
                {
                    return IsLeaf ? siteIndex : -1;
                }
                set => siteIndex = value;
            }

            /// <summary>
            /// 自身がノードの場合. 左右の子供の葉で構成される垂直二等分線
            /// </summary>
            public VEdge Edge { get; set; }

            /// <summary>
            /// この弧に含まれる円イベント
            /// </summary>
            public Event CircleEvent { get; set; }

            /// <summary>
            /// 葉作成
            /// </summary>
            /// <param name="work"></param>
            /// <param name="siteIndex"></param>
            public BeachLine(Work work, int siteIndex)
            {
                Work = work;
                SiteIndex = siteIndex;
            }

            /// <summary>
            /// ノード作成
            /// </summary>
            /// <param name="work"></param>
            /// <param name="edge"></param>
            /// <param name="left"></param>
            /// <param name="right"></param>
            public BeachLine(Work work, VEdge edge, BeachLine left, BeachLine right)
            {
                Work = work;
                Edge = edge;
                SetLeft(left);
                SetRight(right);
            }

            /// <summary>
            /// 自分自身の木構造を文字列で表示する
            /// </summary>
            /// <param name="root"></param>
            /// <returns></returns>
            public string ToTreeString(BeachLine root)
            {
                var height = GetHeight(root);

                // 対象になるようにdigitは奇数にする
                // ただし-1があるので最低3になる
                var digit = (int)Math.Floor(Math.Log10(Math.Pow(2, height)));
                if (digit % 2 == 0)
                    digit++;
                digit = Mathf.Max(3, digit);

                static string Centering(string text, int length)
                {
                    var pad = length - text.Length;
                    var left = pad / 2;
                    return text.PadLeft(left + text.Length).PadRight(length);
                }
                return root.BuildString(x => $"{x.SiteIndex}");
            }
        }

        public enum EventType
        {
            SiteEvent,
            CircleEvent
        }

        public struct EventKey
        {
            public EventType Type { get; set; }

            public Vector2d Point { get; set; }
        }

        public class Event
        {
            /// <summary>
            /// 走査線位置
            /// </summary>
            public VPoint Point { get; set; }

            /// <summary>
            /// イベントタイプ
            /// </summary>
            public EventType Type { get; }

            /// <summary>
            /// Siteイベント用
            /// </summary>
            public int SiteIndex { get; }

            /// <summary>
            /// CircleEvent用
            /// </summary>
            public BeachLine Arch { get; set; }

            /// <summary>
            /// CircleEvent用. 交点
            /// </summary>
            public Vector2d CrossPoint { get; set; }

            private Event(VPoint v, EventType eventType, int siteIndex)
            {
                Point = v;
                Type = eventType;
                Arch = null;
                SiteIndex = siteIndex;
            }

            public static Event SiteEvent(VPoint v, int siteIndex)
            {
                return new Event(v, EventType.SiteEvent, siteIndex);
            }

            public static Event CircleEvent(VPoint v, BeachLine arch, VPoint crossPoint)
            {
                return new Event(v, EventType.CircleEvent, -1) { Arch = arch, CrossPoint = crossPoint };
            }
        }


        private class QueueComparer : IComparer<EventKey>
        {
            public int Compare(EventKey a, EventKey b)
            {
                // yは大きい順
                var dy = Math.Sign(b.Point.y - a.Point.y);
                if (dy != 0)
                    return dy;
                // xは小さい順
                var dx = Math.Sign(a.Point.x - b.Point.x);
                if (dx != 0)
                    return dx;
                // CircleEventが先
                return (int)a.Type - (int)b.Type;
            }
        }

        public class Work
        {
            /// <summary>
            /// イベントキュー
            /// </summary>
            private SortedList<EventKey, Queue<Event>> EventQueue { get; } = new(new QueueComparer());

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
            public BeachLine Root { get; set; }

            /// <summary>
            /// 削除予定のイベント
            /// </summary>
            public HashSet<Event> DeleteEvents { get; } = new HashSet<Event>();


            public double LineY { get; set; }

            /// <summary>
            /// キューにイベントがある
            /// </summary>
            public bool AnyEvent => EventQueue.Any();


            private readonly List<VPoint> sitePoints;
            /// <summary>
            /// サイトポイント
            /// </summary>
            public IReadOnlyList<VPoint> SitePoints => sitePoints;

            public Work(List<Vector2d> vertices)
            {
                sitePoints = vertices.Select(v => new VPoint(v)).ToList();
            }

            public void Enqueue(Event e)
            {
                var key = new EventKey { Point = e.Point.V, Type = e.Type };
                if (EventQueue.ContainsKey(key) == false)
                    EventQueue.Add(key, new Queue<Event>());
                EventQueue[key].Enqueue(e);
            }

            public Event Dequeue()
            {
                var q = EventQueue.First();
                var ret = q.Value.Dequeue();
                if (q.Value.Count == 0)
                    EventQueue.RemoveAt(0);
                return ret;
            }

            public IEnumerable<Event> Events => EventQueue.SelectMany(x => x.Value);


            public static bool TryGetY(double ly, Vector2d p, double x, out double y)
            {
                if (CalcBeachLine(p, ly, out var parabola))
                {
                    y = parabola.GetY(x);
                    return true;
                }

                y = 0.0;
                return false;
            }


            private VEdge Edge(VPoint start, int leftSiteIndex, int rightSiteIndex)
            {
                // l, rの垂直二等分線
                var l = SitePoints[leftSiteIndex];
                var r = SitePoints[rightSiteIndex];
                var direction = (new Vector2d(r.y - l.y, -(r.x - l.x))).normalized;
                return new VEdge(start, direction, leftSiteIndex, rightSiteIndex);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="start"></param>
            /// <param name="leftSiteIndex"></param>
            /// <param name="rightSiteIndex"></param>
            /// <param name="leftEdge"></param>
            /// <param name="rightEdge"></param>
            private void Edge(VPoint start, int leftSiteIndex, int rightSiteIndex, out VEdge leftEdge, out VEdge rightEdge)
            {
                // l, rの垂直二等分線
                var l = SitePoints[leftSiteIndex];
                var r = SitePoints[rightSiteIndex];
                var direction = (new Vector2d(r.y - l.y, -(r.x - l.x))).normalized;
                leftEdge = new VEdge(start, direction, leftSiteIndex, rightSiteIndex);
                rightEdge = new VEdge(start, -direction, rightSiteIndex, leftSiteIndex);

                leftEdge.OppositeEdge = rightEdge;
            }


            private double GetXOfEdge(BeachLine par, double y)
            {
                var left = BeachLine.GetLeftChild(par);
                var right = BeachLine.GetRightChild(par);

                var l = left.Site;
                var r = right.Site;

                // yがp.V.yと同じ場合放物線にならない(y軸平行な直線になる)
                // その場合の交点はp.V.xのまま
                if (CalcBeachLine(l, y, out var leftParabola) == false)
                {
                    return l.x;
                }

                if (CalcBeachLine(r, y, out var rightParabola) == false)
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

            /// <summary>
            /// ビーチラインの中からxに対応する放物線を取得する
            /// </summary>
            /// <param name="x"></param>
            /// <param name="centerLeft"></param>
            /// <param name="centerRight"></param>
            /// <returns></returns>
            public void GetParabolaByX(double x, out BeachLine centerLeft, out BeachLine centerRight)
            {
                centerLeft = centerRight = null;
                BeachLine par = Root;
                while (!par.IsLeaf)
                {
                    var tmpX = GetXOfEdge(par, LineY);
                    if (Math.Abs(tmpX - x) <= Epsilon)
                    {
                        centerLeft = par.ChildLeftLeaf;
                        centerRight = par.ChildRightLeaf;
                        return;
                    }
                    par = tmpX >= x ? par.Left : par.Right;
                }

                centerLeft = centerRight = par;
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
                    Root = new BeachLine(this, siteIndex);
                    return;
                }
                GetParabolaByX(newSite.x, out var centerLeft, out var centerRight);
                var center = centerLeft;
                RemoveCircleEvent(center);

                // centerとnewSiteが同じy座標の場合. 放物線は1点でしか交わらないのでツリーの変更もそれに合わせる
                var isSameY = (center.Site.y - newSite.y) <= Epsilon;
                if (isSameY)
                {
                    var (leftIndex, rightIndex) = (center.SiteIndex, siteIndex);
                    if (center.Site.x > newSite.x)
                        (leftIndex, rightIndex) = (rightIndex, leftIndex);

                    VPoint start = new VPoint((newSite + center.Site) * 0.5);
                    Points.Add(start);
                    Edge(start, leftIndex, rightIndex, out var el, out var er);
                    Edges.Add(el);
                    center.Edge = el;

                    var arcLeft = new BeachLine(this, leftIndex);
                    var arcRight = new BeachLine(this, rightIndex);
                    center.SetRight(arcRight);
                    center.SetLeft(arcLeft);

                    CheckCircles(arcLeft, arcRight);
                }
                else
                {
                    var (leftIndex, rightIndex) = (center.SiteIndex, siteIndex);
                    // center.site.y > newSite.yなのでTryGetYは成功する
                    TryGetY(LineY, center.Site, newSite.x, out var newSiteY);

                    VPoint start = new VPoint(newSite.x, newSiteY);
                    Points.Add(start);

                    // startを起点に二つの半直線を作成
                    Edge(start, leftIndex, rightIndex, out var el, out var er);
                    Edges.Add(el);
                    center.Edge = er;

                    var arcLeft = new BeachLine(this, center.SiteIndex);
                    var arcMid = new BeachLine(this, siteIndex);
                    var arcRight = new BeachLine(this, center.SiteIndex);

                    center.SetRight(arcRight);
                    center.SetLeft(new BeachLine(this, el, arcLeft, arcMid));

                    CheckCircles(arcLeft, arcRight);
                }
            }

            private void CheckCircles(params BeachLine[] parabolas)
            {
                foreach (var c in parabolas)
                    CheckCircle(c);
            }

            /// <summary>
            /// 円イベントチェック
            /// </summary>
            /// <param name="b"></param>
            private void CheckCircle(BeachLine b)
            {
                var lp = BeachLine.GetLeftParent(b);
                var rp = BeachLine.GetRightParent(b);

                var leftChild = BeachLine.GetLeftChild(lp);
                var rightChild = BeachLine.GetRightChild(rp);

                // 
                if (leftChild == null || rightChild == null || leftChild.SiteIndex == rightChild.SiteIndex)
                    return;

                var s = GetEdgeIntersection(lp.Edge, rp.Edge);
                if (s == null)
                    return;

                // 走査線よりも上にある場合は無視
                var d = Math.Min((leftChild.Site - s.V).magnitude, (rightChild.Site - s.V).magnitude);
                if (s.y - d > (LineY + Epsilon))
                    return;

                var point = new VPoint(s.x, s.y - d);
                Points.Add(point);

                AddCircleEvent(point, b, s);
            }

            /// <summary>
            /// 辺同士の交点を取得する
            /// </summary>
            /// <param name="a"></param>
            /// <param name="b"></param>
            /// <returns></returns>
            private VPoint GetEdgeIntersection(VEdge a, VEdge b)
            {
                if (VEdge.TryGetCrossPoint(a, b, out var v, Epsilon) == false)
                    return null;

                VPoint p = new VPoint(v.x, v.y);
                Points.Add(p);
                return p;
            }

            /// <summary>
            /// 円イベントを削除する
            /// </summary>
            /// <param name="b"></param>
            private void RemoveCircleEvent(BeachLine b)
            {
                if (b.CircleEvent == null)
                    return;
                DeleteEvents.Add(b.CircleEvent);
                b.CircleEvent = null;
            }

            /// <summary>
            /// 円イベントを追加する
            /// </summary>
            /// <param name="p"></param>
            /// <param name="b"></param>
            /// <param name="crossPoint"></param>
            private void AddCircleEvent(VPoint p, BeachLine b, VPoint crossPoint)
            {
                Event e = Event.CircleEvent(p, b, crossPoint);
                b.CircleEvent = e;
                Enqueue(e);
            }

            /// <summary>
            /// e.archの弧を削除する
            /// </summary>
            /// <param name="e"></param>
            public void RemoveParabola(Event e)
            {
                // p1が消える
                BeachLine p1 = e.Arch;

                var xl = BeachLine.GetLeftParent(p1);
                var xr = BeachLine.GetRightParent(p1);
                var p0 = BeachLine.GetLeftChild(xl);
                var p2 = BeachLine.GetRightChild(xr);

                if (TryGetY(LineY, p1.Site, e.Point.x, out var pointY) == false)
                    return;

                //VPoint p = new VPoint(e.point.x, pointY);
                VPoint p = new VPoint(e.CrossPoint);
                RemoveCircleEvent(p2);
                RemoveCircleEvent(p0);

                Points.Add(p);

                xl.Edge.End = p;
                xr.Edge.End = p;


                // 自分を削除する
                p1.RemoveLeaf();

                // p0/p2の共通の親を探す
                BeachLine higher = null;
                BeachLine par = p1.Parent;
                while (par != null)
                {
                    if (par.ChildLeftLeaf == p0 && par.ChildRightLeaf == p2)
                        higher = par;
                    par = par.Parent;
                }


                higher.Edge = Edge(p, p0.SiteIndex, p2.SiteIndex);
                Edges.Add(higher.Edge);
                p1.Parent = null;

                CheckCircles(p0, p2);
            }


            /// <summary>
            /// 二つに分けていた半直線を結合する
            /// </summary>
            public void CheckEdgeEnd()
            {
                foreach (var e in Edges)
                {
                    if (e.OppositeEdge == null)
                        continue;

                    e.Start = e.OppositeEdge.End;
                    e.OppositeEdge = null;
                }
            }
        }

        public static VoronoiData<T> CalcVoronoiData<T>(List<T> points, Func<T, Vector2d> toVec2)
        {
            var grouped = points.GroupBy(toVec2).ToList();
            var sitePoints = grouped.Select(g => g.Key).ToList();
            var work = new Work(sitePoints);

            for (var i = 0; i < work.SitePoints.Count; ++i)
            {
                //p.V -= Min;
                var ev = Event.SiteEvent(work.SitePoints[i], i);
                work.Enqueue(ev);
            }

            while (work.AnyEvent)
            {
                var e = work.Dequeue();
                work.LineY = e.Point.y;
                if (work.DeleteEvents.Contains(e))
                {
                    work.DeleteEvents.Remove(e);
                    continue;
                }
                switch (e.Type)
                {
                    case EventType.SiteEvent:
                        work.InsertParabola(e.SiteIndex);
                        break;
                    case EventType.CircleEvent:
                        work.RemoveParabola(e);
                        break;
                }
            }

            work.CheckEdgeEnd();
            return new VoronoiData<T>(grouped.Select(g => g.ToList()).ToList(), work.Points, work.Edges);
        }

        /// <summary>
        /// y = a*x^2 + b*x + c
        /// </summary>
        public struct VParabola
        {
            public double a;
            public double b;
            public double c;

            public VParabola(double a, double b, double c)
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
            public int GetCrossPoint(VParabola other, out Vector2d p1, out Vector2d p2)
            {
                p1 = p2 = new Vector2d();
                var p = new VParabola(a - other.a, b - other.b, c - other.c);
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
        public static bool CalcBeachLine(Vector2d p, double lineY, out VParabola ret)
        {
            double dp = 2 * (p.y - lineY);
            if (dp <= Epsilon)
            {
                ret = new VParabola();
                return false;
            }
            dp = 1 / dp;
            var a = dp;
            var b = -2 * p.x * dp;
            var c = (p.x * p.x - lineY * lineY + p.y * p.y) * dp;
            ret = new VParabola(a, b, c);
            return true;
        }
    }

}