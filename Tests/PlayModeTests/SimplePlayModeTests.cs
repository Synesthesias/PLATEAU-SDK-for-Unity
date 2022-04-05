using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace PlateauSDK.Tests.PlayModeTests
{
    public class SimplePlayModeTests
    {
        // A Test behaves as an ordinary method
        [Test]
        public void SimplePlayModeTestsSimplePasses()
        {
            Assert.That(10, Is.LessThan(100));
        }

        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest]
        public IEnumerator SimplePlayModeTestsWithEnumeratorPasses()
        {
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return null;
            Assert.That(100, Is.GreaterThan(50));
        }
    }
}
