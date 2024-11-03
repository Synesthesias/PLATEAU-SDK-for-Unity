using PLATEAU.CityInfo;
using PLATEAU.RoadNetwork.Util;
using PLATEAU.Util;
using PLATEAU.Util.GeoGraph;
using System;
using System.Collections;
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

        // この境界が道路と接続しているか(中央分離帯含む)
        public bool IsBorder => Road != null;

        // 有効値判定(Border != null)
        public bool IsValid => Border != null;

        /// <summary>
        /// 中央分離帯との境界線か
        /// </summary>
        public bool IsMedianBorder
        {
            get
            {
                if (IsBorder == false)
                    return false;

                if (Road is RnRoad road)
                {
                    if (road.MedianLane != null && road.MedianLane.AllBorders.Any(b => b.IsSameLine(Border)))
                        return true;
                }

                return false;
            }
        }

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
        /// <param name="allowSelfTrack">同じ道路への遷移を許可する</param>
        public void BuildTracks(float tangentLength = 10f, float splitLength = 2f, bool allowSelfTrack = false)
        {
            tracks.Clear();

            var edgeGroups = this.CreateEdgeGroup();

            foreach (var eg in edgeGroups.Where(e => e.IsBorder))
            {
                var inBounds = eg.InBoundEdges.ToList();

                foreach (var other in edgeGroups.Where(e => e.IsBorder && e != eg))
                {
                    // Uターンを許可しない場合
                    if (eg.Key == other.Key && allowSelfTrack == false)
                        continue;

                    // 隣り合っている場合.
                    RnWay way = null;

                    List<float> widthTable = null;
                    if (eg.LeftSide == other.RightSide)
                    {
                        var ls = RnLineString.Create(eg.LeftSide.Edges.SelectMany(e => e.Border.Points));
                        way = new RnWay(ls.Refined(1f), false, true);
                        widthTable = Enumerable.Range(0, way.Count).Select(x => 100f).ToList();
                    }
                    else if (eg.RightSide == other.LeftSide)
                    {
                        var ls = RnLineString.Create(eg.RightSide.Edges.SelectMany(e => e.Border.Points));
                        way = new RnWay(ls.Refined(1f), true, false);

                        var oLs = RnLineString.Create(other.RightSide.Edges.SelectMany(e => e.Border.Points));

                        widthTable = way.Points.Select(x =>
                        {
                            oLs.GetNearestPoint(x.Vertex, out var nearest, out var _, out var distance);
                            return distance;
                        }).ToList();


                    }

                    var turnType = RnTurnTypeEx.GetTurnType(-eg.Normal, other.Normal, AxisPlane.Xz);

                    void AddTrack(RnNeighbor from, RnNeighbor to, RnTurnType edgeTurnType)
                    {
                        var fromNormal = from.Border.GetEdgeNormal((from.Border.Count - 1) / 2);
                        var toNormal = -to.Border.GetEdgeNormal((to.Border.Count - 1) / 2);

                        from.Border.GetLerpPoint(0.5f, out var fromPos);
                        to.Border.GetLerpPoint(0.5f, out var toPos);

                        List<BezierKnot> knots = new();
                        knots.Add(new(fromPos, tangentLength * fromNormal, -tangentLength * fromNormal));

                        if (way != null && way.Count > 2)
                        {
                            Vector3 EdgeNormal(int startVertexIndex)
                            {
                                var p0 = way[startVertexIndex];
                                var p1 = way[startVertexIndex + 1];
                                // Vector3.Crossは左手系なので逆
                                var ret = (-Vector3.Cross(Vector3.up, p1 - p0)).normalized;
                                if (way.IsReverseNormal)
                                    ret = -ret;
                                return ret;
                            }

                            var sLen = (way[0] - fromPos).magnitude;
                            var eLen = (way[^1] - toPos).magnitude;

                            var length = way.CalcLength();
                            var len = 0f;

                            var index = 0;
                            // 現在見る点と次の点の辺/頂点の法線を保存しておく
                            // 線分の法線
                            var edgeNormal = new[] { (fromPos - way[0]).normalized, EdgeNormal(1) };
                            // 頂点の法線
                            var vertexNormal = new[] { edgeNormal[0], (edgeNormal[0] + edgeNormal[1]).normalized };
                            var delta = 1f;

                            for (var i = 0; i < way.Count - 1; i++)
                            {
                                var en0 = edgeNormal[index];
                                var en1 = edgeNormal[(index + 1) & 1];
                                var vn = vertexNormal[index];
                                // 形状維持するためにオフセット距離を変える
                                // en0成分の移動量がdeltaになるように, vnの移動量を求める
                                var m = Vector3.Dot(vn, en0);
                                var d = delta;
                                bool isZero = Mathf.Abs(m) < 1e-5f;
                                if (isZero == false)
                                    d /= m;

                                if (i < way.Count - 2)
                                {
                                    edgeNormal[index] = EdgeNormal(i + 1);
                                    vertexNormal[index] =
                                        (edgeNormal[index] + vertexNormal[(index + 1) & 1]).normalized;
                                }

                                index = (index + 1) & 1;
                                if (i != 0)
                                {
                                    var dIn = (way[i] - way[i - 1]);
                                    var dOut = (way[i + 1] - way[i]);

                                    len += dIn.magnitude;
                                    var p = len / length;
                                    //var l = Mathf.Lerp(sLen, eLen, p) * d;
                                    //var l = sLen * d;
                                    var l = Mathf.Lerp(Mathf.Min(sLen, widthTable[i]), eLen, p) * Mathf.Lerp(d, 1f, p);
                                    var pos = way[i] + vn * l;
                                    knots.Add(new(pos, 0.5f * dIn, 0.5f * dOut.normalized));
                                }
                                delta = d * Vector3.Dot(vn, en1);
                            }

                            var dSt = (Vector3)(knots[0].Position) - fromPos;
                            knots.Insert(0, new(fromPos, -0.5f * dSt, 0.5f * dSt));
                            knots.Add(new(toPos, tangentLength * toNormal, -tangentLength * toNormal));
                        }
                        else
                        {
                            knots.Add(new(fromPos, tangentLength * fromNormal, -tangentLength * fromNormal));
                            knots.Add(new(toPos, tangentLength * toNormal, -tangentLength * toNormal));

                        }

                        var spline = new Spline(knots);
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
        /// RoadA -> null(境界線じゃない部分) -> RoadB -> null -> RoadC -> null -> RoadAのようになる.
        /// 順番はEdgesの順番を保持する
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

        public class RecLine
        {
            public class PartialWay
            {
                public RnWay Way { get; set; }

                public RnPoint Start { get; set; }

                public RnPoint End { get; set; }

                public PartialWay(RnWay way) : this(way, way.GetPoint(0), way.GetPoint(-1))
                {
                }

                public PartialWay(RnWay way, RnPoint start, RnPoint end)
                {
                    Way = way;
                    Start = start;
                    End = end;
                }

                public PartialWayWork ToWork()
                {
                    return new PartialWayWork(this);
                }
            }

            // right -> left方向（時計回り)に格納されている
            public List<RnWay> Lines { get; } = new();

            public List<RecLine> Parents { get; set; } = new();

            public PartialWay LeftSide { get; set; }

            public PartialWay RightSide { get; set; }

            public class PartialWayWork : IList<RnPoint>
            {
                public RnWay Way { get; }

                // 開始インデックス
                public int StartIndex { get; private set; }

                // 終了インデックス
                public int EndIndex { get; private set; }

                public PartialWayWork(PartialWay way)
                {
                    Way = way.Way;
                    StartIndex = way.Way.FindPointIndex(way.Start);
                    EndIndex = way.Way.FindPointIndex(way.End) + 1;
                }


                public IEnumerator<RnPoint> GetEnumerator()
                {
                    for (var i = StartIndex; i < EndIndex; ++i)
                        yield return Way.GetPoint(i);
                }

                IEnumerator IEnumerable.GetEnumerator()
                {
                    return GetEnumerator();
                }

                public void Add(RnPoint item)
                {
                    Insert(Count, item);
                }

                public void Clear()
                {
                    // Clearは許可しない
                    throw new NotImplementedException();
                }

                public bool Contains(RnPoint item)
                {
                    return Way.Points.Skip(StartIndex).Take(EndIndex - StartIndex).Contains(item);
                }

                public void CopyTo(RnPoint[] array, int arrayIndex)
                {
                    // CopyToは許可しない
                    throw new NotImplementedException();
                }

                public bool Remove(RnPoint item)
                {
                    // Removeは許可しない
                    throw new NotImplementedException();
                }

                public int Count => EndIndex - StartIndex;

                public bool IsReadOnly => false;

                public int IndexOf(RnPoint item)
                {
                    var wayIndex = Way.FindPointIndex(item);
                    var index = FromWayIndex(wayIndex);
                    // 範囲外
                    if (index < 0 || index >= EndIndex)
                        return -1;
                    return index;
                }

                public void Insert(int index, RnPoint item)
                {
                    // Addと共通なのでCountも許可する
                    if (index < 0 || index > Count)
                        throw new ArgumentOutOfRangeException(nameof(index));
                    var lineStringIndex = Way.SwitchIndex(ToWayIndex(index));
                    Way.LineString.Points.Insert(lineStringIndex, item);
                    EndIndex++;
                }

                public void RemoveAt(int index)
                {
                    if (index < 0 || index >= Count)
                        throw new ArgumentOutOfRangeException(nameof(index));
                    var lineStringIndex = Way.SwitchIndex(ToWayIndex(index));
                    Way.LineString.Points.RemoveAt(lineStringIndex);
                    EndIndex--;
                }

                public RnPoint this[int index]
                {
                    get
                    {
                        if (index < 0 || index >= Count)
                            throw new ArgumentOutOfRangeException(nameof(index));
                        return Way.GetPoint(ToWayIndex(index));
                    }
                    // setは許可しない
                    set => throw new NotImplementedException();
                }

                public RnPoint Project(Vector3 pos, Vector3 dir)
                {
                    var nearest = Vector3.zero;
                    float len = float.MaxValue;
                    int minIndex = -1;
                    var index = 0;

                    foreach (var s in GeoGraphEx.GetEdges(this.Select(p => p.Vertex), false))
                    {
                        var seg = new LineSegment3D(s.Item1, s.Item2);

                        if (seg.TryHalfLineIntersectionBy2D(pos, dir, AxisPlane.Xz, -1f, out var v, out var t1,
                                out var _))
                        {
                            v = seg.Lerp(t1);
                            if ((v - pos).sqrMagnitude < len)
                            {
                                minIndex = index;
                                len = (v - pos).sqrMagnitude;
                                nearest = v;
                            }
                        }

                        index++;
                    }

                    if (minIndex < 0)
                        return null;

                    var tolerance = 1e-1f;
                    if ((this[minIndex] - nearest).magnitude < tolerance)
                    {
                        if (minIndex == 0)
                            return null;
                        return this[minIndex];
                    }

                    if ((this[minIndex + 1] - nearest).magnitude < 1e-1f)
                        return this[minIndex + 1];

                    var p = new RnPoint(nearest);
                    Insert(minIndex + 1, p);
                    return p;
                }

                private int ToWayIndex(int index)
                {
                    return index + StartIndex;
                }

                private int FromWayIndex(int index)
                {
                    return index - StartIndex;
                }
            }

            public RecLine CreateChild()
            {
                Vector2 Vec2(Vector3 v) => v.ToVector2(AxisPlane.Xz);

                var leftWay = new PartialWayWork(LeftSide);
                var rightWay = new PartialWayWork(RightSide);
                var leftSides = new List<RnPoint> { leftWay[0], leftWay[1] };
                var rightSides = new List<RnPoint> { rightWay[0], rightWay[1] };

                float GetLastLength(List<RnPoint> sides)
                {
                    return Vec2(sides[^1].Vertex - sides[^2].Vertex).magnitude;
                }
                var leftLength = GetLastLength(leftSides);
                var rightLength = GetLastLength(rightSides);

                while (leftSides.Count < leftWay.Count || rightSides.Count < rightWay.Count)
                {
                    var isLeft = leftSides.Count < leftWay.Count &&
                                 (rightSides.Count >= rightWay.Count || leftLength < rightLength);
                    if (isLeft)
                    {
                        leftSides.Add(leftWay[leftSides.Count]);
                    }
                    else
                    {
                        rightSides.Add(rightWay[rightSides.Count]);
                    }

                    var convex = GeoGraph2D.ComputeConvexVolume(
                        leftSides.Concat(rightSides)
                        , v => v.Vertex, AxisPlane.Xz, sameLineTolerance: 1e-3f).ToHashSet();

                    if (convex.Count != (leftSides.Count + rightSides.Count))
                    {
                        if (isLeft)
                            leftSides.RemoveAt(leftSides.Count - 1);
                        else
                            rightSides.RemoveAt(rightSides.Count - 1);
                        break;
                    }

                    if (isLeft)
                        leftLength += GetLastLength(leftSides);
                    else
                        rightLength += GetLastLength(rightSides);
                }

                var left = new PartialWay(leftWay.Way, leftSides[0], leftSides[^1]);
                var right = new PartialWay(rightWay.Way, rightSides[0], rightSides[^1]);

                var right2LeftDir = (leftSides[0].Vertex - rightSides[0].Vertex);
                if (leftLength < rightLength)
                {
                    var rp = rightWay.Project(leftSides[^1].Vertex, -right2LeftDir);
                    if (rp != null)
                        right.End = rp;
                }
                else
                {
                    var lp = leftWay.Project(rightSides[^1].Vertex, right2LeftDir);
                    if (lp != null)
                        left.End = lp;
                }

                var ret = new RecLine
                {
                    LeftSide = left,
                    RightSide = right,
                    Parents = new List<RecLine> { this }
                };
                var lens = Lines.Select(l => l.CalcLength()).ToList();

                var sum = lens.Sum();
                var w = 0f;
                var last = right.End;
                foreach (var f in lens)
                {
                    w += f / sum;
                    var p = w > 1 - 1e-3f ? left.End : new RnPoint(Vector3.Lerp(right.End.Vertex, left.End.Vertex, w));
                    ret.Lines.Add(new RnWay(RnLineString.Create(new[] { last, p })));
                    last = p;
                }

                return ret;
            }
        }

        public static List<RecLine> CreateRecLine(this RnIntersection self)
        {
            var edgeGroups = self.CreateEdgeGroup();
            var ret = new List<RecLine>();
            Dictionary<RnLineString, RnLineString> buffer = new();

            RecLine.PartialWay PartialWay(RnWay way)
            {
                var line = buffer.GetValueOrCreate(way.LineString, ls => ls.Clone());
                var w = new RnWay(line, way.IsReversed, way.IsReverseNormal);
                return new RecLine.PartialWay(w);
            }

            foreach (var index in Enumerable.Range(0, edgeGroups.Count))
            {
                var e = edgeGroups[index];
                if (e.Key != null)
                {
                    var rc = new RecLine();
                    rc.Lines.AddRange(e.Edges.Select(n => n.Border));
                    var right = edgeGroups[(index - 1 + edgeGroups.Count) % edgeGroups.Count];
                    var left = edgeGroups[(index + 1) % edgeGroups.Count];
                    rc.LeftSide = PartialWay(left.Edges.First().Border);
                    rc.RightSide = PartialWay(right.Edges.First().Border.ReversedWay());
                    ret.Add(rc);
                }
            }
            return ret;
        }
        public class LineStringTracer
        {
            public RnLineString LineString { get; set; }

            // 先頭側から
            public RnPoint FrontEnd { get; set; }

            public RnPoint BackEnd { get; set; }
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