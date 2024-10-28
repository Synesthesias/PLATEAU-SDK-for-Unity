using PLATEAU.CityInfo;
using PLATEAU.RoadNetwork.Util;
using PLATEAU.Util;
using PLATEAU.Util.GeoGraph;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Splines;

namespace PLATEAU.RoadNetwork.Structure
{
    [Serializable]
    public class RnTrack : ARnParts<RnTrack>
    {
        //----------------------------------
        // start: フィールド
        //----------------------------------

        /// <summary>
        /// 流入元
        /// </summary>
        public RnWay FromBorder { get; set; }

        /// <summary>
        /// 流出先
        /// </summary>
        public RnWay ToBorder { get; set; }

        /// <summary>
        /// 経路
        /// </summary>
        public Spline Spline { get; set; }

        /// <summary>
        /// 曲がり具合
        /// </summary>
        public RnTurnType TurnType { get; set; }

        //----------------------------------
        // end: フィールド
        //----------------------------------
        // シリアライズ化の為にデフォルトコンストラクタは必要
        public RnTrack() { }

        public RnTrack(RnWay from, RnWay to, Spline spline, RnTurnType turnType = RnTurnType.Straight)
        {
            FromBorder = from;
            ToBorder = to;
            Spline = spline;
            TurnType = turnType;
        }

        /// <summary>
        /// 入口/出口が同じかどうか
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public bool IsSameInOut(RnWay from, RnWay to)
        {
            return FromBorder.IsSameLine(from) && ToBorder.IsSameLine(to);
        }

        /// <summary>
        /// 入口/出口が同じかどうか
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool IsSameInOut(RnTrack other)
        {
            return IsSameInOut(other.FromBorder, other.ToBorder);
        }
    }

    // 交差点における曲がり具合
    public enum RnTurnType
    {
        // 左後ろ
        LeftBack,
        // 左折
        LeftTurn,
        // 左前
        LeftFront,
        // 直進
        Straight,
        // 右前
        RightFront,
        // 右折
        RightTurn,
        // 右後ろ
        RightBack,
        // Uターン
        UTurn,
    }

    public static class RnTurnTypeEx
    {
        // enumの数(キャッシュ用)
        public static readonly int Count = Enum.GetValues(typeof(RnTurnType)).Length;

        // 左折関係か
        public static bool IsLeft(this RnTurnType self)
        {
            return self == RnTurnType.LeftBack || self == RnTurnType.LeftTurn || self == RnTurnType.LeftFront;
        }

        // 右折関係か
        public static bool IsRight(this RnTurnType self)
        {
            return self == RnTurnType.RightBack || self == RnTurnType.RightTurn || self == RnTurnType.RightFront;
        }

        // 流入方向と流出方向から曲がり具合を取得
        public static RnTurnType GetTurnType(Vector3 from, Vector3 to, AxisPlane axis)
        {
            return GetTurnType(from.ToVector2(axis), to.ToVector2(axis));
        }

        // 流入方向と流出方向から曲がり具合を取得
        public static RnTurnType GetTurnType(Vector2 from, Vector2 to)
        {
            var ang = -Vector2.SignedAngle(from, to) + 180f;
            // #NOTE : AVENUEのアルゴリズム
            if (ang is > 10f and < 67.5f)
                return RnTurnType.LeftBack;
            if (ang is >= 67.5f and < 112.5f)
                return RnTurnType.LeftTurn;
            if (ang is >= 112.5f and < 157.5f)
                return RnTurnType.LeftFront;
            if (ang is >= 157.5f and < 202.5f)
                return RnTurnType.Straight;
            if (ang is >= 202.5f and < 247.5f)
                return RnTurnType.RightFront;
            if (ang is >= 247.5f and < 292.5f)
                return RnTurnType.RightTurn;
            if (ang is >= 292.5f and < 337.5f)
                return RnTurnType.RightBack;
            return RnTurnType.UTurn;
        }
    }

    // 交差点への進行タイプ
    [Flags]
    public enum RnFlowTypeMask
    {
        Empty = 0,
        // 流入
        Inbound = 1 << 0,
        // 流出
        Outbound = 1 << 1,
        // 両方
        Both = Inbound | Outbound,
    }

    [Serializable]
    public class RnNeighbor : ARnParts<RnNeighbor>
    {
        //----------------------------------
        // start: フィールド
        //----------------------------------

        // Roadとの境界線
        public RnWay Border { get; set; }

