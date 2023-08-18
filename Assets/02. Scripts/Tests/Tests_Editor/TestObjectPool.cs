using MikroFramework.Pool;
using NUnit.Framework;
using Assert = UnityEngine.Assertions.Assert;


namespace Tests.Tests_Editor {

	public class MyClass : IPoolable {
		public void OnRecycled() {
			number = 0;
		}

		public bool IsRecycled { get; set; }

		public int number;
		
		public void RecycleToCache() {
			SafeObjectPool<MyClass>.Singleton.Recycle(this);
		}

		public static MyClass Allocate(int number) {
			MyClass c = SafeObjectPool<MyClass>.Singleton.Allocate();
			c.number = number;
			return c;
		}
	}
	
	
	
	public class TestObjectPool {
		[Test]
		public void TestObjectPoolBasic() {
			//optional: initialization
			SafeObjectPool<MyClass>.Singleton.Init(10,100);
			
			//allocate
			MyClass myClass = MyClass.Allocate(100);
			
			//tests
			Assert.IsNotNull(myClass);
			Assert.AreEqual(100, myClass.number);
			
			//recycle
			SafeObjectPool<MyClass>.Singleton.Recycle(myClass); //or myClass.RecycleToCache();
			
			//tests
			Assert.IsTrue(myClass.IsRecycled);
			Assert.AreEqual(0, myClass.number);
		}
	}
}