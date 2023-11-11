using _02._Scripts.Runtime.Skills.Model.Builders;
using Runtime.GameResources.Model.Base;

namespace _02._Scripts.Runtime.Skills.Model.Base {
	public interface ISkillModel : IGameResourceModel<ISkillEntity> {
		SkillBuilder<T> GetSkillBuilder<T>(int rarity, bool addToModelOnceBuilt = true)
			where T : class, ISkillEntity, new();
	}
	public class SkillModel : GameResourceModel<ISkillEntity>, ISkillModel {
		public SkillBuilder<T> GetSkillBuilder<T>(int rarity, bool addToModelOnceBuilt = true) where T : class, ISkillEntity, new() {
			SkillBuilder<T> builder = entityBuilderFactory.GetBuilder<SkillBuilder<T>, T>(rarity);
			if (addToModelOnceBuilt) {
				builder.RegisterOnEntityCreated(OnEntityBuilt);
			}

			return builder;
		}
	}
}