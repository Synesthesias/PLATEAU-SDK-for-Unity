using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using PLATEAU.RoadNetwork.Data;
using PLATEAU.RoadNetwork.Structure;
using PLATEAU.Util;

namespace PLATEAU.Editor.RoadNetwork.Exporter
{
    public class RoadNetworkExporter
    {
        /// <summary>
        /// 座標参照系（CRS: EPSG 6697）
        /// </summary>
        private const string CRS = "urn:ogc:def:crs:EPSG::6697";

        /// <summary>
        /// 道路ネットワークのコンテキスト情報
        /// </summary>
        private RoadNetworkContext roadNetworkContext;

        // エクスポートするファイル名の定義

        public const string ExportFileNameLink = "roadnetwork_link.geojson";
        public const string ExportFileNameLane = "roadnetwork_lane.geojson";
        public const string ExportFileNameNode = "roadnetwork_node.geojson";
        public const string ExportFileNameTrack = "roadnetwork_track.geojson";
        public const string ExportFileNameSignalControler = "roadnetwork_signalcontroler.geojson";
        public const string ExportFileNameSignalLight = "roadnetwork_signallight.geojson";
        public const string ExportFileNameSignalStep = "roadnetwork_signalstep.geojson";

        /// <summary>
        /// エクスポートパス
        /// </summary>
        private string exportPath = Application.persistentDataPath;

        /// <summary>
        /// ノードのリスト
        /// </summary>
        private List<RoadNetworkElementNode> Nodes { get; set; } = new List<RoadNetworkElementNode>();

        /// <summary>
        /// リンクのリスト
        /// </summary>
        private List<RoadNetworkElementLink> Links { get; set; } = new List<RoadNetworkElementLink>();

        /// <summary>
        /// 信号制御器のリスト
        /// </summary>
        private List<RoadNetworkElementSignalController> SignalControllers { get; set; } = new List<RoadNetworkElementSignalController>();

        /// <summary>
        /// 信号灯火器のリスト
        /// </summary>
        private List<RoadNetworkElementSignalLight> SignalLights { get; set; } = new List<RoadNetworkElementSignalLight>();

        /// <summary>
        /// 信号現示階梯のリスト
        /// </summary>
        private List<RoadNetworkElementSignalStep> SignalSteps { get; set; } = new List<RoadNetworkElementSignalStep>();

        /// <summary>
        /// 道路ネットワークを指定されたパスにエクスポートします
        /// </summary>
        /// <param name="path">エクスポート先のパス</param>
        public void ExportRoadNetwork(string path)
        {
            exportPath = path;

            var rnStructureModel = GameObject.FindObjectOfType<PLATEAURnStructureModel>();
            if (rnStructureModel == null)
            {
                Dialogue.Display("道路ネットワークがシーン中に見つかりませんでした。", "OK");
                return;
            }
            rnStructureModel.Serialize();
            
            roadNetworkContext = new RoadNetworkContext(rnStructureModel);

            if (!roadNetworkContext.IsInitSucceed) return;

            GenerateExpRoadNetwork(roadNetworkContext);

            ExportNode(Nodes);

            ExportLink(Links);

            ExportLane(Links);

            ExportTrack(Nodes);

            ExportSignalController(SignalControllers);

            ExportSignalLight(SignalLights);

            ExportSignalStep(SignalSteps);
        }

