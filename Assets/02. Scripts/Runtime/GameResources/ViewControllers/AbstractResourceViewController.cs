using MikroFramework.Event;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.ViewControllers.Entities;
using Runtime.GameResources.Model.Base;
using Runtime.GameResources.Model.Builder;
using Runtime.GameResources.Model.Properties.BaitAdjectives;

namespace Runtime.GameResources.ViewControllers {
	public interface IResourceViewController : IEntityViewController {
		IResourceEntity ResourceEntity => Entity as IResourceEntity;
	}
	public abstract class AbstractResourceViewController<T>: AbstractBasicEntityViewController<T>, IResourceViewController
	where  T : class, IResourceEntity {
		/*protected override void OnEntityStart() {
			/*BoundEntity.GetBaitAdjectivesProperty().RealValues.RegisterOnAdd(OnBaitAdjectivesAdd)
				.UnRegisterWhenGameObjectDestroyed(gameObject);
			BoundEntity.GetBaitAdjectivesProperty().RealValues.RegisterOnRemove(OnBaitAdjectivesRemove)
				.UnRegisterWhenGameObjectDestroyed(gameObject);
			OnDisplayNameUpdate(BoundEntity.GetDisplayName());#1# //move to baits
			OnStart();
		}*/

		//protected abstract void OnStart();
		
		/*protected virtual void OnBaitAdjectivesRemove(BaitAdjective adj) {
			OnDisplayNameUpdate(BoundEntity.GetDisplayName());
		}

		protected virtual void OnBaitAdjectivesAdd(BaitAdjective adj) {
			OnDisplayNameUpdate(BoundEntity.GetDisplayName());
		}
		
		protected abstract void OnDisplayNameUpdate(string displayName);*/

		//protected abstract IResourceEntity OnInitEntityInternal();
	}
}