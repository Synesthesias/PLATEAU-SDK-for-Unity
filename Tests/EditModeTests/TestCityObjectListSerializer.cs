using MessagePack;
using NUnit.Framework;
using PLATEAU.CityGML;
using PLATEAU.CityInfo;
using System;
using System.Collections.Generic;
using UnityEditor.Compilation;
using static PLATEAU.CityInfo.SerializableCityObjectList;

namespace PLATEAU.Tests.EditModeTests
{
    /// <summary>
    /// <see cref="SerializableCityObjectList"/>がシリアライズ・デシリアライズできることをテストします。
    /// </summary>
    [TestFixture]
    public class TestCityObjectListSerializer
    {
        [Test]
        public void CityObjectList_Can_SerializeAndDeserialize()
        {
            // テスト用のCityObjectListを作成します。
            var cityObjs = new SerializableCityObjectList
            {
                outsideChildren = new List<string> { "testOutsideChildren" },
                outsideParent = "testOutsideParent",
                rootCityObjects =
                {
                    new SerializableCityObject()
                    {
                        GmlID = "testGmlID",
                        CityObjectIndex = new int[]{0, -1},
                        AttributesMap = new SerializableAttributes(new Dictionary<string, SerializableAttributes.SerializableValue>
                        {
                            {"testAttributeKey", new SerializableAttributes.SerializableValue(AttributeType.Integer, 1)}
                        }),
                        Children = { new SerializableCityObject()
                        {
                            GmlID = "testChildGmlID",
                            CityObjectIndex = new int[]{1, 2},
                            AttributesMap = new SerializableAttributes(new Dictionary<string, SerializableAttributes.SerializableValue>
                            {
                                {"testChildAttributeKey", new SerializableAttributes.SerializableValue(AttributeType.String, "testChildAttributeValue")}
                            })
                        } }
                    }
                }
            };

            // MessagePack形式にシリアライズします。
            var messagePack = MessagePackSerializer.Serialize(cityObjs);
            Assert.True(messagePack.Length > 0, "MessagePack serialization failed.");
            
            // デシリアライズします。
            var deserialized = MessagePackSerializer.Deserialize<SerializableCityObjectList>(messagePack);
            
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