using PLATEAU.RoadNetwork.Util;
using PLATEAU.Util;
using PLATEAU.Util.GeoGraph;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

namespace PLATEAU.RoadNetwork.Structure
{
    /// <summary>
    /// 交差点<see cref="RnIntersection"/>からトラック<see cref="RnTrack"/>を生成します。
    /// </summary>
    internal class RnTracksBuilder
    {
        /// <summary>
        /// Trackの再生成
        /// </summary>
        public void BuildTracks(RnIntersection intersection, BuildTrackOption op = null)
        {
            op ??= new();
            if (op.ClearTracks)
                intersection.ClearTracks();

            // fromEg -> toEgに対する中心線とその各点に置ける幅のテーブル
            ThickCenterLineTables thickCenterLinTables = new();


            var borderEdgeGroups = intersection.CreateEdgeGroup().Where(e => e.IsBorder).ToList();
            for (var startEdgeIndex = 0; startEdgeIndex < borderEdgeGroups.Count; ++startEdgeIndex)
            {
                var fromEg = borderEdgeGroups[startEdgeIndex];
                var inBoundsLeft2Right = fromEg.InBoundEdges.ToList().Reversed().ToList();
                if (inBoundsLeft2Right.Count == 0)
                    continue;

                if (fromEg.IsValid == false)
                    continue;
                // fromEg -> toEgのTurnTypeとtoEgよりも左側に同じTurnTypeがいくつあるか
                // fromEgから出ていくTrackのTurnTypeをTrack数分の配列で事前に作成
                // 左折2レーン, 直進2レーン, 右折1レーンの場合以下のようになる
                // [Left, Left, Straight, Straight, Right]

                List<OutBound> outBoundsLeft2Rights = new();

                // fromEgから出ていくTrackのTurnTypeのテーブルをあらかじめ作成
                var size = op.AllowSelfTrack ? borderEdgeGroups.Count : borderEdgeGroups.Count - 1;
                for (var i = 0; i < size; i++)
                {
                    var index = (startEdgeIndex + i + 1) % borderEdgeGroups.Count;
                    var toEg = borderEdgeGroups[index];
                    if (toEg.IsValid == false)
                        continue;
                    if (toEg.OutBoundEdges.Any() == false)
                        continue;
                    var turnType = RnTurnTypeEx.GetTurnType(-fromEg.Normal, toEg.Normal, RnModel.Plane);
                    foreach (var to in toEg.OutBoundEdges)
                    {
                        outBoundsLeft2Rights.Add(new OutBound(turnType, toEg, to));
                    }
                }

                // 行き先が無い場合は無視
                if (outBoundsLeft2Rights.Count == 0)
                    continue;


                if (inBoundsLeft2Right.Count > outBoundsLeft2Rights.Count)
                {
                    // 一番右のトラックに余った分を割り当てる
                    for (var i = 0; i < inBoundsLeft2Right.Count; ++i)
                    {
                        var outBoundIndex = Mathf.Clamp(i, 0, outBoundsLeft2Rights.Count - 1);
                        var from = inBoundsLeft2Right[i];
                        var outBound = outBoundsLeft2Rights[outBoundIndex];
                        var track = MakeTrack(intersection, from, op, fromEg,
                            thickCenterLinTables, outBound);
                        intersection.TryAddOrUpdateTrack(track);
                    }
                }
                else
                {
                    var inBoundIndex = 0;
                    for (var i = 0; i < outBoundsLeft2Rights.Count; ++i)
                    {
                        // 残りが足りない場合は進める
                        // i=0版は必ずinBoundIndex=0にする
                        // inBoundIndexが次に進めるかチェックする
                        if (i > 0 && inBoundIndex < inBoundsLeft2Right.Count - 1)
                        {
                            // 残りの流出先の数と流入先の数が同じ場合は残りは1:1対応なので進める
                            if ((inBoundsLeft2Right.Count - inBoundIndex) > (outBoundsLeft2Rights.Count - i))
                            {
                                inBoundIndex++;
                            }
                            else
                            {
                                // 直進の時は可能な限り流入位置とまっすぐの位置になる物を採用するようにする
                                var to = outBoundsLeft2Rights[i];
                                var toPos = RnIntersection.GetEdgeCenter2D(to.To);
                                // 直進に関しては可能な限り流入位置とまっすぐの位置になる物を採用するようにする
                                if (to.TurnType == RnTurnType.Straight)
                                {
                                    var now = inBoundsLeft2Right[inBoundIndex];
                                    var next = inBoundsLeft2Right[inBoundIndex + 1];

                                    var nowDir = -RnIntersection.GetEdgeNormal2D(now);
                                    var nowPos = RnIntersection.GetEdgeCenter2D(now);
                                    var nowAngle = Vector2.Angle(nowDir, toPos - nowPos);

                                    var nextDir = -RnIntersection.GetEdgeNormal2D(next);
                                    var nextPos = RnIntersection.GetEdgeCenter2D(next);
                                    var nextAngle = Vector2.Angle(nextDir, toPos - nextPos);

                                    if (nowAngle > nextAngle)
                                    {
                                        inBoundIndex++;
                                    }
                                }
                            }
                        }

                        var fromNeighbor = inBoundsLeft2Right[inBoundIndex];
                        var outBound = outBoundsLeft2Rights[i];
                        var track = MakeTrack(intersection, fromNeighbor, op, fromEg, thickCenterLinTables, outBound);
                        intersection.TryAddOrUpdateTrack(track);
                    }
                }
            }
        }