        /// <summary>
        /// エクスポート用の道路ネットワークを生成します
        /// </summary>
        /// <param name="context">道路ネットワークのコンテキスト</param>
        private void GenerateExpRoadNetwork(RoadNetworkContext context)
        {
            var roadNetworkRoads = context.RoadNetworkGetter.GetRoadBases();

            var roadNetworkNodes = roadNetworkRoads.OfType<RnDataIntersection>().ToList();

            var roadNetworkLinks = roadNetworkRoads.OfType<RnDataRoad>().ToList();

            // ノードの生成

            var simRoadNetworkNodes = new List<RoadNetworkElementNode>();

            foreach (var node in roadNetworkRoads.Select((value, index) => new { value, index }))
            {
                if (node.value is RnDataIntersection)
                {
                    simRoadNetworkNodes.Add(new RoadNetworkElementNode(context, simRoadNetworkNodes.Count.ToString(), node.index));
                }
            }

            Nodes = simRoadNetworkNodes;

            // リンクの生成

            var simRoadNetworkLinks = new List<RoadNetworkElementLink>();

            Dictionary<int, RoadNetworkElementNode> vNodes = new Dictionary<int, RoadNetworkElementNode>();

            foreach (var link in roadNetworkLinks.Select((value, index) => new { value, index }))
            {
                // リンクが接続されていない場合はスキップ
                if (!link.value.Next.IsValid && !link.value.Prev.IsValid)
                {
                    Debug.LogWarning("Link is not connected to any node.");

                    continue;
                }

                var next = link.value.Next.IsValid ? roadNetworkRoads[link.value.Next.ID] as RnDataIntersection : null;
                var prev = link.value.Prev.IsValid ? roadNetworkRoads[link.value.Prev.ID] as RnDataIntersection : null;

                //　仮想ノードが割り当てられている場合はそのノードを取得
                RoadNetworkElementNode vNext = vNodes.ContainsKey(link.value.Next.ID) ? vNodes[link.value.Next.ID] : null;
                RoadNetworkElementNode vPrev = vNodes.ContainsKey(link.value.Prev.ID) ? vNodes[link.value.Prev.ID] : null;

                // 接続先がリンクかつ仮想ノードが割り当てられていない場合は仮想ノードを生成
                // 接続点ノードが割り当てられていない場合への対処
                if (next == null && link.value.Next.IsValid && roadNetworkRoads[link.value.Next.ID] as RnDataRoad != null && vNext == null)
                {
                    vNext = new RoadNetworkElementNode(context, simRoadNetworkNodes.Count.ToString(), -1);

                    simRoadNetworkNodes.Add(vNext);

                    vNodes.Add(link.index, vNext);
                }
                if (prev == null && link.value.Prev.IsValid && roadNetworkRoads[link.value.Prev.ID] as RnDataRoad != null && vPrev == null)
                {
                    vPrev = new RoadNetworkElementNode(context, simRoadNetworkNodes.Count.ToString(), -1);

                    simRoadNetworkNodes.Add(vPrev);

                    vNodes.Add(link.index, vPrev);
                }

                // 終端の仮想ノードを生成
                if (next == null && vNext == null)
                {
                    vNext = new RoadNetworkElementNode(context, simRoadNetworkNodes.Count.ToString(), -1);

                    simRoadNetworkNodes.Add(vNext);
                }
                if (prev == null && vPrev == null)
                {
                    vPrev = new RoadNetworkElementNode(context, simRoadNetworkNodes.Count.ToString(), -1);

                    simRoadNetworkNodes.Add(vPrev);
                }

                var simNodeNext = next != null ? simRoadNetworkNodes.Find(x => x.OriginNode == next) : vNext;
                var simNodePrev = prev != null ? simRoadNetworkNodes.Find(x => x.OriginNode == prev) : vPrev;

                var simNodeNextID = simNodeNext.ID.Replace(RoadNetworkElementNode.IDPrefix, "");
                var simNodePrevID = simNodePrev.ID.Replace(RoadNetworkElementNode.IDPrefix, "");

                // リンクの生成
                var simLinkL = new RoadNetworkElementLink(context, link.index + "_" + simNodePrevID + "_" + simNodeNextID, link.index);
                var simLinkR = new RoadNetworkElementLink(context, link.index + "_" + simNodeNextID + "_" + simNodePrevID, link.index);

                simLinkL.IsReverse = false;
                simLinkR.IsReverse = true;

                simLinkL.UpNode = simNodePrev;
                simLinkL.DownNode = simNodeNext;

                simLinkR.UpNode = simNodeNext;
                simLinkR.DownNode = simNodePrev;

                simLinkL.Pair = simLinkR;
                simLinkR.Pair = simLinkL;

                simRoadNetworkLinks.Add(simLinkL);
                simRoadNetworkLinks.Add(simLinkR);
            }

            Links = simRoadNetworkLinks;

            // レーンの生成
            foreach (var link in Links)
            {
                link.GenerateLane();
            }

            // トラックの生成
            foreach (var node in Nodes)
            {
                node.GenerateTrack(Links);
            }

            // 接続点ノード・仮想ノードの座標を設定
            foreach (var node in Nodes)
            {
                if (node.IsVirtual)
                {
                    // 接続されているリンクの隣接情報から座標を取得する
                    // 交差点ではなく接続点なので上流・下流ともに１つでいい
                    var uplink = Links.Where(x => x.DownNode == node && x.GetOriginLanes().Count > 0);
                    var downlink = Links.Where(x => x.UpNode == node && x.GetOriginLanes().Count > 0);

                    // uplinksの下流ボーダーとdownlinksの上流ボーダーの中点を取得
                    List<Vector3> points = new List<Vector3>();

                    if (uplink.Count() > 0)
                    {
                        points.Add(CalculateCenter(context, uplink.First().GetOriginLanes(), true));
                    }
                    if (downlink.Count() > 0)
                    {
                        points.Add(CalculateCenter(context, downlink.First().GetOriginLanes(), false));
                    }

                    node.Coord = CalculateCenter(points);
                }
            }
        }

