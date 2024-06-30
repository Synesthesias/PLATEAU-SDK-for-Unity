using NUnit.Framework;
using PLATEAU.RoadNetwork;
using PLATEAU.RoadNetwork.Data;
using PLATEAU.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Tests.RoadNetworkTest
{
    public class TypeUtilTest
    {
        // A Test behaves as an ordinary method
        [Test()]
        public void GetAllMemberRecursivelyTest()
        {
            var vertices = Enumerable.Repeat(Vector3.one, 5).ToList();
            var lineString = RnLineString.Create(vertices);
            var obj = new RnWay(lineString);
            {
                var ret = TypeUtil.GetAllMembersRecursively<RnLineString>(obj).ToList();
                Assert.IsTrue(lineString == ret.FirstOrDefault()?.Item2, "lineString == ret.FirstOrDefault()?.Item2");
            }
            {
                var ret = TypeUtil.GetAllMembersRecursively<Vector3>(obj).ToList();
                Assert.IsTrue(lineString[0] == ret.FirstOrDefault()?.Item2, "lineString == ret.FirstOrDefault()?.Item2");
            }
        }
    }
}
