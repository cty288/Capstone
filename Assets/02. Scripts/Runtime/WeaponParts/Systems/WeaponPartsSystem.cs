using _02._Scripts.Runtime.BuffSystem;
using _02._Scripts.Runtime.WeaponParts.Model.Base;
using MikroFramework.Architecture;
using Runtime.GameResources.Model.Base;
using Runtime.Weapons.Model.Base;

namespace _02._Scripts.Runtime.WeaponParts.Systems {
	public interface IWeaponPartsSystem : ISystem {
		
	}
	public class WeaponPartsSystem : AbstractSystem, IWeaponPartsSystem{
		private IBuffSystem buffSystem;
		protected override void OnInit() {
			buffSystem = this.GetSystem<IBuffSystem>();
			this.RegisterEvent<OnWeaponPartsUpdate>(OnWeaponPartsUpdate);
		}
		

		private void OnWeaponPartsUpdate(OnWeaponPartsUpdate e) {
			IWeaponPartsEntity previousWeaponParts =
				GlobalGameResourceEntities.GetAnyResource(e.PreviousTopPartsUUID) as IWeaponPartsEntity;
            
			if(previousWeaponParts != null) {
				buffSystem.RemoveBuff(e.WeaponEntity, previousWeaponParts.BuffType);
			}
            
			IWeaponPartsEntity newWeaponParts =
				GlobalGameResourceEntities.GetAnyResource(e.CurrentTopPartsUUID) as IWeaponPartsEntity;
			if(newWeaponParts != null) {
				buffSystem.AddBuff(e.WeaponEntity, newWeaponParts.OnGetBuff(e.WeaponEntity));
			}
		}
	}
}