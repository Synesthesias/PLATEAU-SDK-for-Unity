using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using PLATEAU.CityGML;
using PLATEAU.Dataset;
using Package = PLATEAU.Dataset.PredefinedCityModelPackage;
using COType = PLATEAU.CityGML.CityObjectType;

namespace PLATEAU.CityInfo
{
    public static class CityObjectTypeHierarchy
    {
        /// <summary>
        /// 都市オブジェクトの分類を階層構造のノードで表現したものです。
        /// </summary>
        // 前提 :
        // ・ヒエラルキーの中で 各CityObjectType が登場するのは1回以下で、ダブりはありません。
        // ・PredefinedCityModelPackageも同様にダブりはありません。
        // ・入れ子構造の2段目(ルートの子) は PredefinedCityModelPackage による分類が主であり、CityObjectTypeの指定はオプションです。
        // ・入れ子構造の3段目 は CityObjectType による分類です。
        public static Node RootNode { get; } =
            new Node("ルート", Package.None, null, new []
                {
                new Node("建築物 (Building)", Package.Building, new[] { CityObjectType.COT_Building }, new[]
                {
                    new Node("建築物付属設備 (BuildingInstallation)", Package.None, new[] { COType.COT_BuildingInstallation }, null),
                    new Node("ドア (Door)", Package.None, new[] { COType.COT_Door }, null),
                    new Node("窓 (Window)", Package.None, new[] { COType.COT_Window }, null),
                    new Node("建築物パーツ (BuildingPart)", Package.None, new[] { COType.COT_BuildingPart }, null),
                    new Node("壁面 (WallSurface)", Package.None, new[] { COType.COT_WallSurface }, null),
                    new Node("屋根面 (RoofSurface)", Package.None, new[] { COType.COT_RoofSurface }, null),
                    new Node("接地面 (GroundSurface)", Package.None, new[] { COType.COT_GroundSurface }, null),
                    new Node("開口部 (ClosureSurface)", Package.None, new[] { COType.COT_ClosureSurface }, null),
                    new Node("外側の天井 (OuterCeilingSurface)", Package.None, new[] { COType.COT_OuterCeilingSurface }, null),
                    new Node("屋根の通行可能部分 (OuterFloorSurface)", Package.None, new[] { COType.COT_OuterFloorSurface }, null)
                }),
                new Node("道路 (Road)", Package.Road, new[] { COType.COT_Road }, null),
                new Node("都市設備 (CityFurniture)", Package.CityFurniture, new[] { COType.COT_CityFurniture }, null),
                new Node("都市計画決定情報 (UrbanPlanningDecision)", Package.UrbanPlanningDecision, null, null),
                new Node("土地利用 (LandUse)", Package.LandUse, new[] { COType.COT_LandUse }, null),
                new Node("土地起伏 (Relief)", Package.Relief, new[] { COType.COT_ReliefFeature, COType.COT_ReliefComponent }, new[]
                {
                    new Node("ポリゴンによる起伏表現 (TINRelief)", Package.None, new[] { COType.COT_TINRelief }, null),
                    new Node("点群による起伏表現 (MassPointRelief)", Package.None, new[] { COType.COT_MassPointRelief }, null)
                }),
                new Node("植生 (Vegetation)", Package.Vegetation, null, null),
                new Node("災害リスク (DisasterRisk)", Package.DisasterRisk, null, null),
                new Node("鉄道 (Railway)", Package.Railway, null, null),
                new Node("航路 (Waterway)", Package.Waterway, null, null),
                new Node("水部 (WaterBody)", Package.WaterBody, null, null),
                new Node("橋梁 (Bridge)", Package.Bridge, null, null),
                new Node("徒歩道 (Track)", Package.Track, null, null),
                new Node("広場 (Square)", Package.Square, null, null),
                new Node("トンネル (Tunnel)", Package.Tunnel, null, null),
                new Node("地下埋設物 (UndergroundFacility)", Package.UndergroundFacility, null, null),
                new Node("地下街 (UndergroundBuilding)", Package.UndergroundBuilding, null, null),
                new Node("区域 (Area)", Package.Area, null, null),
                new Node("その他の構造物 (OtherConstruction)", Package.OtherConstruction, null, null),
                new Node("汎用都市 (Generic)", Package.Generic, null, null),
                new Node("その他 (Unknown)", Package.Unknown, null, null)
                }
            );

        public static Node GetNodeByType(COType t)
        {
            if (typeToNode.TryGetValue(t, out var node)) return node;
            return null;
        }

        public static Node GetNodeByPackage(Package p)
        {
            if (packageToNode.TryGetValue(p, out var node)) return node;
            throw new ArgumentOutOfRangeException(nameof(p), $"Package {p} is not found in the hierarchy.");
        }
        
        

        /// <summary>
        /// ノードの階層構造を CityObjectType で検索するための辞書です。
        /// </summary>
        private static Dictionary<COType, Node> typeToNode;

        /// <summary>
        /// ノードの階層構造を <see cref="PredefinedCityModelPackage"/> で検索するための辞書です。
        /// </summary>
        private static Dictionary<Package, Node> packageToNode;
        
        public class Node
        {
            public string NodeName { get; }
            public PredefinedCityModelPackage Package { get; }
            private ReadOnlyCollection<CityObjectType> Types { get; }
            public ReadOnlyCollection<Node> Children { get; }
            public Node Parent { get; private set; }

            public Node(string nodeName, PredefinedCityModelPackage package, CityObjectType[] types, Node[] children)
            {
                NodeName = nodeName;
                Package = package;
                Types = types == null ?
                    new ReadOnlyCollection<CityObjectType>(new CityObjectType[]{}) :
                    new ReadOnlyCollection<CityObjectType>(types);
                Children = children == null ?
                    new ReadOnlyCollection<Node>(new Node[]{}) :
                    new ReadOnlyCollection<Node>(children);
                foreach (var t in Types)
                {
                    typeToNode ??= new Dictionary<CityObjectType, Node>();
                    typeToNode.Add(t, this);
                }

                if (Package != Package.None)
                {
                    packageToNode ??= new Dictionary<PredefinedCityModelPackage, Node>();
                    packageToNode.Add(Package, this);
                }
                foreach (var child in Children)
                {
                    child.Parent = this;
                }
            }

            /// <summary>
            /// Nodeの親子関係を自身から上へ調べて、PackageがNoneでない初めて見つかったPackageを返します。
            /// 調べてもNoneだけならNoneを返します。
            /// </summary>
            public Package UpperPackage
            {
                get
                {
                    var node = this;
                    while (node.Parent != null)
                    {
                        if (node.Package != PredefinedCityModelPackage.None) return node.Package;
                        node = node.Parent;
                    }

                    return PredefinedCityModelPackage.None;
                }
            }
        }
    }
}
    

