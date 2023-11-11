using System.Collections.Generic;
using _02._Scripts.Runtime.Baits.Commands;
using _02._Scripts.Runtime.Baits.Model.Base;
using _02._Scripts.Runtime.Baits.Model.Builders;
using MikroFramework.Architecture;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Properties;
using Runtime.DataFramework.ViewControllers.Entities;
using Runtime.Enemies.Model.Properties;
using Runtime.GameResources.Model.Base;
using Runtime.GameResources.ViewControllers;
using Runtime.Player;
using Runtime.Weapons.Model.Builders;
using UnityEngine;
using PropertyName = Runtime.DataFramework.Properties.PropertyName;

namespace _02._Scripts.Runtime.Baits.ViewControllers {
	
	public interface IBaitViewController : IResourceViewController {
		IBaitEntity BaitEntity { get; }

		IBaitEntity BuildBait(int rarity, float vigilianceBase, List<TasteType> tastesBase);
	}
	
	
	public class BaitViewController : AbstractInHandDeployableResourceViewController<BaitEntity>, IBaitViewController {
		private IBaitModel baitModel;
		protected IGamePlayerModel playerModel;

		[Header("Build Bait From Inspector")] 
		[SerializeField] private int rarity = 1;
		[SerializeField] private float vigilianceBase = 100f;
		[SerializeField] private List<TasteType> tastesBase = new List<TasteType>();
		
		protected override void Awake() {
			base.Awake();
			baitModel = this.GetModel<IBaitModel>();
			playerModel = this.GetModel<IGamePlayerModel>();
		}

		public override IResourceEntity OnBuildNewPickableResourceEntity(bool setRarity, int rarity) {
			if (setRarity) {
				return BuildBait(rarity, vigilianceBase, tastesBase);
			}

			return OnBuildNewEntity() as IResourceEntity;
		}
		protected override IEntity OnBuildNewEntity() {
			return BuildBait(rarity, vigilianceBase, tastesBase);
		}
		
		
		public IBaitEntity BuildBait(int rarity, float vigilianceBase, List<TasteType> tastesBase) {
			if(baitModel == null)
				baitModel = this.GetModel<IBaitModel>();
			BaitBuilder<BaitEntity> builder = baitModel.GetBaitBuilder<BaitEntity>();
			return builder.SetProperty(new PropertyNameInfo(PropertyName.rarity), rarity)
				.SetBaseTastes(tastesBase)
				.SetBaseVigiliance(vigilianceBase)
				.SetProperty(new PropertyNameInfo(PropertyName.max_stack), 1)
				.Build();
		}

		protected override void OnBindEntityProperty() {
			
		}
		

		public override void OnItemScopePressed() {
			
		}

		public override void OnItemScopeReleased() {
			
		}

		public IBaitEntity BaitEntity => BoundEntity;

		public override void OnDeployFailureReasonChanged(DeployFailureReason lastReason, DeployFailureReason currentReason) {
			base.OnDeployFailureReasonChanged(lastReason, currentReason);
			if (currentReason == DeployFailureReason.BaitInBattle) {
				this.SendCommand(SetDeployStatusHintCommand.Allocate("DEPLOY_ERROR_COMBAT"));
			}
		}
	}
}