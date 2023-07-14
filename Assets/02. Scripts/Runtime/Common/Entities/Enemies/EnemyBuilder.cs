using _02._Scripts.Runtime.Base.Entity;
using MikroFramework.Pool;

namespace _02._Scripts.Runtime.Common.Entities.Enemies {
	public class EnemyBuilder<T> : EntityBuilder<T> where T : class, IEnemyEntity, new() {
		
		
		public override void RecycleToCache() {
			SafeObjectPool<EnemyBuilder<T>>.Singleton.Recycle(this);
		}
		
		public static EnemyBuilder<T> Allocate() {
			return SafeObjectPool<EnemyBuilder<T>>.Singleton.Allocate();
		}
	}
}