using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace PlateauUnitySDK.Tests.EditModeTests {
    public class SimpleEditorTest
    {
        // A Test behaves as an ordinary method
        [Test]
        public void SimpleTestSimplePasses() {
            Assert.That("This is a test example", Does.EndWith("test example"));
        }

        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        // [UnityTest]
        // public IEnumerator SimpleTestWithEnumeratorPasses()
        // {
        //     // Use the Assert class to test conditions.
        //     // Use yield to skip a frame.
        //     yield return null;
        // }
    }
}
