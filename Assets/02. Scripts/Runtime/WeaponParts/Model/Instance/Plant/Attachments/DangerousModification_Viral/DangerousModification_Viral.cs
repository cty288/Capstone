using System;
using System.Collections.Generic;
using _02._Scripts.Runtime.BuffSystem;
using _02._Scripts.Runtime.WeaponParts.Model.Base;
using _02._Scripts.Runtime.WeaponParts.Model.Instance.BuildBuff.Combat;
using _02._Scripts.Runtime.WeaponParts.Model.Instance.BuildBuff.PlantBuff;
using _02._Scripts.Runtime.WeaponParts.Model.Instance.Combat.Attachments.DangerousModification;
using MikroFramework.BindableProperty;
using Polyglot;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Factions;
using Runtime.DataFramework.Properties.CustomProperties;
using Runtime.GameResources.Model.Base;
using Runtime.GameResources.Others;
using Random = UnityEngine.Random;

namespace _02._Scripts.Runtime.WeaponParts.Model.Instance.Plant.Attachments.DangerousModification_Viral {
	public class DangerousModification_ViralDamageDealer : ICanDealDamage {
		private Action<ICanDealDamage, IDamageable, int> _onDealDamageCallback;
		private Action<ICanDealDamage, IDamageable> _onKillDamageableCallback;
		public BindableProperty<Faction> CurrentFaction { get; } = new BindableProperty<Faction>(Faction.Neutral);
		public void OnKillDamageable(ICanDealDamage sourceDealer, IDamageable damageable) {
			
		}

		public void OnDealDamage(ICanDealDamage sourceDealer, IDamageable damageable, int damage) {
			
		}
		
		public DangerousModification_ViralDamageDealer(ICanDealDamage parentDamageDealer) {
			ParentDamageDealer = parentDamageDealer;
		}

		public HashSet<Func<int, int>> OnModifyDamageCountCallbackList { get; } = new HashSet<Func<int, int>>();

		Action<ICanDealDamage, IDamageable, int> ICanDealDamage.OnDealDamageCallback {
			get => _onDealDamageCallback;
			set => _onDealDamageCallback = value;
		}

		Action<ICanDealDamage, IDamageable> ICanDealDamage.OnKillDamageableCallback {
			get => _onKillDamageableCallback;
			set => _onKillDamageableCallback = value;
		}

		public ICanDealDamage ParentDamageDealer { get; }
	}
	public class DangerousModification_Viral : WeaponPartsEntity<DangerousModification_Viral, DangerousModification_ViralBuff> {
		[field: ES3Serializable]
		public override string EntityName { get; set; } = "DangerousModification_Viral";
		protected override void OnEntityStart(bool isLoadedFromSave) {
			
		}

		protected override void OnInitModifiers(int rarity) {
			
		}

		public override bool Collectable => true;
		protected override string OnGetWeaponPartDescription(string defaultLocalizationKey) {
			float chance = GetCustomDataValueOfCurrentLevel<float>("chance");
			int displayChance = (int) (chance * 100);

			int damage = GetCustomDataValueOfCurrentLevel<int>("damage");
			
			return Localization.GetFormat(defaultLocalizationKey, displayChance, damage);
		}

		public override WeaponPartType WeaponPartType => WeaponPartType.Attachment;
		
		protected override ICustomProperty[] OnRegisterAdditionalCustomProperties() {
			return null;
		}
	}
	
	public class DangerousModification_ViralBuff : WeaponPartsBuff<DangerousModification_Viral, DangerousModification_ViralBuff> {
		[field: ES3Serializable]	
		public override float TickInterval { get; protected set; } = -1;
		
		public override void OnInitialize() {
			weaponEntity.RegisterOnModifyValueEvent<OnPlantBuffModifyChanceEvent>(OnPlantBuffModifyChanceEvent);
			RegisterWeaponBuildBuffEvent<OnPlantBuffGenerateInitialBuffEvent>(OnPlantBuffGenerateInitialBuffEvent);
		}

		private void OnPlantBuffGenerateInitialBuffEvent(OnPlantBuffGenerateInitialBuffEvent e) {
			float chance = weaponPartsEntity.GetCustomDataValueOfCurrentLevel<float>("chance");
			if (Random.Range(0f, 1f) < chance) {
				ICanDealDamage rootDamageDealer = weaponEntity.GetRootDamageDealer();
				if (rootDamageDealer is IDamageable damageable) {
					int damage = weaponPartsEntity.GetCustomDataValueOfCurrentLevel<int>("damage");
					damageable.TakeDamage(damage, new DangerousModification_ViralDamageDealer(weaponEntity), out _);
				}
			}
		}

		private OnPlantBuffModifyChanceEvent OnPlantBuffModifyChanceEvent(OnPlantBuffModifyChanceEvent e) {
			e.Value *= 2;
			return e;
		}


		public override void OnStart() {
			base.OnStart();
		}

		public override BuffStatus OnTick() {
			return BuffStatus.Running;
		}

		public override void OnBuffEnd() {
			
		}

		protected override IEnumerable<BuffedProperties> GetBuffedPropertyGroups() {
			return null;
		}

		public override void OnRecycled() {
			UnRegisterWeaponBuildBuffEvent<OnPlantBuffGenerateInitialBuffEvent>(OnPlantBuffGenerateInitialBuffEvent);
			weaponEntity.UnRegisterOnModifyValueEvent<OnPlantBuffModifyChanceEvent>(OnPlantBuffModifyChanceEvent);
			base.OnRecycled();
		}

		public override List<GetResourcePropertyDescriptionGetter> OnRegisterResourcePropertyDescriptionGetters(
			string iconName, string title) {
			return new List<GetResourcePropertyDescriptionGetter>() {
				new GetResourcePropertyDescriptionGetter(() => {
					
					float chance = weaponPartsEntity.GetCustomDataValueOfCurrentLevel<float>("chance");
					int displayChance = (int) (chance * 100);
					int damage = weaponPartsEntity.GetCustomDataValueOfCurrentLevel<int>("damage");
					
					return new WeaponBuffedAdditionalPropertyDescription(iconName, title,
						Localization.GetFormat("DangerousModification_Viral_desc", displayChance, damage));
				})
			};
		}
	}
}