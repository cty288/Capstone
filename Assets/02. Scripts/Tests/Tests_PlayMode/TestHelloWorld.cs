using System.Collections;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace Tests.Tests_PlayMode {
    public class TestHelloWorld {
        [UnityTest]
        public IEnumerator TestHelloWorldWithEnumeratorPasses() {
            yield return null;
            Assert.Pass();
        
        }
    }
}
