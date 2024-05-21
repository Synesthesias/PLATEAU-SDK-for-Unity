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
            var lineString = RoadNetworkLineString.Create(vertices);
            var obj = new RoadNetworkWay(lineString);
            {
                var ret = TypeUtil.GetAllMembersRecursively<RoadNetworkLineString>(obj).ToList();
                Assert.IsTrue(lineString == ret.FirstOrDefault()?.Item2, "lineString == ret.FirstOrDefault()?.Item2");
            }
            {
                var ret = TypeUtil.GetAllMembersRecursively<Vector3>(obj).ToList();
                Assert.IsTrue(lineString[0] == ret.FirstOrDefault()?.Item2, "lineString == ret.FirstOrDefault()?.Item2");
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

        [Test()]
        public void SerializeFieldTest()
        {
            var a = new List<int>();
            var type = typeof(RoadNetworkDataLink);
            var flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            var properties = type.GetProperties(flags);
            var fields = type.GetFields(flags);
            var propAttributes = properties.Select(p => p.CustomAttributes.ToList()).ToList();
            var fieldAttributes = fields.Select(p => p.CustomAttributes.ToList()).ToList();

        }

        [Test()]
        public void ObjectKeyTableTest()
        {
            var table = new Dictionary<object, object>();

            table[1] = 10;

            object key = 1;
            Console.WriteLine(table[key]);
        }

        [Test()]
        public void GetPropertyFieldInfoTest()
        {
            var props = typeof(RoadNetworkLink).GetProperty(nameof(RoadNetworkLink.MainLanes));
            var getter = props.GetGetMethod(true);
            var memberInfo = (System.Reflection.MemberInfo)getter;
            var fieldInfo = memberInfo as FieldInfo;

            Console.WriteLine(fieldInfo.DeclaringType);
        }
    }
}