        /// <summary>
        /// レーンのリストから中心点を計算します
        /// </summary>
        /// <param name="context">道路ネットワークのコンテキスト</param>
        /// <param name="lanes">レーンのリスト</param>
        /// <param name="isNext">次のボーダーを使用するかどうか</param>
        /// <returns>中心点の座標</returns>
        public Vector3 CalculateCenter(RoadNetworkContext context, List<RnID<RnDataLane>> lanes, bool isNext)
        {
            var allLanes = context.RoadNetworkGetter.GetLanes();

            var allWays = context.RoadNetworkGetter.GetWays();

            var allLineStrings = context.RoadNetworkGetter.GetLineStrings();

            var allPoints = context.RoadNetworkGetter.GetPoints();

            List<Vector3> borderVertices = new List<Vector3>();

            foreach (var lane in lanes)
            {
                var border = isNext ? allLanes[lane.ID].NextBorder : allLanes[lane.ID].PrevBorder;

                var way = allWays[border.ID];

                var lineString = allLineStrings[way.LineString.ID];

                foreach (var point in lineString.Points)
                {
                    borderVertices.Add(allPoints[point.ID].Vertex);
                }
            }

            return CalculateCenter(borderVertices);
        }

        /// <summary>
        /// 座標のリストから中心点を計算します
        /// </summary>
        /// <param name="positions">座標のリスト</param>
        /// <returns>中心点の座標</returns>
        public Vector3 CalculateCenter(List<Vector3> positions)
        {
            if (positions == null || positions.Count == 0)
                return Vector3.zero; // リストが空の場合、ゼロベクトルを返す

            Vector3 sum = Vector3.zero;

            // 全てのベクトルを合計
            foreach (Vector3 pos in positions)
            {
                sum += pos;
            }

            // 平均値を求める
            return sum / positions.Count;
        }

        /// <summary>
        /// リンクをエクスポートします
        /// </summary>
        /// <param name="simRoadNetworkLinks">リンクのリスト</param>
        private void ExportLink(List<RoadNetworkElementLink> simRoadNetworkLinks)
        {
            var simLinks = new List<GeoJsonFeature>();

            foreach (var simLink in simRoadNetworkLinks)
            {
                // レーンが存在しない場合はリンクを出力しない
                if (simLink.GetOriginLanes().Count == 0)
                {
                    Debug.Log("No valid lane is linked to this link. " + simLink.ID);

                    continue;
                }

                var geom = simLink.GetGeometory();

                var lineString = new GeoJSON.Net.Geometry.LineString(geom);

                var propety = new GeoJsonFeaturePropertiesLink
                {
                    ID = simLink.ID,
                    UPNODE = simLink.UpNode.ID,
                    DOWNNODE = simLink.DownNode.ID,
                    LENGTH = simLink.GetLinkLength(),
                    LANENUM = simLink.GetLaneNum(),

                    // 拡張性のために残す
                    RLANENUM = 0,
                    RLANELENGTH = 0,
                    LLANENUM = 0,
                    LLANELENGTH = 0,
                };

                simLinks.Add(new GeoJsonFeature(lineString, propety));
            }

            ExportGeoJson(simLinks, ExportFileNameLink);
        }

