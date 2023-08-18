using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Entities.Enemies;

namespace Runtime.Framework {
	public class MainGame : SavableArchitecture<MainGame> {
		protected override void Init() {
			this.RegisterModel<ICommonEntityModel>(new CommonEntityModel());
			this.RegisterModel<IEnemyEntityModel>(new EnemyEntityModel());
		}

		protected override string saveFileSuffix { get; } = "main";
	}
}