        public RnTrack MakeTrack(RnIntersection intersection, RnIntersectionEdge from, BuildTrackOption op,
            RnIntersectionEx.EdgeGroup fromEg, ThickCenterLineTables thickCenterLinTables, OutBound outBound)
        {

            // 対象外のものは無視
            if (op.IsBuildTarget(intersection, from, outBound.To) == false)
                return null;

            // 隣り合っている場合.
            var (way, widthTable) = GetThickCenterLineWay(intersection, fromEg, outBound.ToEg, thickCenterLinTables);

            var track = CreateTrackOrDefault(intersection, op, way, widthTable, from, outBound.To, outBound.TurnType);
            return track;
        }

        private (RnWay, List<float>) GetThickCenterLineWay(RnIntersection intersection,
            RnIntersectionEx.EdgeGroup fromEg,
            RnIntersectionEx.EdgeGroup toEg,
            ThickCenterLineTables thickCenterLinTables)
        {
            var tables = thickCenterLinTables.Data.GetValueOrCreate(fromEg);
            var centerGraph = intersection.CreateCenterLineGraphOrDefault();
            if (tables.ContainsKey(toEg) == false)
            {
                // 反対側に行き過ぎないように
                // ベースとなる線分と, 反対側の線分の距離テーブル
                // ベース線分がother.RightSideの場合は, other.LeftSideとの距離テーブルを作成する
                // ベース線分が中央線の場合はother.RightSideとの距離テーブルを作成する
                //   |  A  | 
                //   |  .  |
                //   |  .   ------  C
                //   |  .  ....... 
                //   |  .   ------
                //   |  .  |
                //   |  B  |
                // .は中央線. |-は輪郭線
                // B -> Aに行く場合は, B.LeftSide == A.RightSide なので AのLeftSideとの距離テーブルを作成
                // B -> Cに行く場合は, 右折なので中央線をベースラインに使う. -> CのRightSideと中央線の距離テーブルを作成
                List<float> widthTable = null;

                // 隣り合っている場合.
                RnWay way = null;
                // 左側で隣接している場合はそれをそのまま使う
                if (fromEg.LeftSide == toEg.RightSide)
                {
                    // #TODO : 以下のようなfrom -> toでかつxがどこともつながっていないただの輪郭線の場合に, 中央線がxでストップしてしまった
                    //          to
                    //        _____
                    //   ____|     |
                    //  | x
                    //   -----
                    //        |
                    //         ====
                    //          from
                    var ls = RnLineString.Create(fromEg.LeftSide.Edges.SelectMany(e => e.Border.Points));
                    way = new RnWay(ls.Refined(1f), false, true);
                    var oLs = RnLineString.Create(toEg.LeftSide.Edges.SelectMany(e => e.Border.Points));
                    widthTable = way.Points.Select(x =>
                    {
                        oLs.GetNearestPoint(x.Vertex, out var nearest, out var _, out var distance);
                        return distance;
                    }).ToList();
                }
                // それ以外の場合は中央線を使う
                // ただし自分自身に戻る(Uターン)の場合は中央線使わない
                else if (fromEg.Key != toEg.Key && centerGraph != null)
                {
                    way = centerGraph.CenterLines.GetValueOrDefault(fromEg.Key)?.GetValueOrDefault(toEg.Key);
                    var oLs = RnLineString.Create(toEg.RightSide.Edges.SelectMany(e => e.Border.Points));

                    if (way != null)
                    {
                        widthTable = way.Points.Select(x =>
                        {
                            oLs.GetNearestPoint(x.Vertex, out var nearest, out var _, out var distance);
                            return distance;
                        }).ToList();
                    }
                }

                tables[toEg] = (way, widthTable);
            }

            return tables[toEg];
        }

