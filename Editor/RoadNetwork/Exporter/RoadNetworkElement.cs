using PLATEAU.CityInfo;
using PLATEAU.Geometries;
using PLATEAU.Native;
using PLATEAU.RoadNetwork;
using PLATEAU.RoadNetwork.Data;
using PLATEAU.RoadNetwork.Structure;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PLATEAU.Editor.RoadNetwork.Exporter
{
    /// <summary>
    /// 道路ネットワークのベースクラス
    /// </summary>
    public class RoadNetworkElement
    {
        public string ID { get; set; }

        protected RoadNetworkContext roadNetworkContext;

        public RoadNetworkElement(RoadNetworkContext context, string id)
        {
            ID = id;

            roadNetworkContext = context;
        }

        public virtual GeoJSON.Net.Geometry.IGeometryObject GetGeometory()
        {
            return null;
        }
    }
}