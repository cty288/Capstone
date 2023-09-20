using Framework;
using Runtime.DataFramework.Entities;
using Runtime.Enemies.Model;
using Runtime.Enemies.Model.Builders;
using Runtime.Player.Builders;

namespace Runtime.Player {
	
	/// <summary>
	/// Model for enemies
	/// </summary>
	public interface IGamePlayerModel : IEntityModel<IPlayerEntity>, ISavableModel {
		/*/// <summary>
		/// Get the enemy builder for the entity type
		/// </summary>
		/// <param name="rarity"></param>
		/// <param name="addToModelOnceBuilt"></param>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		PlayerBuilder<T> GetBuilder<T>(int rarity, bool addToModelOnceBuilt = true)
			where T : class, IPlayerEntity, new();*/
		
		IPlayerEntity GetPlayer();
		
		bool IsPlayerDead();
		
	}
	public class GamePlayerModel: EntityModel<IPlayerEntity>, IGamePlayerModel {

		[ES3Serializable]
		private string playerUUID = null;
		
		
		private PlayerBuilder<T> GetBuilder<T>(int rarity, bool addToModelOnceBuilt = true) where T : class, IPlayerEntity, new() {
			PlayerBuilder<T> builder = entityBuilderFactory.GetBuilder<PlayerBuilder<T>, T>(rarity);
		
			if (addToModelOnceBuilt) {
				builder.RegisterOnEntityCreated(OnEntityBuilt);
			}

			return builder;
		}

		public IPlayerEntity GetPlayer() {
			if (playerUUID == null) {
				return GetBuilder<PlayerEntity>(0).FromConfig().Build();
			}
			return GetEntity(playerUUID);
		}

		public bool IsPlayerDead() {
			return GetPlayer().HealthProperty.RealValue.Value.CurrentHealth <= 0;
		}

		protected override void OnEntityBuilt(IPlayerEntity entity) {
			base.OnEntityBuilt(entity);
			playerUUID = entity.UUID;
		}
	}
}