        // 隣接道路(交差点)基本的にRoadだが、初期のPLATEAUモデルによってはIntersectionもあり得るため基底クラスで持っている
        // 隣接していない場合はnull
        public RnRoadBase Road { get; set; }

        //----------------------------------
        // end: フィールド
        //----------------------------------

        // この境界が道路と接続しているか
        public bool IsBorder => Road != null;

        // 有効値判定(Border != null)
        public bool IsValid => Border != null;

        // この境界線に対して流入/流出するタイプを取得
        public RnFlowTypeMask GetFlowType()
        {
            if (Border == null || Road == null)
                return RnFlowTypeMask.Empty;
            if (Border.IsValid == false)
                return RnFlowTypeMask.Empty;

            if (Road is RnRoad road)
            {
                // #NOTE : 車線一つしかない場合は、出ていくレーンも対象とする
                if (road.MainLanes.Count == 1)
                    return RnFlowTypeMask.Both;

                var ret = RnFlowTypeMask.Empty;
                if (road.GetConnectedLanes(Border).Any(l => l.IsMedianLane == false && l.NextBorder.IsSameLine(Border)))
                    ret |= RnFlowTypeMask.Inbound;
                if (road.GetConnectedLanes(Border).Any(l => l.IsMedianLane == false && l.PrevBorder.IsSameLine(Border)))
                    ret |= RnFlowTypeMask.Outbound;
                return ret;
            }
            // 交差点同士の接続の場合はレーン全部対象
            else if (Road is RnIntersection)
            {
                return RnFlowTypeMask.Both;
            }

            return RnFlowTypeMask.Empty;
        }

        public Vector3 CalcCenter()
        {
            return Border.GetLerpPoint(0.5f);   // 計算負荷削減のため way[0], way[way.Count-1]の平均でもいいかも
        }

        /// <summary>
        /// この境界とつながっているレーン
        /// </summary>
        /// <returns></returns>
        public RnLane GetConnectedLane()
        {
            return GetConnectedLanes().FirstOrDefault();
        }

        /// <summary>
        /// この境界とつながっているレーンリスト. 基本的に0 or 1
        /// </summary>
        /// <returns></returns>
        public IEnumerable<RnLane> GetConnectedLanes()
        {
            if (Border == null)
                yield break;
            if (Road == null)
                yield break;

            if (Road is RnRoad road)
            {
                foreach (var lane in road.MainLanes)
                {
                    // Borderと同じ線上にあるレーンを返す
                    if (lane.AllBorders.Any(b => b.IsSameLine(Border)))
                        yield return lane;
                }
            }
        }
    }

    /// <summary>
    /// 交差点
    /// </summary>
    [Serializable]
    public class RnIntersection : RnRoadBase
    {
        //----------------------------------
        // start: フィールド
        //----------------------------------
        // 自分が所属するRoadNetworkModel
        public RnModel ParentModel { get; set; }

        // 対象のtranオブジェクト
        public PLATEAUCityObjectGroup TargetTran { get; set; }

        // 交差点の外形情報. 時計回り/反時計回りかは保証されていないが, 連結はしている
        private List<RnNeighbor> edges = new List<RnNeighbor>();

        // 信号制御器
        public TrafficSignalLightController SignalController { get; set; } = null;

        // 交差点内のトラック
        private List<RnTrack> tracks = new();

        //　道路と道路の間に入れる空交差点かどうかの判定
        public bool IsEmptyIntersection { get; private set; }


        //----------------------------------
        // end: フィールド
        //----------------------------------

        public override PLATEAUCityObjectGroup CityObjectGroup => TargetTran;

        /// <summary>
        /// 他の道路との境界線Edge取得
        /// </summary>
        public IEnumerable<RnNeighbor> Neighbors => edges.Where(e => e.IsBorder);

        /// <summary>
        /// 輪郭のEdge取得
        /// </summary>
        public IReadOnlyList<RnNeighbor> Edges => edges;

        // 交差点内のトラック
        public IReadOnlyList<RnTrack> Tracks => tracks;

        public RnIntersection() { }

        public RnIntersection(PLATEAUCityObjectGroup targetTran)
        {
            TargetTran = targetTran;
        }

        /// <summary>
        /// 隣接するRnRoadBaseを取得重複チェックはしていないので注意
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<RnRoadBase> GetNeighborRoads()
        {
            foreach (var neighbor in Neighbors)
            {
                if (neighbor.Road != null)
                    yield return neighbor.Road;
            }
        }

