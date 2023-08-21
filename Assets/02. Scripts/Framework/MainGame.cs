using Runtime.DataFramework.Entities;
using Runtime.Enemies.Model;
using Runtime.GameResources.Model.Base;
using Runtime.Weapons;
using Runtime.Weapons.Model.Base;

namespace Framework
{
	public class MainGame : SavableArchitecture<MainGame>
	{
		protected override void Init()
		{
			this.RegisterModel<ICommonEntityModel>(new CommonEntityModel());
			this.RegisterModel<IEnemyEntityModel>(new EnemyEntityModel());
			this.RegisterModel<IWeaponModel>(new WeaponModel());
			this.RegisterModel<IGameResourceModel>(new GameResourceModel());
		}

		protected override string saveFileSuffix { get; } = "main";
	}
}
