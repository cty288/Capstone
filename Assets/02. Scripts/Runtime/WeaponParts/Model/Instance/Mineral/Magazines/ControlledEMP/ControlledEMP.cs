using System.Collections.Generic;
using System.Linq;
using _02._Scripts.Runtime.BuffSystem;
using _02._Scripts.Runtime.WeaponParts.Model.Base;
using _02._Scripts.Runtime.WeaponParts.Model.Instance.BuildBuff.Plant;
using Polyglot;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.DataFramework.Properties.CustomProperties;
using Runtime.DataFramework.ViewControllers.Entities;
using Runtime.GameResources.Model.Base;
using Runtime.GameResources.Others;
using Runtime.Utilities.Collision;
using Runtime.Weapons.Model.Base;
using UnityEngine;

namespace _02._Scripts.Runtime.WeaponParts.Model.Instance.Mineral.Magazines.ControlledEMP {
	public class ControlledEMP : WeaponPartsEntity<ControlledEMP, ControlledEMPBuff> {
		[field: ES3Serializable]
		public override string EntityName { get; set; } = "ControlledEMP";
		protected override void OnEntityStart(bool isLoadedFromSave) {
			
		}

		protected override void OnInitModifiers(int rarity) {
			
		}

		public override bool Collectable => true;
		protected override string OnGetWeaponPartDescription(string defaultLocalizationKey) {
			return Localization.Get(defaultLocalizationKey);
		}
		

		public override WeaponPartType WeaponPartType => WeaponPartType.Magazine;
		
		protected override ICustomProperty[] OnRegisterAdditionalCustomProperties() {
			return null;
		}
	}

	public class ControlledEMPBuff : WeaponPartsBuff<ControlledEMP, ControlledEMPBuff> {
		[field: ES3Serializable]	
		public override float TickInterval { get; protected set; } = -1;

		private Dictionary<string, MineralBuffModifyTriggerAOEEvent> storedAOEs = new Dictionary<string, MineralBuffModifyTriggerAOEEvent>();

		public override void OnInitialize() {
			weaponEntity.RegisterOnModifyValueEvent<MineralBuffModifyTriggerAOEEvent>(OnModifyTriggerAOE);
			weaponEntity.RegisterOnModifyHitData(OnModifyHitData);
		}

		private HitData OnModifyHitData(HitData hit, IWeaponEntity weapon) {
			//find all stored AOEs hit ids where the hitID is different from the current hitID
			var ids = storedAOEs.Keys.Where(k => k != hit.HitDataUUID).ToList();
			if (hit.Hurtbox == null || !hit.Hurtbox.Owner) {
				return hit;
			}
			
			IDamageable target = hit.Hurtbox.Owner.GetComponent<IDamageableViewController>()?.DamageableEntity;
			if (target == null) {
				return hit;
			}
			
			foreach (var id in ids) {
				MineralBuffModifyTriggerAOEEvent e = storedAOEs[id];
				storedAOEs.Remove(id);
				
				if(e.MineralBuff.IsRecycled) continue;
				e.MineralBuff.RangeAOE(e.Range, e.Damage, e.Duration, hit.Hurtbox.Owner.transform, target,
					new MineralBuffAOE(weaponEntity), true);
			}
			
			

			return hit;
		}

		private MineralBuffModifyTriggerAOEEvent OnModifyTriggerAOE(MineralBuffModifyTriggerAOEEvent e) {
			e.Value = false;
			storedAOEs.TryAdd(e.HitID, e);
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


		public override void OnRecycled() {
			base.OnRecycled();
			weaponEntity.UnRegisterOnModifyValueEvent<MineralBuffModifyTriggerAOEEvent>(OnModifyTriggerAOE);
			weaponEntity.UnRegisterOnModifyHitData(OnModifyHitData);
			storedAOEs.Clear();
		}

		protected override IEnumerable<BuffedProperties> GetBuffedPropertyGroups() {
			return null;
		}

		public override List<GetResourcePropertyDescriptionGetter> OnRegisterResourcePropertyDescriptionGetters(
			string iconName, string title) {
			return new List<GetResourcePropertyDescriptionGetter>() {
				new GetResourcePropertyDescriptionGetter(() => {
					return new WeaponBuffedAdditionalPropertyDescription(iconName, title,
						Localization.Get("ControlledEMP_desc"));
				})
			};
		}
	}
}