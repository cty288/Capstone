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
using Runtime.Utilities.Collision;
using UnityEngine;

namespace Runtime.Enemies.ViewControllers.Instances.Berserker {
	public class BerserkerEntity : BossEntity<BerserkerEntity>
	{
		[field: ES3Serializable]
		public override string EntityName { get; set; } = "Berserker";

		public List<BerserkerNode> Nodes;
		public int StaggerThreshold;
		public int StaggerDamage = 0;
		
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
            
			return new[]
			{
				new AutoConfigCustomProperty("entity")
			};
		}

	}
	
	
	public class Berserker : AbstractBossViewController<BerserkerEntity>{
		private bool deathAnimationEnd = false;

		[SerializeField] private List<BerserkerNode> nodes;
		[SerializeField] private HurtBox[] vulerableHurboxes;
		private HashSet<IHurtbox> hashedVulerableHurboxes = new HashSet<IHurtbox>();

		private float StaggerDelay = 0f;
		private float lastHitTime = 0f;
		private float StaggerTick = 0f;
		private float staggerTimer = 0f;
		
		protected override void Awake()
		{
			base.Awake();
			hashedVulerableHurboxes.Clear();
			foreach (var hurtbox in vulerableHurboxes) {
				hashedVulerableHurboxes.Add(hurtbox);
			}
		}

		protected override void OnEntityStart()
		{
			BoundEntity.Nodes = nodes;
			BoundEntity.StaggerThreshold = BoundEntity.GetCustomDataValue<int>("stagger", "staggerThreshold");
			StaggerDelay = BoundEntity.GetCustomDataValue<float>("stagger", "staggerDelay");
			StaggerTick = BoundEntity.GetCustomDataValue<float>("stagger", "staggerTick");
		}

		protected override void OnEntityTakeDamage(int damage, int currenthealth, ICanDealDamage damagedealer) {
			
		}

		protected override void OnEntityHeal(int heal, int currenthealth, IBelongToFaction healer) {
			
		}

		protected override void Update()
		{
			base.Update();
			
			if(Time.time - lastHitTime >= StaggerDelay) {
				staggerTimer += Time.deltaTime;
				if (staggerTimer >= StaggerTick)
				{
					BoundEntity.StaggerDamage--;
					staggerTimer = 0f;
				}
			}
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

		public override void HurtResponse(HitData data)
		{
			if(hashedVulerableHurboxes.Contains(data.Hurtbox)) {
				BoundEntity.StaggerDamage += data.Damage;
				lastHitTime = Time.time;
				if(BoundEntity.StaggerDamage >= BoundEntity.StaggerThreshold) {
					BoundEntity.StaggerDamage = 0;
					// Stagger();
				}
			}
			BoundEntity.TakeDamage(data.Damage, data.Attacker,out _, data);
		}

		public override void OnRecycled() {
			base.OnRecycled();
			deathAnimationEnd = false;
		}
	}
}