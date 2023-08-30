using Runtime.DataFramework.Entities;
using Runtime.Enemies.Model;
using Runtime.GameResources.Model.Base;
using Runtime.Inventory.Model;
using Runtime.RawMaterials.Model.Base;
using Runtime.Weapons.Model.Base;

namespace Framework {
	public class MainGame_Test : SavableArchitecture<MainGame_Test> {
		protected override void Init() {
			this.RegisterSystem<IInventorySystem>(new InventorySystem());
			
			this.RegisterModel<ICommonEntityModel>(new CommonEntityModel());
			this.RegisterModel<IEnemyEntityModel>(new EnemyEntityModel());
			//this.RegisterModel<IGameResourceModel>(new GameResourceModel());
			this.RegisterModel<IRawMaterialModel>(new RawMaterialModel());
			this.RegisterModel<IInventoryModel>(new InventoryModel());
			this.RegisterModel<IWeaponModel>(new WeaponModel());
		}

		protected override string saveFileSuffix { get; } = "test";
	}
}
