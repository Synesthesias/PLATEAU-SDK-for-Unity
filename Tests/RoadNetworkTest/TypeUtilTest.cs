using NUnit.Framework;
using PLATEAU.RoadNetwork;
using PLATEAU.Util;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.TestTools;

namespace PLATEAU.Tests.TestUtils
{
    public class TypeUtilTest
    {
        // A Test behaves as an ordinary method
        [Test()]
        public void GetAllMemberRecursivelyTest()
        {
            var vertices = Enumerable.Repeat(Vector3.one, 5).ToList();
            var lineString = RoadNetworkLineString.Create(vertices);
            var obj = new RoadNetworkWay(lineString);
            {
                var ret = TypeUtil.GetAllMembersRecursively<RoadNetworkLineString>(obj).ToList();
                Assert.IsTrue(lineString == ret.FirstOrDefault()?.Item2, "lineString == ret.FirstOrDefault()?.Item2");
            }
            {
                var ret = TypeUtil.GetAllMembersRecursively<Vector3>(obj).ToList();
                Assert.IsTrue(lineString.Vertices[0] == ret.FirstOrDefault()?.Item2, "lineString == ret.FirstOrDefault()?.Item2");
            }
        }
        [Test()]
        public void GenericListMemberTest()
        {
            var a = new List<int>();
            var type = a.GetType();
            var args = type.GenericTypeArguments;
            var genType = type.GetGenericTypeDefinition();

            Assert.IsTrue(genType == typeof(List<>), "genType == typeof(List<>)");
        }
        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest]
        public IEnumerator TypeUtilTestWithEnumeratorPasses()
        {
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return null;
        }
    }
}
