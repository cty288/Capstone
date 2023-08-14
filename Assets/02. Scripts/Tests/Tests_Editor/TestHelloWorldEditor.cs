using NUnit.Framework;
using Assert = UnityEngine.Assertions.Assert;

namespace Tests.Tests_Editor {
    public class TestHelloWorldEditor
    {
        // A Test behaves as an ordinary method
        [Test]
        public void TestHelloWorldEditorSimplePasses() {
            Assert.AreEqual<int>(1, 1);
            Assert.AreEqual<int>(1, 1);
        }
    
    }
}