        /// <summary>
        /// from/toを繋ぐトラックを作成する
        /// </summary>
        public RnTrack CreateTrackOrDefault(RnIntersection intersection, BuildTrackOption op, RnWay way,
            List<float> widthTable, RnIntersectionEdge from, RnIntersectionEdge to, RnTurnType edgeTurnType)
        {
            var fromNormal = RnIntersection.GetEdgeNormal(from);
            var toNormal = RnIntersection.GetEdgeNormal(to);

            from.Border.GetLerpPoint(0.5f, out var fromPos);
            to.Border.GetLerpPoint(0.5f, out var toPos);

            // 先に1回のカーブで繋がるトラックをチェック(そっちの方がきれいな曲線になりやすいので)
            var track = TryCreateTwoLineTrack(intersection, fromPos, fromNormal, toPos, toNormal, from, to,
                edgeTurnType);
            if (track != null)
                return track;

            // 作れない場合直進で繋がるかチェックする
            track = TryCreateOneLineTrack(intersection, fromPos, fromNormal, toPos, toNormal, from, to, edgeTurnType);
            if (track != null)
                return track;

            // 中央テーブル作れない時は諦める
            if (widthTable == null)
                return null;

            List<BezierKnot> knots = new()
            {
                new BezierKnot(fromPos, op.TangentLength * fromNormal, -op.TangentLength * fromNormal)
            };

            void AddKnots(Vector3 pos, bool check = true)
            {
                // #NOTE : 戻りが発生しないように90度以上の角度を持つ場合は無視する
                if (knots.Count >= 2 && check)
                {
                    Vector3 d1 = pos - (Vector3)knots[^1].Position;
                    Vector3 d2 = knots[^1].Position - knots[^2].Position;

                    if (Vector2.Angle(d1.Xz(), d2.Xz()) > 90)
                        return;
                }

                var tanIn = 0.5f * (knots[^1].Position - (float3)pos);
                var knot = new BezierKnot(pos, tanIn, -tanIn);
                var last = knots[^1];
                last.TangentOut = -knot.TangentIn;
                knots[^1] = last;
                knots.Add(knot);
            }

            // ベースラインを法線方向に動かしてトラックラインを作成する
            // 入口の道路幅と出口の道路幅が違うので, 法線方向へのオフセットも距離に応じて線形補完する
            // そのうえで、なるべく輪郭を崩さないようにする処理も入れる
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

                // 始点と終点でベースラインをまたぐ場合があるので法線からの方向を記録しておく
                var sSign = Vector3.Dot(fromPos - way[0], EdgeNormal(0)) < 0 ? -1 : 1;
                var eSign = Vector3.Dot(toPos - way[^1], EdgeNormal(way.Count - 2)) < 0 ? -1 : 1;

                var index = 0;
                // 現在見る点と次の点の辺/頂点の法線を保存しておく
                // 線分の法線
                var edgeNormal = new[] { (fromPos - way[0]).normalized, EdgeNormal(1) };
                // 先頭の法線が逆の場合計算がおかしくなるので反転して最後に適用するときに戻す
                if (sSign < 0)
                    edgeNormal[0] = edgeNormal[0].AxisSymmetric(way[1] - way[0]);

                // 頂点の法線
                var vertexNormal = new[] { edgeNormal[0], (edgeNormal[0] + edgeNormal[1]).normalized };

                var length = way.CalcLength();
                var len = 0f;
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
                        len += (way[i] - way[i - 1]).magnitude;
                        var p = len / length;

                        var sL = Mathf.Min(sLen, widthTable[i]);
                        var eL = Mathf.Min(eLen, widthTable[i]);
                        //var l = Mathf.Lerp(sL, eL, p) * d;
                        //var l = sL * d;
                        var l = Mathf.Lerp(sSign * sL, eSign * eL, p) * Mathf.Lerp(d, 1f, p);
                        var pos = way[i] + vn * l;
                        AddKnots(pos);
                    }

