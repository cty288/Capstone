using _02._Scripts.Runtime.Scraps.Model.Builder;
using Framework;
using Runtime.GameResources.Model.Base;
using Runtime.RawMaterials.Model.Base;
using Runtime.RawMaterials.Model.Builder;

namespace _02._Scripts.Runtime.Scraps.Model.Base {
	public interface IScrapModel  : IGameResourceModel<IScrapEntity>, ISavableModel {
		ScrapBuilder GetBuilder(bool addToModelOnceBuilt = true);
	}
	
	public class ScrapModel : GameResourceModel<IScrapEntity>, IScrapModel {

		public ScrapBuilder GetBuilder(bool addToModelOnceBuilt = true) {
			ScrapBuilder builder = entityBuilderFactory.GetBuilder<ScrapBuilder, ScrapEntity>(1);
		
			if (addToModelOnceBuilt) {
				builder.RegisterOnEntityCreated(OnEntityBuilt);
			}

			return builder;
		}
	}
}