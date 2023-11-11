using _02._Scripts.Runtime.Skills.Model.Base;
using _02._Scripts.Runtime.Skills.Model.Builders;
using MikroFramework.Architecture;
using Runtime.DataFramework.Properties;
using Runtime.GameResources.Model.Base;
using Runtime.GameResources.ViewControllers;

namespace _02._Scripts.Runtime.Skills.ViewControllers.Base {
	
	public interface ISkillViewController : IResourceViewController {
		ISkillEntity SkillEntity { get; }
	}
	
	
	
	public abstract class AbstractInHandSkillViewController<T> :
		AbstractPickableInHandResourceViewController<T>, ISkillViewController  where T : class, ISkillEntity, new() {
		
		private ISkillModel skillModel;

		protected override void Awake() {
			base.Awake();
			skillModel = this.GetModel<ISkillModel>();
		}

		public override IResourceEntity OnBuildNewPickableResourceEntity(bool setRarity, int rarity) {
			if(skillModel == null) {
				skillModel = this.GetModel<ISkillModel>();
			}

			SkillBuilder<T> builder = skillModel.GetSkillBuilder<T>(1);
			if (setRarity) {
				builder.SetProperty(new PropertyNameInfo(PropertyName.rarity), rarity);
			}

			return OnInitSkillEntity(builder);
		}

		protected abstract IResourceEntity OnInitSkillEntity(SkillBuilder<T> builder);


		public override void OnItemScopePressed() {
			
		}


		public override void OnItemScopeReleased() {
			
		}

		public ISkillEntity SkillEntity => BoundEntity;
	}
}