using _02._Scripts.Runtime.Baits.Model.Base;
using _02._Scripts.Runtime.Currency;
using _02._Scripts.Runtime.Levels.Models;
using _02._Scripts.Runtime.Levels.Systems;
using _02._Scripts.Runtime.Currency.Model;
using _02._Scripts.Runtime.Skills.Model.Base;
using MikroFramework.ResKit;
using Runtime.DataFramework.Entities;
using Runtime.Enemies.Model;
using Runtime.GameResources.Model.Base;
using Runtime.Inventory.Model;
using Runtime.Player;
using Runtime.RawMaterials.Model.Base;
using Runtime.Spawning;
using Runtime.Weapons.Model.Base;


namespace Framework {
	public struct OnBeforeGameSave{}
	public class MainGame : SavableArchitecture<MainGame> {
		
		protected override void Init() {
			//ES3AutoSaveMgr.Current.Load();
			GlobalEntities.Reset();
			GlobalGameResourceEntities.Reset();
			
			this.RegisterSystem<IInventorySystem>(new InventorySystem());
			this.RegisterSystem<ILevelSystem>(new LevelSystem());
			this.RegisterSystem<ICurrencySystem>(new CurrencySystem());
			
			this.RegisterModel<ICommonEntityModel>(new CommonEntityModel());
			this.RegisterModel<IEnemyEntityModel>(new EnemyEntityModel());
			//this.RegisterModel<IGameResourceModel>(new GameResourceModel());
			this.RegisterModel<IRawMaterialModel>(new RawMaterialModel());
			this.RegisterModel<IInventoryModel>(new InventoryModel());
			this.RegisterModel<IWeaponModel>(new WeaponModel());
			this.RegisterModel<IGamePlayerModel>(new GamePlayerModel());
			this.RegisterModel<ILevelModel>(new LevelModel());
			this.RegisterModel<IDirectorModel>(new DirectorModel());
			this.RegisterModel<IBaitModel>(new BaitModel());
			this.RegisterModel<ICurrencyModel>(new CurrencyModel());
			this.RegisterModel<ISkillModel>(new SkillModel());
			
			this.RegisterExtensibleUtility<ResLoader>(new ResLoader());
		}

		public override void SaveGame() {
			if (!IsSave) {
				return;
			}
			this.SendEvent<OnBeforeGameSave>();
			foreach (AbstractSavableModel savableModel in savableModels) {
				savableModel.Save(saveFileSuffix);
			}
			foreach (AbstractSavableSystem savableSystem in savableSystems) {
				savableSystem.Save(saveFileSuffix);
			}

			if (ES3AutoSaveMgr.Current) {
				ES3AutoSaveMgr.Current.Save();
			}
		}

		protected override string saveFileSuffix { get; } = "main";
	}
}
