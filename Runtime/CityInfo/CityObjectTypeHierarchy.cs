using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
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
        /// ここでいう階層構造やノードとは、都市オブジェクトの分類についての上位概念・下位概念を示すものであり、実際の都市Modelデータとは異なることに注意してください。
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
                    new Node("屋根の通行可能部分 (OuterFloorSurface)", Package.None, new[] { COType.COT_OuterFloorSurface }, null),
                    new Node("部屋(Room)", Package.None, new[]{COType.COT_Room}, null),
                    new Node("天井(CeilingSurface)", Package.None, new[]{COType.COT_CeilingSurface}, null),
                    new Node("床(FloorSurface)", Package.None, new[]{COType.COT_FloorSurface}, null),
                    new Node("内装壁(InteriorWallSurface)", Package.None, new[]{COType.COT_InteriorWallSurface}, null),
                    new Node("内装付属設備(IntBuildingInstallation)", Package.None, new[]{COType.COT_IntBuildingInstallation}, null),
                    
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
                new Node("植生 (Vegetation)", Package.Vegetation, new[]{COType.COT_SolitaryVegetationObject, COType.COT_PlantCover}, null),
                new Node("災害リスク (DisasterRisk)", Package.DisasterRisk, null, null),
                new Node("鉄道 (Railway)", Package.Railway, new[]{COType.COT_Railway}, null),
                new Node("航路 (Waterway)", Package.Waterway, null, null),
                new Node("水部 (WaterBody)", Package.WaterBody, new[]{COType.COT_WaterBody}, null),
                new Node("橋梁 (Bridge)", Package.Bridge, new[]{COType.COT_Bridge}, null),
                new Node("徒歩道 (Track)", Package.Track, new[]{COType.COT_Track}, null),
                new Node("広場 (Square)", Package.Square, new[]{COType.COT_Square}, null),
                new Node("トンネル (Tunnel)", Package.Tunnel, new[]{COType.COT_Tunnel}, null),
                new Node("地下埋設物 (UndergroundFacility)", Package.UndergroundFacility, null, null),
                new Node("地下街 (UndergroundBuilding)", Package.UndergroundBuilding, null, null),
                new Node("区域 (Area)", Package.Area, null, null),
                new Node("その他の構造物 (OtherConstruction)", Package.OtherConstruction, null, null),
                new Node("汎用都市 (Generic)", Package.Generic, new[]{COType.COT_GenericCityObject}, null),
                new Node("その他 (Unknown)", Package.Unknown, new[]{COType.COT_Unknown}, null)
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
        /// タイプ分類の中で、引数の<see cref="Node"/>は何番目かを計算します。
        /// 順番はヒエラルキーの深さ優先探索の順とします。
        /// </summary>
        private static int CalcIndexOf(Node targetNode)
        {
            Stack<Node> nodeStack = new();
            nodeStack.Push(RootNode);
            int index = 0;
            Node currentNode = RootNode;
            while (nodeStack.Count > 0)
            {
                for(int i=currentNode.Children.Count-1; i>=0; i--) // 子番号の若いほうから検索したいので逆順にpushします。
                {
                    nodeStack.Push(currentNode.Children[i]);
                }

                currentNode = nodeStack.Pop();

                if (currentNode == targetNode) return index;
                index++;
            }
            throw new KeyNotFoundException("targetNode is not found.");
        }
        
        

        /// <summary>
        /// ノードの階層構造を CityObjectType で検索するための辞書です。
        /// </summary>
        private static Dictionary<COType, Node> typeToNode;

        /// <summary>
        /// ノードの階層構造を <see cref="PredefinedCityModelPackage"/> で検索するための辞書です。
        /// </summary>
        private static Dictionary<Package, Node> packageToNode;
        
        public class Node : IComparable<Node>
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
            /// 分類のディスプレイ名を返します。
            /// </summary>
            public string GetDisplayName()
            {
                // 親の分類がある場合、それらをスラッシュで繋ぎます
                Stack<Node> pathStack = new();
                Node current = this;
                // 親を辿ってパスを記録します。
                while(current != null)
                {
                    pathStack.Push(current);
                    current = current.Parent;
                }
                // ルートノードは不要なので除きます
                pathStack.Pop();
                // パスをもとに文字列にします。
                StringBuilder sb = new();
                while (pathStack.Count > 0)
                {
                    var node = pathStack.Pop();
                    sb.Append(node.NodeName);
                    if (pathStack.Count > 0) sb.Append("/");
                }

                return sb.ToString();
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

            /// <summary>
            /// 都市オブジェクトの種類をGUIで列挙するなどの場合に、列挙の順序を定義します。
            /// <see cref="CityObjectTypeHierarchy"/>の深さ優先探索の順番とします。
            /// </summary>
            public int CompareTo(Node other)
            {
                int thisIndex = CalcIndexOf(this);
                int otherIndex = CalcIndexOf(other);
                int result = thisIndex.CompareTo(otherIndex);
                return result;
            }
        }
    }
}
    

