using _02._Scripts.Runtime.Baits.Model.Builders;
using Framework;
using Runtime.GameResources.Model.Base;
using Runtime.Weapons.Model.Base;
using Runtime.Weapons.Model.Builders;

namespace _02._Scripts.Runtime.Baits.Model.Base {
	public interface IBaitModel : IGameResourceModel<IBaitEntity>, ISavableModel {
		BaitBuilder<T> GetBaitBuilder<T>(bool addToModelOnceBuilt = true)
			where T : class, IBaitEntity, new();
	}


	public class BaitModel : GameResourceModel<IBaitEntity>, IBaitModel {
		protected override void OnInit() {
			base.OnInit();
		}
		
		public BaitBuilder<T> GetBaitBuilder<T>(bool addToModelOnceBuilt = true)
			where T : class, IBaitEntity, new() {
			
			BaitBuilder<T> builder = entityBuilderFactory.GetBuilder<BaitBuilder<T>, T>(1);

			if (addToModelOnceBuilt) {
				builder.RegisterOnEntityCreated(OnEntityBuilt);
			}

			return builder;
		}
	}
}