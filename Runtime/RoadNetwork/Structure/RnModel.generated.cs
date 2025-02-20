// GENERATED CODE
using System.Collections.Generic;
using System.Reflection;
namespace PLATEAU.RoadNetwork.Structure{
	internal class CollectRnModelWork{
		public HashSet<object> Visited {get;} = new();
		public HashSet<TrafficSignalLightController> TrafficSignalLightControllers { get; } = new ();
		public HashSet<TrafficSignalLight> TrafficSignalLights { get; } = new ();
		public HashSet<TrafficSignalControllerPattern> TrafficSignalControllerPatterns { get; } = new ();
		public HashSet<TrafficSignalControllerPhase> TrafficSignalControllerPhases { get; } = new ();
		public HashSet<RnPoint> RnPoints { get; } = new ();
		public HashSet<RnLineString> RnLineStrings { get; } = new ();
		public HashSet<RnLane> RnLanes { get; } = new ();
		public HashSet<RnRoadBase> RnRoadBases { get; } = new ();
		public HashSet<RnWay> RnWays { get; } = new ();
		public HashSet<RnSideWalk> RnSideWalks { get; } = new ();
		public bool TryAdd(TrafficSignalLightController x) { return TrafficSignalLightControllers.Add(x); }
		public bool TryAdd(TrafficSignalLight x) { return TrafficSignalLights.Add(x); }
		public bool TryAdd(TrafficSignalControllerPattern x) { return TrafficSignalControllerPatterns.Add(x); }
		public bool TryAdd(TrafficSignalControllerPhase x) { return TrafficSignalControllerPhases.Add(x); }
		public bool TryAdd(RnPoint x) { return RnPoints.Add(x); }
		public bool TryAdd(RnLineString x) { return RnLineStrings.Add(x); }
		public bool TryAdd(RnLane x) { return RnLanes.Add(x); }
		public bool TryAdd(RnRoadBase x) { return RnRoadBases.Add(x); }
		public bool TryAdd(RnWay x) { return RnWays.Add(x); }
		public bool TryAdd(RnSideWalk x) { return RnSideWalks.Add(x); }
	}
}
namespace PLATEAU.RoadNetwork.Structure{
	partial class RnIntersection{
		// データシリアライズ用. メンバ変数から対象オブジェクトを抽出する
		internal override bool Collect(CollectRnModelWork collectWork){
			if(base.Collect(collectWork) == false) return false;
			var edgesTmp = edges;
			foreach(var v in edgesTmp ?? new ()){
				if(v != null){
					v.Collect(collectWork);
				}
			}
			var SignalControllerTmp = SignalController;
			if(SignalControllerTmp != null){
				if(collectWork.TryAdd(SignalControllerTmp)){
					SignalControllerTmp.Collect(collectWork);
				}
			}
			var tracksTmp = tracks;
			foreach(var v in tracksTmp ?? new ()){
				if(v != null){
					v.Collect(collectWork);
				}
			}
			return true;
		}
	}
	partial class RnLane{
		// データシリアライズ用. メンバ変数から対象オブジェクトを抽出する
		internal virtual  bool Collect(CollectRnModelWork collectWork){
			if(collectWork.Visited.Add(this)== false) return false;
			var ParentTmp = Parent;
			if(ParentTmp != null){
				if(collectWork.TryAdd(ParentTmp)){
					ParentTmp.Collect(collectWork);
				}
			}
			var PrevBorderTmp = PrevBorder;
			if(PrevBorderTmp != null){
				if(collectWork.TryAdd(PrevBorderTmp)){
					PrevBorderTmp.Collect(collectWork);
				}
			}
			var NextBorderTmp = NextBorder;
			if(NextBorderTmp != null){
				if(collectWork.TryAdd(NextBorderTmp)){
					NextBorderTmp.Collect(collectWork);
				}
			}
			var LeftWayTmp = LeftWay;
			if(LeftWayTmp != null){
				if(collectWork.TryAdd(LeftWayTmp)){
					LeftWayTmp.Collect(collectWork);
				}
			}
			var RightWayTmp = RightWay;
			if(RightWayTmp != null){
				if(collectWork.TryAdd(RightWayTmp)){
					RightWayTmp.Collect(collectWork);
				}
			}
			var centerWayTmp = centerWay;
			if(centerWayTmp != null){
				if(collectWork.TryAdd(centerWayTmp)){
					centerWayTmp.Collect(collectWork);
				}
			}
			return true;
		}
	}
	partial class RnLineString{
		// データシリアライズ用. メンバ変数から対象オブジェクトを抽出する
		internal virtual  bool Collect(CollectRnModelWork collectWork){
			if(collectWork.Visited.Add(this)== false) return false;
			var PointsTmp = Points;
			foreach(var v in PointsTmp ?? new ()){
				if(v != null){
					if(collectWork.TryAdd(v)){
					}
				}
			}
			return true;
		}
	}
	partial class RnModel{
		// データシリアライズ用. メンバ変数から対象オブジェクトを抽出する
		internal virtual  bool Collect(CollectRnModelWork collectWork){
			if(collectWork.Visited.Add(this)== false) return false;
			var roadsTmp = roads;
			foreach(var v in roadsTmp ?? new ()){
				if(v != null){
					if(collectWork.TryAdd(v)){
						v.Collect(collectWork);
					}
				}
			}
			var intersectionsTmp = intersections;
			foreach(var v in intersectionsTmp ?? new ()){
				if(v != null){
					if(collectWork.TryAdd(v)){
						v.Collect(collectWork);
					}
				}
			}
			var sideWalksTmp = sideWalks;
			foreach(var v in sideWalksTmp ?? new ()){
				if(v != null){
					if(collectWork.TryAdd(v)){
						v.Collect(collectWork);
					}
				}
			}
			return true;
		}
	}
	partial class RnIntersectionEdge{
		// データシリアライズ用. メンバ変数から対象オブジェクトを抽出する
		internal virtual  bool Collect(CollectRnModelWork collectWork){
			if(collectWork.Visited.Add(this)== false) return false;
			var BorderTmp = Border;
			if(BorderTmp != null){
				if(collectWork.TryAdd(BorderTmp)){
					BorderTmp.Collect(collectWork);
				}
			}
			var RoadTmp = Road;
			if(RoadTmp != null){
				if(collectWork.TryAdd(RoadTmp)){
					RoadTmp.Collect(collectWork);
				}
			}
			return true;
		}
	}
	partial class RnRoad{
		// データシリアライズ用. メンバ変数から対象オブジェクトを抽出する
		internal override bool Collect(CollectRnModelWork collectWork){
			if(base.Collect(collectWork) == false) return false;
			var NextTmp = Next;
			if(NextTmp != null){
				if(collectWork.TryAdd(NextTmp)){
					NextTmp.Collect(collectWork);
				}
			}
			var PrevTmp = Prev;
			if(PrevTmp != null){
				if(collectWork.TryAdd(PrevTmp)){
					PrevTmp.Collect(collectWork);
				}
			}
			var mainLanesTmp = mainLanes;
			foreach(var v in mainLanesTmp ?? new ()){
				if(v != null){
					if(collectWork.TryAdd(v)){
						v.Collect(collectWork);
					}
				}
			}
			var medianLaneTmp = medianLane;
			if(medianLaneTmp != null){
				if(collectWork.TryAdd(medianLaneTmp)){
					medianLaneTmp.Collect(collectWork);
				}
			}
			return true;
		}
	}
	partial class RnRoadBase{
		// データシリアライズ用. メンバ変数から対象オブジェクトを抽出する
		internal virtual  bool Collect(CollectRnModelWork collectWork){
			if(collectWork.Visited.Add(this)== false) return false;
			var ParentModelTmp = ParentModel;
			if(ParentModelTmp != null){
				ParentModelTmp.Collect(collectWork);
			}
			var sideWalksTmp = sideWalks;
			foreach(var v in sideWalksTmp ?? new ()){
				if(v != null){
					if(collectWork.TryAdd(v)){
						v.Collect(collectWork);
					}
				}
			}
			return true;
		}
	}
	partial class RnSideWalk{
		// データシリアライズ用. メンバ変数から対象オブジェクトを抽出する
		internal virtual  bool Collect(CollectRnModelWork collectWork){
			if(collectWork.Visited.Add(this)== false) return false;
			var parentRoadTmp = parentRoad;
			if(parentRoadTmp != null){
				if(collectWork.TryAdd(parentRoadTmp)){
					parentRoadTmp.Collect(collectWork);
				}
			}
			var outsideWayTmp = outsideWay;
			if(outsideWayTmp != null){
				if(collectWork.TryAdd(outsideWayTmp)){
					outsideWayTmp.Collect(collectWork);
				}
			}
			var insideWayTmp = insideWay;
			if(insideWayTmp != null){
				if(collectWork.TryAdd(insideWayTmp)){
					insideWayTmp.Collect(collectWork);
				}
			}
			var startEdgeWayTmp = startEdgeWay;
			if(startEdgeWayTmp != null){
				if(collectWork.TryAdd(startEdgeWayTmp)){
					startEdgeWayTmp.Collect(collectWork);
				}
			}
			var endEdgeWayTmp = endEdgeWay;
			if(endEdgeWayTmp != null){
				if(collectWork.TryAdd(endEdgeWayTmp)){
					endEdgeWayTmp.Collect(collectWork);
				}
			}
			return true;
		}
	}
	partial class RnTrack{
		// データシリアライズ用. メンバ変数から対象オブジェクトを抽出する
		internal virtual  bool Collect(CollectRnModelWork collectWork){
			if(collectWork.Visited.Add(this)== false) return false;
			var FromBorderTmp = FromBorder;
			if(FromBorderTmp != null){
				if(collectWork.TryAdd(FromBorderTmp)){
					FromBorderTmp.Collect(collectWork);
				}
			}
			var ToBorderTmp = ToBorder;
			if(ToBorderTmp != null){
				if(collectWork.TryAdd(ToBorderTmp)){
					ToBorderTmp.Collect(collectWork);
				}
			}
			return true;
		}
	}
	partial class RnWay{
		// データシリアライズ用. メンバ変数から対象オブジェクトを抽出する
		internal virtual  bool Collect(CollectRnModelWork collectWork){
			if(collectWork.Visited.Add(this)== false) return false;
			var LineStringTmp = LineString;
			if(LineStringTmp != null){
				if(collectWork.TryAdd(LineStringTmp)){
					LineStringTmp.Collect(collectWork);
				}
			}
			return true;
		}
	}
	partial class TrafficSignalControllerPattern{
		// データシリアライズ用. メンバ変数から対象オブジェクトを抽出する
		internal virtual  bool Collect(CollectRnModelWork collectWork){
			if(collectWork.Visited.Add(this)== false) return false;
			var ParentTmp = Parent;
			if(ParentTmp != null){
				if(collectWork.TryAdd(ParentTmp)){
					ParentTmp.Collect(collectWork);
				}
			}
			var PhasesTmp = Phases;
			foreach(var v in PhasesTmp ?? new ()){
				if(v != null){
					if(collectWork.TryAdd(v)){
						v.Collect(collectWork);
					}
				}
			}
			var OffsetTrafficLightTmp = OffsetTrafficLight;
			if(OffsetTrafficLightTmp != null){
				if(collectWork.TryAdd(OffsetTrafficLightTmp)){
					OffsetTrafficLightTmp.Collect(collectWork);
				}
			}
			return true;
		}
	}
	partial class TrafficSignalControllerPhase{
		// データシリアライズ用. メンバ変数から対象オブジェクトを抽出する
		internal virtual  bool Collect(CollectRnModelWork collectWork){
			if(collectWork.Visited.Add(this)== false) return false;
			var ParentTmp = Parent;
			if(ParentTmp != null){
				if(collectWork.TryAdd(ParentTmp)){
					ParentTmp.Collect(collectWork);
				}
			}
			var BlueRoadPairsTmp = BlueRoadPairs;
			foreach(var v in BlueRoadPairsTmp ?? new ()){
				if(v != null){
					if(collectWork.TryAdd(v)){
						v.Collect(collectWork);
					}
				}
			}
			var YellowRoadPairsTmp = YellowRoadPairs;
			foreach(var v in YellowRoadPairsTmp ?? new ()){
				if(v != null){
					if(collectWork.TryAdd(v)){
						v.Collect(collectWork);
					}
				}
			}
			var RedRoadPairsTmp = RedRoadPairs;
			foreach(var v in RedRoadPairsTmp ?? new ()){
				if(v != null){
					if(collectWork.TryAdd(v)){
						v.Collect(collectWork);
					}
				}
			}
			return true;
		}
	}
	partial class TrafficSignalLight{
		// データシリアライズ用. メンバ変数から対象オブジェクトを抽出する
		internal virtual  bool Collect(CollectRnModelWork collectWork){
			if(collectWork.Visited.Add(this)== false) return false;
			var ParentTmp = Parent;
			if(ParentTmp != null){
				if(collectWork.TryAdd(ParentTmp)){
					ParentTmp.Collect(collectWork);
				}
			}
			var RoadTmp = Road;
			if(RoadTmp != null){
				if(collectWork.TryAdd(RoadTmp)){
					RoadTmp.Collect(collectWork);
				}
			}
			var NeighborTmp = Neighbor;
			foreach(var v in NeighborTmp ?? new ()){
				if(v != null){
					if(collectWork.TryAdd(v)){
						v.Collect(collectWork);
					}
				}
			}
			return true;
		}
	}
	partial class TrafficSignalLightController{
		// データシリアライズ用. メンバ変数から対象オブジェクトを抽出する
		internal virtual  bool Collect(CollectRnModelWork collectWork){
			if(collectWork.Visited.Add(this)== false) return false;
			var ParentTmp = Parent;
			if(ParentTmp != null){
				if(collectWork.TryAdd(ParentTmp)){
					ParentTmp.Collect(collectWork);
				}
			}
			var TrafficLightsTmp = TrafficLights;
			foreach(var v in TrafficLightsTmp ?? new ()){
				if(v != null){
					if(collectWork.TryAdd(v)){
						v.Collect(collectWork);
					}
				}
			}
			var SignalPatternsTmp = SignalPatterns;
			foreach(var v in SignalPatternsTmp ?? new ()){
				if(v != null){
					if(collectWork.TryAdd(v)){
						v.Collect(collectWork);
					}
				}
			}
			return true;
		}
	}
}