        /// <summary>
        /// 隣接道路との境界線を取得
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<RnBorder> GetBorders()
        {
            foreach (var neighbor in Neighbors.Where(n => n.Road != null))
            {
                yield return new RnBorder(neighbor.Border);
            }
        }

        /// <summary>
        /// 隣接情報追加. borderがnullの場合は追加しない
        /// </summary>
        /// <param name="road"></param>
        /// <param name="border"></param>
        public void AddEdge(RnRoadBase road, RnWay border)
        {
            if (border == null)
                return;
            edges.Add(new RnNeighbor { Road = road, Border = border });
        }

        /// <summary>
        /// edgesを差し替え
        /// afterEdgesは必ず連結している者とする
        /// </summary>
        /// <param name="afterEdges"></param>
        public void ReplaceEdges(List<RnNeighbor> afterEdges)
        {
            this.edges = afterEdges;
        }

        public Vector3 GetCenterPoint()
        {
            var ret = Neighbors.SelectMany(n => n.Border.Vertices).Aggregate(Vector3.zero, (a, b) => a + b);
            var cnt = Neighbors.Sum(n => n.Border.Count);
            return ret / cnt;
        }

        public void ReplaceBorder(RnRoad link, List<RnWay> borders)
        {
            RemoveNeighbors(n => n.Road == link);
            edges.AddRange(borders.Select(b => new RnNeighbor { Road = link, Border = b }));
        }

        public void RemoveNeighbors(Func<RnNeighbor, bool> predicate)
        {
            for (var i = 0; i < edges.Count; i++)
            {
                var n = edges[i];
                if (predicate(n))
                {
                    edges.RemoveAt(i);
                    i--;
                    // トラックからも削除する
                    tracks.RemoveAll(x => x.FromBorder == n.Border || x.ToBorder == n.Border);
                }
            }
        }

        /// <summary>
        /// トラック情報を追加/更新する.
        /// 同じfrom/toのトラックがすでにある場合は上書きする. そうでない場合は追加する
        /// </summary>
        /// <param name="track"></param>
        public bool TryAddOrUpdateTrack(RnTrack track)
        {
            // trackの入口/出口がこの交差点のものかチェックする
            if (!edges.Any(e =>
                    e.Border.IsSameLine(track.FromBorder) || !edges.Any(e => e.Border.IsSameLine(track.ToBorder))))
            {
                DebugEx.LogError("交差点に含まれないトラックが追加されようとしています");
                return false;
            }

            // それに同じ物が入口/出口のものがあれば削除する
            tracks.RemoveAll(t => t.IsSameInOut(track));

            // track追加
            tracks.Add(track);
            return true;
        }

        /// <summary>
        /// 指定した条件のトラックを削除する
        /// </summary>
        /// <param name="predicate"></param>
        public void RemoveTracks(Predicate<RnTrack> predicate)
        {
            tracks.RemoveAll(predicate);
        }

        /// <summary>
        /// road/laneに接続している隣接情報を削除する
        /// </summary>
        /// <param name="road"></param>
        /// <param name="lane"></param>
        public void RemoveNeighbor(RnRoad road, RnLane lane)
        {
            RemoveNeighbors(x => x.Road == road && ((lane.PrevBorder?.IsSameLine(x.Border) ?? false) || (lane.NextBorder?.IsSameLine(x.Border) ?? false)));
        }

        /// <summary>
        /// 隣接情報からotherを削除する. other側の接続は消えない
        /// </summary>
        /// <param name="other"></param>
        public override void UnLink(RnRoadBase other)
        {
            // 削除するBorderに接続しているレーンも削除
            var borders = edges.Where(n => n.Road == other).Select(n => n.Border).ToList();
            RemoveNeighbors(n => n.Road == other);
        }

        /// <summary>
        /// 自身を切断する
        /// </summary>
        public override void DisConnect(bool removeFromModel)
        {
            // リンクを削除する
            foreach (var e in Edges)
            {
                e.Road?.UnLink(this);
                e.Road = null;
            }

            if (removeFromModel)
                ParentModel?.RemoveIntersection(this);
        }


