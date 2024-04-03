using System.Collections.Generic;
using DG.Tweening;
using MikroFramework;
using MikroFramework.ActionKit;
using MikroFramework.BindableProperty;
using Polyglot;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Factions;
using Runtime.DataFramework.Properties.CustomProperties;
using Runtime.Enemies.Model;
using Runtime.Enemies.Model.Builders;
using Runtime.Enemies.ViewControllers.Base;
using UnityEngine;

namespace Runtime.Enemies.ViewControllers.Instances.Berserker {
	public class BerserkerEntity : BossEntity<BerserkerEntity>
	{
		[field: ES3Serializable]
		public override string EntityName { get; set; } = "Berserker";

		public List<GameObject> Nodes;
		
		protected override void OnEntityStart(bool isLoadedFromSave) {
            
		}

		public override void OnRecycle() {
			base.OnRecycle();
		}
		protected override void OnInitModifiers(int rarity, int level) {
            
		}
        

        
		protected override void OnEnemyRegisterAdditionalProperties() {
            
		}

		protected override string OnGetDescription(string defaultLocalizationKey) {
			return Localization.Get(defaultLocalizationKey);
		}

		protected override ICustomProperty[] OnRegisterCustomProperties()
		{
            
			return new ICustomProperty[0];
		}

	}
	
	
	public class Berserker : AbstractBossViewController<BerserkerEntity>{
		private bool deathAnimationEnd = false;

		[SerializeField] private List<GameObject> nodes;
		
		protected override void OnEntityStart()
		{
			BoundEntity.Nodes = nodes;
		}

		protected override void OnEntityTakeDamage(int damage, int currenthealth, ICanDealDamage damagedealer) {
			
		}

		protected override void OnEntityHeal(int heal, int currenthealth, IBelongToFaction healer) {
			
		}

		protected override MikroAction WaitingForDeathCondition() {
			transform.DOScale(Vector3.zero, 0.5f).OnComplete(() => {
				deathAnimationEnd = true;
			});
            
			return UntilAction.Allocate(() => deathAnimationEnd);
		}

		protected override void OnAnimationEvent(string eventName) {
			
		}

		protected override IEnemyEntity OnInitEnemyEntity(EnemyBuilder<BerserkerEntity> builder) {
			return builder.FromConfig().Build();
		}

		public override void OnRecycled() {
			base.OnRecycled();
			deathAnimationEnd = false;
		}
	}
}