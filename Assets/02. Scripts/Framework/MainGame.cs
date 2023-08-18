using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Entities.Enemies;
using Runtime.DataFramework.Entities.Weapons;

namespace Framework {
	public class MainGame : SavableArchitecture<MainGame> {
		protected override void Init() {
			this.RegisterModel<ICommonEntityModel>(new CommonEntityModel());
			this.RegisterModel<IEnemyEntityModel>(new EnemyEntityModel());
			this.RegisterModel<IWeaponEntityModel>(new WeaponEntityModel());
		}

		protected override string saveFileSuffix { get; } = "main";
	}
}