        /// <summary>
        ///  隣接情報を差し替える(呼び出し注意)
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        public override void ReplaceNeighbor(RnRoadBase from, RnRoadBase to)
        {
            if (from == null)
                return;
            foreach (var e in Edges)
            {
                if (e.Road == from)
                    e.Road = to;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="tangentLength"></param>
        /// <param name="splitLength"></param>
        /// <param name="allowUTurn">Uターンを許可する</param>
        public void BuildTracks(float tangentLength = 10f, float splitLength = 2f, bool allowUTurn = false)
        {
            tracks.Clear();

            var edgeGroups = this.CreateEdgeGroup();

            foreach (var eg in edgeGroups)
            {
                if (eg.IsBorder == false)
                    continue;

                var inBounds = eg.InBoundEdges.ToList();
                foreach (var other in edgeGroups)
                {
                    if (other.IsBorder == false || eg == other)
                        continue;

                    // Uターンを許可しない場合
                    if (eg.Key == other.Key && allowUTurn == false)
                        continue;

                    var turnType = RnTurnTypeEx.GetTurnType(-eg.Normal, other.Normal, AxisPlane.Xz);

                    void AddTrack(RnNeighbor from, RnNeighbor to, RnTurnType edgeTurnType)
                    {
                        var fromNormal = from.Border.GetEdgeNormal((from.Border.Count - 1) / 2).normalized;
                        var toNormal = -to.Border.GetEdgeNormal((to.Border.Count - 1) / 2).normalized;

                        from.Border.GetLerpPoint(0.5f, out var fromPos);
                        to.Border.GetLerpPoint(0.5f, out var toPos);

                        var spline = new Spline
                        {
                            new(fromPos, tangentLength * fromNormal, -tangentLength *fromNormal),
                            new(toPos, tangentLength *toNormal, -tangentLength *toNormal)
                        }; ;
                        tracks.Add(new RnTrack(from.Border, to.Border, spline, edgeTurnType));
                    }

                    var outBounds = other.OutBoundEdges.ToList();
                    if (turnType.IsLeft())
                    {
                        // 左折の場合は左側のレーンのみ作成する
                        var num = Mathf.Min(inBounds.Count, outBounds.Count);
                        foreach (var from in Enumerable.Range(0, num).Select(i => inBounds[^(i + 1)]))
                        {
                            foreach (var to in outBounds)
                                AddTrack(from, to, turnType);
                        }
                    }
                    else if (turnType.IsRight())
                    {
                        // 右折の場合は右側のレーンのみ作成する
                        var num = Mathf.Min(inBounds.Count, outBounds.Count);
                        foreach (var from in inBounds.Take(num))
                        {
                            foreach (var to in outBounds)
                                AddTrack(from, to, turnType);
                        }
                    }
                    else
                    {
                        foreach (var from in inBounds)
                        {
                            foreach (var to in outBounds)
                            {
                                AddTrack(from, to, turnType);
                            }
                        }
                    }

                }
            }
        }

        /// <summary>
        /// edgesの順番を整列する. 各Edgeが連結かつ時計回りになるように整列する
        /// </summary>
        public void Align()
        {
            for (var i = 0; i < edges.Count; ++i)
            {
                var e0 = edges[i].Border;
                for (var j = i + 1; j < edges.Count; ++j)
                {
                    var e1 = edges[j].Border;
                    if (RnPoint.Equals(e0.GetPoint(-1), e1.GetPoint(0)))
                    {
                        (edges[i + 1], edges[j]) = (edges[j], edges[i + 1]);
                        break;
                    }
                    if (RnPoint.Equals(e0.GetPoint(-1), e1.GetPoint(-1)))
                    {
                        e1.Reverse(false);
                        (edges[i + 1], edges[j]) = (edges[j], edges[i + 1]);
                        break;
                    }
                }
            }

            // 時計回りになるように整列
            if (GeoGraph2D.IsClockwise(edges.Select(e => e.Border[0].ToVector2(AxisPlane.Xz))) == false)
            {
                foreach (var e in edges)
                    e.Border.Reverse(true);
                edges.Reverse();
            }
        }

        /// <summary>
        /// selfの全頂点の重心を返す
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public override Vector3 GetCenter()
        {
            var a = Neighbors
                .Where(n => n.Border != null)
                .SelectMany(n => n.Border.Vertices)
                .Aggregate(new { sum = Vector3.zero, i = 0 }, (a, p) => new { sum = a.sum + p, i = a.i + 1 });
            if (a.i == 0)
                return Vector3.zero;
            return a.sum / a.i;
        }
#if false
        /// <summary>
        /// a,bを繋ぐ経路を計算する
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="tangentLength"></param>
        public RnLane CalcTrackWay(RnRoadBase from, RnRoadBase to, float tangentLength = 10f)
        {
            if (from == to)
                return null;
            var nFrom = Neighbors.FirstOrDefault(n => n.Road == from);
            var nTo = Neighbors.FirstOrDefault(n => n.Road == to);
            if (nFrom == null || nTo == null)
                return null;
            if (nFrom.Border.Count < 2 || nTo.Border.Count < 2)
                return null;

            var fromLane = nFrom.GetConnectedLane();
            if (fromLane == null)
                return null;

            var toLane = nTo.GetConnectedLane();
            if (toLane == null)
                return null;


            fromLane.TryGetBorderNormal(nFrom.Border, out var aLeftPos, out var aLeftNormal, out var aRightPos, out var aRightNormal);
            toLane.TryGetBorderNormal(nTo.Border, out var bLeftPos, out var bLeftNormal, out var bRightPos, out var bRightNormal);

            var rightSp = new Spline
            {
                new(aLeftPos, tangentLength * aLeftNormal, tangentLength *aLeftNormal),
                new(bRightPos, tangentLength *bRightNormal, tangentLength *bRightNormal)
            };

            var leftSp = new Spline
            {
                new(aRightPos, tangentLength *aRightNormal, tangentLength *aRightNormal),
                new(bLeftPos, tangentLength *bLeftNormal, tangentLength *bLeftNormal)
            };

            var rates = Enumerable.Range(0, 10).Select(i => 1f * i / (9)).ToList();
            var leftWay = new RnWay(RnLineString.Create(rates.Select(t =>
            {
                leftSp.Evaluate(t, out var pos, out var tang, out var up);
                return (Vector3)pos;
            })));
            var rightWay = new RnWay(RnLineString.Create(rates.Select(t =>
            {
                rightSp.Evaluate(t, out var pos, out var tang, out var up);
                return (Vector3)pos;
            })));
            return new RnLane(leftWay, rightWay, nFrom.Border, nTo.Border);
        }
#endif
        // ---------------
        // Static Methods
        // ---------------
        public static RnIntersection CreateEmptyIntersection(List<RnWay> borderLeft2Right, RnRoad prev, RnRoad next)
        {
            var ret = new RnIntersection
            {
                IsEmptyIntersection = true,
                edges = new List<RnNeighbor>(borderLeft2Right.Count * 2)
            };

            foreach (var border in borderLeft2Right)
            {
                var neighbor = new RnNeighbor { Road = prev, Border = border };
                ret.edges.Add(neighbor);

                var pos = border.GetLerpPoint(0.5f);
                var dir = border.GetEdgeNormal(0).normalized;

                var (from, to) = (new BezierKnot(pos, dir, -dir), new BezierKnot(pos, -dir, dir));

                var flow = neighbor.GetFlowType();
                if ((flow & RnFlowTypeMask.Inbound) != 0)
                {
                    ret.tracks.Add(new RnTrack(border, border, new Spline { from, to }));
                }
                else if ((flow & RnFlowTypeMask.Outbound) != 0)
                {
                    (to, from) = (from, to);
                    ret.tracks.Add(new RnTrack(border, border, new Spline { from, to }));
                }
            }

            foreach (var border in borderLeft2Right.Reversed())
            {
                ret.edges.Add(new RnNeighbor { Road = next, Border = border.ReversedWay() });
            }
            return ret;
        }
    }

    public static class RnIntersectionEx
    {
        public class EdgeGroup
        {
            public RnRoadBase Key => Edges.First().Road;

            // 時計回りになるように入っている
            // =(交差点の外から見て0が右側)
            public List<RnNeighbor> Edges { get; } = new();

            public bool IsBorder => Key != null;

            // 流入してくるボーダー
            public IEnumerable<RnNeighbor> InBoundEdges => Edges.Where(n => (n.GetFlowType() & RnFlowTypeMask.Inbound) != 0);

            // 流出するボーダー
            public IEnumerable<RnNeighbor> OutBoundEdges => Edges.Where(n => (n.GetFlowType() & RnFlowTypeMask.Outbound) != 0);

            // 右側
            public EdgeGroup RightSide { get; set; }

            // 左側
            public EdgeGroup LeftSide { get; set; }

            // 法線方向
            public Vector3 Normal => Edges.First().Border.GetEdgeNormal(0).normalized;
        };

        /// <summary>
        /// 交差点のEdgesをRoadごとにグループ化する.
        /// RoadA -> null(境界線じゃない部分) -> RoadB -> null -> RoadC -> null -> RoadAのようになる
        /// </summary>
        /// <param name="self"></param>
        /// <returns></returns>
        public static List<EdgeGroup> CreateEdgeGroup(this RnIntersection self)
        {
            if (self.Edges.Count == 0)
                return new List<EdgeGroup>();
            self.Align();
            var ret = new List<EdgeGroup> { new() };

            for (var i = 0; i < self.Edges.Count; ++i)
            {
                var eg = ret[^1];
                var e0 = self.Edges[i];
                if (eg.Edges.Any() == false || eg.Key == e0.Road)
                {
                    eg.Edges.Add(e0);
                }
                else
                {
                    var next = new EdgeGroup();
                    next.Edges.Add(e0);
                    eg.LeftSide = next;
                    next.RightSide = eg;
                    ret.Add(next);
                }
            }

            if (ret.Count <= 1)
                return ret;

            if (ret[0].Key == ret[^1].Key)
            {
                ret[^1].Edges.AddRange(ret[0].Edges);
                ret.RemoveAt(0);
            }
            ret[0].RightSide = ret[^1];
            ret[^1].LeftSide = ret[0];

            return ret;
        }

        /// <summary>
        /// borderWayで指定したRnNeighborを取得する(基本的に0か1)
        /// </summary>
        /// <param name="self"></param>
        /// <param name="borderWay"></param>
        /// <returns></returns>
        public static IEnumerable<RnNeighbor> FindEdges(this RnIntersection self, RnWay borderWay)
        {
            if (self == null || borderWay == null)
                return Enumerable.Empty<RnNeighbor>();

            return self.Edges.Where(e => e.Border?.IsSameLine(borderWay) ?? false);
        }

        /// <summary>
        /// borderWayで指定した境界線と繋がっているレーンリスト(基本的に0か1)
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<RnLane> GetConnectedLanes(this RnIntersection self, RnWay borderWay)
        {
            return self.FindEdges(borderWay).SelectMany(n => n.GetConnectedLanes());
        }


        /// <summary>
        /// selfの全LinestringとlineSegmentの交点を取得する
        /// </summary>
        /// <param name="self"></param>
        /// <param name="lineSegment"></param>
        /// <returns></returns>
        public static LineCrossPointResult GetEdgeCrossPoints(this RnIntersection self, LineSegment3D lineSegment)
        {
            var targetLines = self.Edges
                .Select(l => l.Border)
                .Concat(self.SideWalks.SelectMany(s => s.SideWays));
            return RnEx.GetLineIntersections(lineSegment, targetLines);
        }
#if false
        /// <summary>
        /// a,bを繋ぐ経路を計算する
        /// </summary>
        /// <param name="self"></param>
        /// <param name="fromBorder"></param>
        /// <param name="toBorder"></param>
        /// <param name="tangentLength"></param>
        /// <param name="splitLength"></param>
        public static RnWay CalcTrackWay(this RnIntersection self, RnWay fromBorder, RnWay toBorder, float tangentLength = 10f, float splitLength = 2f)
        {
            if (fromBorder == toBorder)
                return null;
            var nFrom = self.Neighbors.FirstOrDefault(n => n.Border == fromBorder);
            var nTo = self.Neighbors.FirstOrDefault(n => n.Border == toBorder);
            if (nFrom == null || nTo == null)
                return null;
            if (nFrom.Border.Count < 2 || nTo.Border.Count < 2)
                return null;

            var fromLane = nFrom.GetConnectedLane();
            if (fromLane == null)
                return null;

            var toLane = nTo.GetConnectedLane();
            if (toLane == null)
                return null;


            fromLane.TryGetBorderNormal(nFrom.Border, out var aLeftPos, out var aLeftNormal, out var aRightPos, out var aRightNormal);
            toLane.TryGetBorderNormal(nTo.Border, out var bLeftPos, out var bLeftNormal, out var bRightPos, out var bRightNormal);

            fromBorder.GetLerpPoint(0.5f, out var fromPos);
            toBorder.GetLerpPoint(0.5f, out var toPos);

            var rightSp = new Spline
            {
                new(fromPos, tangentLength * aLeftNormal, tangentLength *aLeftNormal),
                new(toPos, tangentLength *bRightNormal, tangentLength *bRightNormal)
            };

            var num = Mathf.Max(2, Mathf.CeilToInt((fromPos - toPos).magnitude / splitLength));
            var rates = Enumerable.Range(0, num).Select(i => 1f * i / (num - 1)).ToList();
            return new RnWay(RnLineString.Create(rates.Select(t =>
            {
                rightSp.Evaluate(t, out var pos, out var tang, out var up);
                return (Vector3)pos;
            })));
        }
#endif
    }
}