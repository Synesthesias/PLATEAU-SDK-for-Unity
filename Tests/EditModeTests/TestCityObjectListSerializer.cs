using MessagePack;
using NUnit.Framework;
using PLATEAU.CityGML;
using PLATEAU.CityInfo;
using System;
using System.Collections.Generic;
using UnityEditor.Compilation;
using static PLATEAU.CityInfo.CityObjectList;

namespace PLATEAU.Tests.EditModeTests
{
    /// <summary>
    /// <see cref="CityObjectList"/>がシリアライズ・デシリアライズできることをテストします。
    /// </summary>
    [TestFixture]
    public class TestCityObjectListSerializer
    {
        private CityObjectList CreateTestCityObjectList()
        {
            // テスト用のCityObjectListを作成します。
            var cityObjs = new CityObjectList
            {
                outsideChildren = new List<string> { "testOutsideChildren" },
                outsideParent = "testOutsideParent",
                rootCityObjects =
                {
                    new CityObjectList.CityObject()
                    {
                        GmlID = "testGmlID",
                        CityObjectIndex = new int[]{0, -1},
                        AttributesMap = new Attributes(new Dictionary<string, Attributes.Value>
                        {
                            {"testAttributeKey", new Attributes.Value(AttributeType.Integer, 1)}
                        }),
                        Children = { new CityObjectList.CityObject()
                        {
                            GmlID = "testChildGmlID",
                            CityObjectIndex = new int[]{1, 2},
                            AttributesMap = new Attributes(new Dictionary<string, Attributes.Value>
                            {
                                {"testChildAttributeKey", new Attributes.Value(AttributeType.String, "testChildAttributeValue")}
                            })
                        } }
                    }
                }
            };
            return cityObjs;
        }
        
        [Test]
        public void CityObjectList_Can_SerializeAndDeserialize()
        {
            var cityObjs = CreateTestCityObjectList();

            // MessagePack形式にシリアライズします。
            var messagePack = MessagePackSerializer.Serialize(cityObjs);
            Assert.True(messagePack.Length > 0, "MessagePack serialization failed.");
            
            // デシリアライズします。
            var deserialized = MessagePackSerializer.Deserialize<CityObjectList>(messagePack);
            
            // デシリアライズ結果をチェックします。
            Assert.AreEqual(1, deserialized.outsideChildren.Count);
            Assert.AreEqual("testOutsideChildren", deserialized.outsideChildren[0]);
            Assert.AreEqual("testOutsideParent", deserialized.outsideParent);
            Assert.AreEqual(1, deserialized.rootCityObjects.Count);
            var cityObj = deserialized.rootCityObjects[0];
            Assert.AreEqual("testGmlID", cityObj.GmlID);
            Assert.AreEqual(1, cityObj.AttributesMap.Count);
            Assert.AreEqual(AttributeType.Integer, cityObj.AttributesMap["testAttributeKey"].Type);
            Assert.AreEqual(1, cityObj.AttributesMap["testAttributeKey"].IntValue);
            Assert.AreEqual(0, cityObj.CityObjectIndex[0]);
            Assert.AreEqual(-1, cityObj.CityObjectIndex[1]);
            Assert.AreEqual(1, cityObj.Children.Count);
            var childObj = cityObj.Children[0];
            Assert.AreEqual(childObj.GmlID, "testChildGmlID");
            Assert.AreEqual(1, childObj.CityObjectIndex[0]);
            Assert.AreEqual(2, childObj.CityObjectIndex[1]);
            Assert.AreEqual(1, childObj.AttributesMap.Count);
            Assert.AreEqual(AttributeType.String, childObj.AttributesMap["testChildAttributeKey"].Type);
            Assert.AreEqual("testChildAttributeValue", childObj.AttributesMap["testChildAttributeKey"].StringValue);
            Assert.AreEqual(0, childObj.Children.Count);
        }
    }
}