using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;

using Assert = UnityEngine.Assertions.Assert;

public class TestHelloWorldEditor
{
    // A Test behaves as an ordinary method
    [Test]
    public void TestHelloWorldEditorSimplePasses() {
        Assert.AreEqual<int>(1, 1);
        Assert.AreEqual<int>(1, 1);
    }
    
}
