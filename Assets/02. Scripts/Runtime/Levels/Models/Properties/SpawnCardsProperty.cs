using _02._Scripts.Runtime.Levels.ViewControllers;
using Runtime.DataFramework.Properties;

namespace _02._Scripts.Runtime.Levels.Models.Properties {
	public interface ISpawnCardsProperty : IListProperty<LevelSpawnCard> {
		
	}
	public class SpawnCardsProperty : IndependentListProperty<LevelSpawnCard>, ISpawnCardsProperty {
		protected override PropertyName GetPropertyName() {
			return PropertyName.spawn_cards;
		}
	}
}