        /// <summary>
        /// レーンをエクスポートします
        /// </summary>
        /// <param name="simRoadNetworkLinks">リンクのリスト</param>
        private void ExportLane(List<RoadNetworkElementLink> simRoadNetworkLinks)
        {
            var simLanes = new List<GeoJsonFeature>();

            foreach (var simLink in simRoadNetworkLinks)
            {
                foreach (var simLane in simLink.Lanes)
                {
                    var geom = simLane.GetGeometory();

                    var lineString = new GeoJSON.Net.Geometry.LineString(geom);

                    var propety = new GeoJsonFeaturePropertiesLane
                    {
                        ID = simLane.ID,
                        LINKID = simLink.ID,
                        LANEPOS = simLane.Order,
                        LENGTH = simLane.GetLength(),
                        WIDTH = simLane.GetLaneWidth(),
                    };

                    simLanes.Add(new GeoJsonFeature(lineString, propety));
                }
            }

            ExportGeoJson(simLanes, ExportFileNameLane);
        }

        /// <summary>
        /// ノードをエクスポートします
        /// </summary>
        /// <param name="simRoadNetworkNodes">ノードのリスト</param>
        private void ExportNode(List<RoadNetworkElementNode> simRoadNetworkNodes)
        {
            var simNodes = new List<GeoJsonFeature>();

            foreach (var simNode in simRoadNetworkNodes)
            {
                var geom = simNode.GetGeometory();

                var point = new GeoJSON.Net.Geometry.Point(geom);

                var propety = new GeoJsonFeaturePropertiesNode
                {
                    ID = simNode.ID
                };

                simNodes.Add(new GeoJsonFeature(point, propety));
            }

            ExportGeoJson(simNodes, ExportFileNameNode);
        }

        /// <summary>
        /// トラックをエクスポートします
        /// </summary>
        /// <param name="simRoadNetworkNodes">ノードのリスト</param>
        private void ExportTrack(List<RoadNetworkElementNode> simRoadNetworkNodes)
        {
            var simNodes = new List<GeoJsonFeature>();

            foreach (var simNode in simRoadNetworkNodes)
            {
                foreach (var simTrack in simNode.Tracks)
                {
                    var geom = simTrack.GetGeometory();

                    var point = new GeoJSON.Net.Geometry.LineString(geom);

                    var propety = new GeoJsonFeaturePropertiesTrack
                    {
                        ID = simTrack.ID,
                        ORDER = simTrack.Order,
                        UPLINKID = simTrack.UpLink?.ID,
                        UPLANEPOS = simTrack.UpLane,
                        UPDISTANCE = simTrack.UpDistance,
                        DOWNLINKID = simTrack.DownLink?.ID,
                        DOWNLANEPOS = simTrack.DownLane,
                        DOWNDISTANCE = simTrack.DownDistance,
                        LENGTH = simTrack.GetLength(),
                    };

                    simNodes.Add(new GeoJsonFeature(point, propety));
                }
            }

            ExportGeoJson(simNodes, ExportFileNameTrack);
        }

