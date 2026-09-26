using NUnit.Framework;
using PLATEAU.CityAdjust.NonLibData;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace PLATEAU.Tests.EditModeTests
{
    [TestFixture]
    public class TestNonLibDictionary
    {
        private readonly List<GameObject> createdObjects = new();

        [TearDown]
        public void TearDown()
        {
            foreach (var obj in createdObjects)
            {
                if (obj != null) Object.DestroyImmediate(obj);
            }
            createdObjects.Clear();
        }

        [Test]
        public void Restore_WithPrefix_MatchesRootAndNestedObjects()
        {
            var srcRoot = CreateObject("Road");
            var srcChild = CreateObject("Surface", srcRoot.transform);
            var value = new TestValue();
            var dictionary = new NonLibDictionary<TestValue>("ALIGNED");
            dictionary.Add(srcChild.transform, new[] { srcRoot.transform }, value);

            var dstRoot = CreateObject("ALIGNED_Road");
            var dstChild = CreateObject("ALIGNED_Surface", dstRoot.transform);

            var restored = dictionary.GetNonRestoredAndMarkRestored(
                dstChild.transform, new[] { dstRoot.transform });

            Assert.That(restored, Is.SameAs(value));
            Assert.That(dictionary.RemainingNonRestored, Is.Zero);
        }

        [Test]
        public void Restore_WithPrefix_RemovesOnlyOnePrefixForRepeatedConversion()
        {
            var src = CreateObject("ALIGNED_Road");
            var value = new TestValue();
            var dictionary = new NonLibDictionary<TestValue>("ALIGNED");
            dictionary.Add(src.transform, new[] { src.transform }, value);

            var dst = CreateObject("ALIGNED_ALIGNED_Road");

            var restored = dictionary.GetNonRestoredAndMarkRestored(
                dst.transform, new[] { dst.transform });

            Assert.That(restored, Is.SameAs(value));
        }

        [Test]
        public void Restore_WithoutPrefixConfiguration_KeepsExactMatchingBehavior()
        {
            var src = CreateObject("Road");
            var value = new TestValue();
            var dictionary = new NonLibDictionary<TestValue>();
            dictionary.Add(src.transform, new[] { src.transform }, value);

            var dst = CreateObject("ALIGNED_Road");

            var restored = dictionary.GetNonRestoredAndMarkRestored(
                dst.transform, new[] { dst.transform });

            Assert.That(restored, Is.Null);
            Assert.That(dictionary.RemainingNonRestored, Is.EqualTo(1));
        }

        [Test]
        public void Restore_WithPrefix_DoesNotMatchSimilarPrefix()
        {
            var src = CreateObject("Road");
            var value = new TestValue();
            var dictionary = new NonLibDictionary<TestValue>("ALIGNED");
            dictionary.Add(src.transform, new[] { src.transform }, value);

            var dst = CreateObject("ALIGNED2_Road");

            var restored = dictionary.GetNonRestoredAndMarkRestored(
                dst.transform, new[] { dst.transform });

            Assert.That(restored, Is.Null);
        }

        private GameObject CreateObject(string name, Transform parent = null)
        {
            var obj = new GameObject(name);
            obj.transform.parent = parent;
            createdObjects.Add(obj);
            return obj;
        }

        private class TestValue
        {
        }
    }
}