                    delta = d * Vector3.Dot(vn, en1);
                }

                AddKnots(toPos, false);
            }
            else
            {
                AddKnots(toPos, false);
            }

            var spline = new Spline(knots);
            return new RnTrack(from.Border, to.Border, spline, edgeTurnType);
        }


        /// <summary>
        /// トラックを直線とするにふさわしいかチェックし、そうなら生成して返します。そうでないならnullを返します。
        /// </summary>
        private RnTrack TryCreateOneLineTrack(RnIntersection intersection, Vector3 fromPos, Vector3 fromNormal,
            Vector3 toPos, Vector3 toNormal, RnIntersectionEdge from, RnIntersectionEdge to, RnTurnType edgeTurnType)
        {
            // 直進で繋がるとき
            var segment = new LineSegment3D(fromPos - fromNormal * 0.1f,
                toPos - toNormal * 0.1f);

            // 直進でつながったとしても角度つくようだとダメ
            // #TODO : マジックナンバー
            var ang = Vector2.Angle(-fromNormal.Xz(), segment.Direction.Xz());
            if (ang > 15)
                return null;

            var oneLineCrossPoints =
                intersection.GetEdgeCrossPoints(segment);

            // 途中でEdgesと交差する場合は外に出るからダメ
            if (IsCollide(oneLineCrossPoints, from, to))
                return null;


            var spline = new Spline
            {
                { new BezierKnot(fromPos), TangentMode.AutoSmooth },
                { new BezierKnot(toPos), TangentMode.AutoSmooth }
            };
            return new RnTrack(from.Border, to.Border, spline, edgeTurnType);
        }

        /// <summary>
        /// トラックが1回のカーブとするにふさわしいかチェックし、そうならトラックを生成して返します。そうでないならnullを返します。
        /// </summary>
        private RnTrack TryCreateTwoLineTrack(RnIntersection intersection, Vector3 fromPos, Vector3 fromNormal,
            Vector3 toPos, Vector3 toNormal, RnIntersectionEdge from, RnIntersectionEdge to, RnTurnType edgeTurnType)
        {
            var offset = 0.01f;
            var fromRay = new Ray(fromPos - fromNormal * offset, -fromNormal);
            var toRay = new Ray(toPos - toNormal * offset, -toNormal);

            // 交差しない場合は無視
            if (fromRay.CalcIntersectionBy2D(toRay, RnDef.Plane, out var cp, out var _, out var _) == false)
                return null;

            // 交点が内部に無い場合は無視
            if (intersection.IsInside2D(cp) == false)
                return null;

            // cpからfromPos/toPosに向かう線分とEdgesの交点を取得
            // 交点がある場合は外に出てしまうので無視
            var cp1 =
                intersection.GetEdgeCrossPoints(new LineSegment3D(fromPos - fromNormal * 0.1f, cp));

            var cp2
                = intersection.GetEdgeCrossPoints(new LineSegment3D(cp, toPos - toNormal * 0.1f));

            if (IsCollide(cp1, from, to) || IsCollide(cp2, from, to))
                return null;

            var curveOffset = 3;
            var spline = new Spline();
            spline.Add(new BezierKnot(fromPos), TangentMode.AutoSmooth);

#if false
                spline.Add(new BezierKnot(cp), TangentMode.AutoSmooth);
#else
            var fromCpLen = (fromPos - cp).magnitude;
            if (fromCpLen > 1e-3f)
            {
                var len = Mathf.Clamp(curveOffset, 0, fromCpLen - 1);
                var p = Vector3.Lerp(cp, fromPos, len / fromCpLen);
                spline.Add(new BezierKnot(p, 0, -fromNormal * len), TangentMode.AutoSmooth);
            }

            var toCpLen = (toPos - cp).magnitude;
            if (toCpLen > 1e-3f)
            {
                var len = Mathf.Clamp(curveOffset, 0, toCpLen - 1);
                var p = Vector3.Lerp(cp, toPos, len / toCpLen);
                spline.Add(new BezierKnot(p, toNormal * len, 0), TangentMode.AutoSmooth);
            }

            ;
#endif
            spline.Add(new BezierKnot(toPos), TangentMode.AutoSmooth);
            return new RnTrack(from.Border, to.Border, spline, edgeTurnType);
        }

        private bool IsCollide(LineCrossPointResult lcp, RnIntersectionEdge from, RnIntersectionEdge to)
        {
            return lcp.CrossingLines.Any(t =>
                t.LineString != from.Border.LineString && t.LineString != to.Border.LineString);
        }

        internal class OutBound
        {
            public RnTurnType TurnType { get; set; }
            public RnIntersectionEx.EdgeGroup ToEg { get; set; }
            public RnIntersectionEdge To { get; set; }

            public OutBound(RnTurnType turnType, RnIntersectionEx.EdgeGroup toEg, RnIntersectionEdge to)
            {
                TurnType = turnType;
                ToEg = toEg;
                To = to;
            }
        }

        /// <summary>
        /// fromEg -> toEgに対する中心線とその各点に置ける幅のテーブル
        /// </summary>
        internal class ThickCenterLineTables
        {
            public Dictionary<RnIntersectionEx.EdgeGroup, Dictionary<RnIntersectionEx.EdgeGroup, (RnWay, List<float>)>>
                Data = new();
        }
    }


    public class BuildTrackOption
    {
        /// <summary>
        /// スプラインのTangentの長さ
        /// </summary>
        public float TangentLength { get; set; } = 10f;

        /// <summary>
        /// 同じ道路への侵入を許可する
        /// </summary>
        public bool AllowSelfTrack { get; set; } = false;

        /// <summary>
        /// 生成前に,既存のTrackをクリアする
        /// </summary>
        public bool ClearTracks { get; set; } = true;

        /// <summary>
        /// 現在存在しないトラックのみをビルド対象とする(ClearTracks=trueだと実質的に全トラックが対象)
        /// </summary>
        public bool UnCreatedTrackOnly { get; set; } = true;

        /// <summary>
        /// ここで指定されたLineStringを境界とするトラックのみビルド対象とする.
        /// 空の場合は全てのトラックが対象となる
        /// </summary>
        public HashSet<RnLineString> TargetBorderLineStrings { get; set; } = new();

        /// <summary>
        /// ビルド対象のトラックかどうか
        /// </summary>
        public bool IsBuildTarget(RnIntersection intersection, RnIntersectionEdge from, RnIntersectionEdge to)
        {
            if (intersection == null)
                return false;

            if (UnCreatedTrackOnly)
            {
                // すでに存在するトラックは対象外
                if (intersection.Tracks.Any(t => t.IsSameInOut(from.Border, to.Border)))
                    return false;
            }

            if (TargetBorderLineStrings.Count != 0)
            {
                if (TargetBorderLineStrings.Contains(from.Border.LineString) == false
                    && TargetBorderLineStrings.Contains(to.Border.LineString) == false)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// 全トラックの再生成
        /// </summary>
        /// <returns></returns>
        public static BuildTrackOption Default() => new();

        /// <summary>
        /// 未生成のトラックだけを対象とする
        /// </summary>
        /// <returns></returns>
        public static BuildTrackOption UnBuiltTracks()
        {
            var ret = new BuildTrackOption { ClearTracks = false, UnCreatedTrackOnly = true, };

            return ret;
        }

        /// <summary>
        /// 指定した境界線に関係するトラックのみビルド対象とする
        /// </summary>
        /// <param name="borders"></param>
        /// <returns></returns>
        public static BuildTrackOption WithBorder(IEnumerable<RnLineString> borders)
        {
            var ret = new BuildTrackOption
            {
                ClearTracks = false,
                // 指定したトラックのみビルド対象とする
                TargetBorderLineStrings = borders.Where(x => x != null).ToHashSet(),
            };

            return ret;
        }
    }
}