        /// <summary>
        /// 信号制御器をエクスポートします。
        /// この機能は未完成です。
        /// </summary>
        /// <param name="simSignalControllers">信号制御器のリスト</param>
        private void ExportSignalController(List<RoadNetworkElementSignalController> simSignalControllers)
        {
            if (simSignalControllers.Count == 0) return;
            
            var geoJsonFeature = new List<GeoJsonFeature>();

            foreach (var simSignalController in simSignalControllers)
            {
                var geom = simSignalController.GetGeometory();

                var point = new GeoJSON.Net.Geometry.Point(geom);

                var propety = new GeoJsonFeaturePropertiesSignalController
                {
                    ID = simSignalController.ID,
                    ALLOCNODE = simSignalController.GetNode(),
                    SIGLIGHT = simSignalController.GetSignalLights(),
                    OFFSETBASESIGID = simSignalController.OffsetController != null ? simSignalController.OffsetController.ID : null,
                    NUMOFPATTERN = simSignalController.GetPatternNum(),
                    PATTERNID = simSignalController.GetPatternID(),
                    INITCYCLE = simSignalController.GetCycleLen(),
                    PHASENUM = simSignalController.GetPhaseNum(),
                    OFFSETTYPE = simSignalController.OffsetType,
                    OFFSET = simSignalController.OffsetValue,
                    STARTTIME = simSignalController.GetStartTime(),
                };

                geoJsonFeature.Add(new GeoJsonFeature(point, propety));
            }

            ExportGeoJson(geoJsonFeature, ExportFileNameSignalControler);
        }

        /// <summary>
        /// 信号灯火気をエクスポートします
        /// この機能は未完成です。
        /// </summary>
        /// <param name="simSignalLights">信号灯火器のリスト</param>
        private void ExportSignalLight(List<RoadNetworkElementSignalLight> simSignalLights)
        {
            if (simSignalLights.Count == 0) return;
            
            var geoJsonFeature = new List<GeoJsonFeature>();

            foreach (var simSignalLight in simSignalLights)
            {
                var geom = simSignalLight.GetGeometory();

                var point = new GeoJSON.Net.Geometry.Point(geom);

                var propety = new GeoJsonFeaturePropertiesSignalLight
                {
                    ID = simSignalLight.ID,
                    SIGNALID = simSignalLight.Controller.ID,
                    LINKID = simSignalLight.Link.ID,
                    LANETYPE = simSignalLight.GetLaneType(),
                    LANEPOS = simSignalLight.GetLanePos(),
                    DISTANCE = simSignalLight.GetDistance(),
                };

                geoJsonFeature.Add(new GeoJsonFeature(point, propety));
            }

            ExportGeoJson(geoJsonFeature, ExportFileNameSignalLight);
        }

        /// <summary>
        /// 信号現示階梯をエクスポートします。
        /// この機能は未完成です。
        /// </summary>
        /// <param name="simSignalSteps">信号現示階梯のリスト</param>
        private void ExportSignalStep(List<RoadNetworkElementSignalStep> simSignalSteps)
        {
            if (simSignalSteps.Count == 0) return;
            
            var geoJsonFeature = new List<GeoJsonFeature>();

            foreach (var simSignalStep in simSignalSteps)
            {
                var geom = simSignalStep.GetGeometory();

                var point = new GeoJSON.Net.Geometry.Point(geom);

                var propety = new GeoJsonFeaturePropertiesSignalStep
                {
                    ID = simSignalStep.ID,
                    PATTERNID = simSignalStep.PatternID,
                    ORDER = simSignalStep.Order,
                    DURATION = simSignalStep.Duration,
                    SIGLIGHT = simSignalStep.GetSignalLights(),
                    TYPEMASK = simSignalStep.GetTypeMask(),
                    GREEN = simSignalStep.GetColor(simSignalStep.LinkPairsGreen),
                    YELLOW = simSignalStep.GetColor(simSignalStep.LinkPairsYellow),
                    RED = simSignalStep.GetColor(simSignalStep.LinkPairsRed),
                };

                geoJsonFeature.Add(new GeoJsonFeature(point, propety));
            }

            ExportGeoJson(geoJsonFeature, ExportFileNameSignalStep);
        }

        /// <summary>
        /// GeoJSON形式でデータをエクスポートします
        /// </summary>
        /// <param name="features">GeoJSONフィーチャーのリスト</param>
        /// <param name="fileName">エクスポートするファイル名</param>
        private async void ExportGeoJson(List<GeoJsonFeature> features, string fileName)
        {
            var geoJson = GeoJsonExporter.CreateGeoJson(features, CRS);

            Debug.Log(geoJson);

            string path = Path.Combine(exportPath, fileName);

            await GeoJsonExporter.ExportGeoJsonAsync(geoJson, path, () => Debug.Log("Json file saved successfully."));
        }
    }
}