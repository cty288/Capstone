using Runtime.DataFramework.Entities;
using Runtime.Enemies.Model;
using Runtime.GameResources.Model.Base;
using Runtime.Inventory.Model;
using Runtime.RawMaterials.Model.Base;

namespace Framework {
	public class MainGame : SavableArchitecture<MainGame> {
		protected override void Init() {
			GlobalEntities.Reset();
			GlobalGameResourceEntities.Reset();
			
			this.RegisterSystem<IInventorySystem>(new InventorySystem());
			
			this.RegisterModel<ICommonEntityModel>(new CommonEntityModel());
			this.RegisterModel<IEnemyEntityModel>(new EnemyEntityModel());
			//this.RegisterModel<IGameResourceModel>(new GameResourceModel());
			this.RegisterModel<IRawMaterialModel>(new RawMaterialModel());
			this.RegisterModel<IInventoryModel>(new InventoryModel());
		}

		protected override string saveFileSuffix { get; } = "main";
	}
}
