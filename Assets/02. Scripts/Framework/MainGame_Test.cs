using Runtime.DataFramework.Entities;
using Runtime.Enemies.Model;
using Runtime.GameResources.Model.Base;
using Runtime.RawMaterials.Model.Base;
using Runtime.Weapons.Model.Base;

namespace Framework {
	public class MainGame_Test : SavableArchitecture<MainGame_Test> {
		protected override void Init() {
			this.RegisterModel<ICommonEntityModel>(new CommonEntityModel());
			this.RegisterModel<IEnemyEntityModel>(new EnemyEntityModel());
			//this.RegisterModel<IGameResourceModel>(new GameResourceModel());
			this.RegisterModel<IRawMaterialModel>(new RawMaterialModel());
			this.RegisterModel<IWeaponModel>(new WeaponModel());
		}

		protected override string saveFileSuffix { get; } = "test";
	}
}
