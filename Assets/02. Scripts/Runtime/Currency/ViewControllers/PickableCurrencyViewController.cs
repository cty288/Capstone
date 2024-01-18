using _02._Scripts.Runtime.Currency.Model;
using MikroFramework.Architecture;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Entities.Builders;
using Runtime.GameResources.Model.Base;
using Runtime.GameResources.ViewControllers;
using UnityEngine;

namespace _02._Scripts.Runtime.Currency.ViewControllers {
	public interface IPickableCurrencyViewController : IResourceViewController {
		public ICurrencyEntity OnBuildNewPickableCurrencyEntity(CurrencyType currencyType, int amount,
			bool addToModelWhenBuilt = true);
		
		
	}
	
	public class PickableCurrencyViewController : AbstractPickableResourceViewController<CurrencyEntity>, IPickableCurrencyViewController {
		private ICurrencyModel currencyModel;
		private ICommonEntityModel commonEntityModel;
		[SerializeField] protected CurrencyType currencyTypeBuildFromInspector;
		[SerializeField] protected int amountBuildFromInspector;
		private ICurrencySystem currencySystem;
		protected override void Awake() {
			base.Awake();
			currencyModel = this.GetModel<ICurrencyModel>();
			commonEntityModel = this.GetModel<ICommonEntityModel>();
			currencySystem = this.GetSystem<ICurrencySystem>();
		}

		protected override void OnBindEntityProperty() {
			
		}

		protected override void OnStartAbsorb() {
			
		}

		protected override bool AbsorbHandler(CurrencyEntity entity) {
			currencySystem.AddCurrency(BoundEntity.CurrencyType, BoundEntity.Amount);
			return true;
		}

		public override void OnRecycled() {
			base.OnRecycled();
			if (BoundEntity != null) {
				commonEntityModel.RemoveEntity(BoundEntity.UUID);
			}
			
		}

		public override IResourceEntity OnBuildNewPickableResourceEntity(bool setRarity, int rarity,
			bool addToModelWhenBuilt = true) {
			return OnBuildNewPickableCurrencyEntity(currencyTypeBuildFromInspector, amountBuildFromInspector,
				addToModelWhenBuilt);
		}

		public ICurrencyEntity OnBuildNewPickableCurrencyEntity(CurrencyType currencyType, int amount,
			bool addToModelWhenBuilt = true) {
			if (commonEntityModel == null) {
				commonEntityModel = this.GetModel<ICommonEntityModel>();
			}
			CurrencyEntity entity = commonEntityModel.GetBuilder<CurrencyEntity>(1).Build();
			entity.InitCurrency(currencyType, amount);
			return entity;
		}
	}
}