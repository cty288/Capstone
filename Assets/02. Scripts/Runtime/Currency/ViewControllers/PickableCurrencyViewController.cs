using _02._Scripts.Runtime.Currency.Model;
using MikroFramework.Architecture;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Entities.Builders;
using Runtime.GameResources.Model.Base;
using Runtime.GameResources.ViewControllers;
using UnityEngine;

namespace _02._Scripts.Runtime.Currency.ViewControllers {
	public interface IPickableCurrencyViewController : IResourceViewController {
		public ICurrencyEntity OnBuildNewPickableCurrencyEntity(CurrencyType currencyType, int amount);
		
		
	}
	
	public class PickableCurrencyViewController : AbstractPickableResourceViewController<CurrencyEntity>, IPickableCurrencyViewController {
		private ICurrencyModel currencyModel;
		private ICommonEntityModel commonEntityModel;
		[SerializeField] protected CurrencyType currencyTypeBuildFromInspector;
		[SerializeField] protected int amountBuildFromInspector;
		protected override void Awake() {
			base.Awake();
			currencyModel = this.GetModel<ICurrencyModel>();
			commonEntityModel = this.GetModel<ICommonEntityModel>();
		}

		protected override void OnBindEntityProperty() {
			
		}

		protected override void OnStartAbsorb() {
			
		}

		protected override bool AbsorbHandler(CurrencyEntity entity) {
			currencyModel.AddCurrency(BoundEntity.CurrencyType, BoundEntity.Amount);
			return true;
		}

		public override void OnRecycled() {
			base.OnRecycled();
			if (BoundEntity != null) {
				commonEntityModel.RemoveEntity(BoundEntity.UUID);
			}
			
		}

		public override IResourceEntity OnBuildNewPickableResourceEntity(bool setRarity, int rarity) {
			return OnBuildNewPickableCurrencyEntity(currencyTypeBuildFromInspector, amountBuildFromInspector);
		}

		public ICurrencyEntity OnBuildNewPickableCurrencyEntity(CurrencyType currencyType, int amount) {
			CurrencyEntity entity = commonEntityModel.GetBuilder<CurrencyEntity>(1).Build();
			entity.InitCurrency(currencyType, amount);
			return entity;
		}
	}
}