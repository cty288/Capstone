using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Entities.Enemies;
using Runtime.DataFramework.Entities.Weapons;
using Runtime.Enemies.Model;
using Runtime.GameResources.Model.Base;

namespace Framework
{
	public class MainGame : SavableArchitecture<MainGame>
	{
		protected override void Init()
		{
			this.RegisterModel<ICommonEntityModel>(new CommonEntityModel());
			this.RegisterModel<IEnemyEntityModel>(new EnemyEntityModel());
			this.RegisterModel<IWeaponEntityModel>(new WeaponEntityModel());
			this.RegisterModel<IGameResourceModel>(new GameResourceModel());
		}

		protected override string saveFileSuffix { get; } = "main";
	}
}
