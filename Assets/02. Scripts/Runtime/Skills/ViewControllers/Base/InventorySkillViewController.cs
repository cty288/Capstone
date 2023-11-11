using _02._Scripts.Runtime.Skills.Model.Base;
using Runtime.DataFramework.Entities;
using Runtime.GameResources.ViewControllers;
using Runtime.Inventory;
using UnityEngine.UI;

namespace _02._Scripts.Runtime.Skills.ViewControllers.Base {
	public class InventorySkillViewController : AbstractResourceViewController<ISkillEntity>, ISkillViewController, IInventoryResourceViewController {
		private Image icon;
		
		
		protected override void Awake() {
			base.Awake();
			icon = transform.Find("ItemImage").GetComponent<Image>();
		}
		
		protected override bool CanAutoRemoveEntityWhenLevelEnd { get; } = false;
		protected override IEntity OnBuildNewEntity() {
			return null;
		}

		protected override void OnEntityStart() {
			icon.sprite = InventorySpriteFactory.Singleton.GetSprite(BoundEntity.IconSpriteName);
		}

		protected override void OnBindEntityProperty() {
			
		}

		public ISkillEntity SkillEntity => BoundEntity;
	}
}