using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Entities.Enemies;

namespace Framework {
	public class MainGame_Test : SavableArchitecture<MainGame_Test> {
		protected override void Init() {
			this.RegisterModel<ICommonEntityModel>(new CommonEntityModel());
			this.RegisterModel<IEnemyEntityModel>(new EnemyEntityModel());
		}

		protected override string saveFileSuffix { get; } = "test";
	}
}
