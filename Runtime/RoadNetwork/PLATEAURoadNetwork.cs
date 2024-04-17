using PLATEAU.CityInfo;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace PLATEAU.Packages.PlateauUnitySDK.Runtime.RoadNetwork
{
    public class PLATEAURoadNetwork
    {

    }

    public class RoadModelNetwork
    {

    }

    [Serializable]
    public class Lane
    {
        [SerializeField] public PLATEAUCityObjectGroup CityObjectGroup;

        // 構成する頂点
        // ポリゴン的に時計回りの順に格納されている
        [SerializeField]
        public List<Vector3> Vertices = new List<Vector3>();

        // 連結しているレーン
        [NonSerialized]
        public List<Lane> Connected = new List<Lane>();

        public List<Way> Ways = new List<Way>();

        public List<Edge> Edges = new List<Edge>();

        // #TODO : 左側/右側の判断ができるのか
        // 左側Way
        public Way Left { get; set; }
        // 右側Way
        public Way Right { get; set; }

        // #TODO : 連結先/連結元の判断ができるのか？


        // 連結先レーン
        public List<Lane> NextLanes { get; } = new List<Lane>();

        // 連結元レーン
        public List<Lane> PrevLanes { get; } = new List<Lane>();

        public Vector3 GetDrawCenterPoint()
        {
            return Vertices[0];
        }
    }

    public class Edge
    {

        public List<Vector3> Vertices { get; } = new List<Vector3>();
        public Lane NeighborLane { get; set; }
    }

    /// <summary>
    /// レーンを構成する１車線を表す
    /// </summary>
    public class Way
    {
        public List<Vector3> Vertices { get; } = new List<Vector3>();

        public List<Lane> NextLanes { get; } = new List<Lane>();

        public List<Lane> PrevLanes { get; } = new List<Lane>();
    }

}