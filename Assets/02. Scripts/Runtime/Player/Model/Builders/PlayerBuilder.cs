using MikroFramework.Pool;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Entities.Creatures;
using Runtime.DataFramework.Properties;
using Runtime.Enemies.Model.Builders;

namespace Runtime.Player.Builders {
	public class PlayerBuilder<T> : CreatureBuilder<PlayerBuilder<T>, T> where T : class, IEntity, new() {
		public PlayerBuilder() {
			CheckEntity();
		}
		public override void RecycleToCache() {
			SafeObjectPool<PlayerBuilder<T>>.Singleton.Recycle(this);
		}
		
		public static PlayerBuilder<T> Allocate(int rarity) {
			PlayerBuilder<T> target = SafeObjectPool<PlayerBuilder<T>>.Singleton.Allocate();
			target.SetProperty<int>(new PropertyNameInfo(PropertyName.rarity), rarity);
			return target;
		}
	}
}