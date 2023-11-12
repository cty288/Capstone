using System;
using System.Collections.Generic;
using _02._Scripts.Runtime.Currency.Model;
using _02._Scripts.Runtime.Skills.Model.Base;
using MikroFramework.Architecture;
using Runtime.DataFramework.Entities;
using Runtime.GameResources.ViewControllers;
using Runtime.Inventory;
using Runtime.Inventory.Model;
using UnityEngine;
using UnityEngine.UI;

namespace _02._Scripts.Runtime.Skills.ViewControllers.Base {
	public class InventorySkillViewController : AbstractResourceViewController<ISkillEntity>, ISkillViewController, IInventoryResourceViewController {
		private Image icon;
		private Image cooldownImg;
		private Image conditionUnsatisfiedImg;
		private GameObject conditionSatisfiedImg;
		private Func<Dictionary<CurrencyType, int>, bool> inventoryAllSwitchCondition;
		private Func<Dictionary<CurrencyType, int>, bool> useCurrencySatisfiedCondition;
		private Dictionary<CurrencyType, int> currencyAmountDict;
		private ICurrencyModel currencyModel;
		
		private Color conditionUnsatisfiedOpaqueColor;
		private Color coolDownOpaqueColor;
		private Color conditionUnsatisfiedTransparentColor;
		private Color coolDownTransparentColor;
		private Animator conditionSatisfiedAnimator;
		protected override void Awake() {
			base.Awake();
			icon = transform.Find("ItemImage").GetComponent<Image>();
			cooldownImg = transform.Find("CooldownImg").GetComponent<Image>();
			conditionUnsatisfiedImg = transform.Find("ConditionUnSatisfiedImg").GetComponent<Image>();
			conditionSatisfiedImg = transform.Find("ConditionSatisfiedImg").gameObject;
			conditionUnsatisfiedOpaqueColor = conditionUnsatisfiedImg.color;
			coolDownOpaqueColor = cooldownImg.color;
			conditionUnsatisfiedTransparentColor =
				new Color(conditionUnsatisfiedImg.color.r, conditionUnsatisfiedImg.color.g,
					conditionUnsatisfiedImg.color.b, 0);
			coolDownTransparentColor =
				new Color(cooldownImg.color.r, cooldownImg.color.g, cooldownImg.color.b, 0);
			cooldownImg.color = coolDownTransparentColor;
			conditionSatisfiedAnimator = conditionSatisfiedImg.GetComponent<Animator>();
			conditionSatisfiedAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;

			
			Update();
		}
		
		protected override bool CanAutoRemoveEntityWhenLevelEnd { get; } = false;
		protected override IEntity OnBuildNewEntity() {
			return null;
		}

		protected override void Update() {
			base.Update();

			
			
			if (IsHUDSlot && BoundEntity!=null && BoundEntity.HasCooldown() && BoundEntity.GetMaxCooldown() > 0) {
				cooldownImg.fillAmount = BoundEntity.GetRemainingCooldown() / BoundEntity.GetMaxCooldown();

			
				
				
				if (useCurrencySatisfiedCondition.Invoke(currencyAmountDict)) {
					//cooldownImg.gameObject.SetActive(true);
					//lerp transparency to 1
					
					cooldownImg.color = Color.Lerp(cooldownImg.color, coolDownOpaqueColor, Time.deltaTime * 5);
					
					//conditionUnsatisfiedImg.gameObject.SetActive(false);
					
					conditionUnsatisfiedImg.color = Color.Lerp(conditionUnsatisfiedImg.color,
						conditionUnsatisfiedTransparentColor, Time.deltaTime * 5);
					
					
					if (inventoryAllSwitchCondition.Invoke(currencyAmountDict)) {
						conditionSatisfiedImg.SetActive(true);
						
					}
					else {
						conditionSatisfiedImg.SetActive(false);
					}
				}
				else {
					conditionUnsatisfiedImg.color = Color.Lerp(conditionUnsatisfiedImg.color,
						conditionUnsatisfiedOpaqueColor, Time.deltaTime * 5);
					
					conditionSatisfiedImg.SetActive(false);
					//cooldownImg.gameObject.SetActive(false);
					cooldownImg.color = Color.Lerp(cooldownImg.color, coolDownTransparentColor, Time.deltaTime * 5);
				}
				
			}
			else {
				cooldownImg.fillAmount = 0;
				conditionUnsatisfiedImg.color = conditionUnsatisfiedTransparentColor;
				conditionSatisfiedImg.SetActive(false);
				cooldownImg.color = coolDownTransparentColor;
			}

			
		}

		protected override void OnEntityStart() {
			icon.sprite = InventorySpriteFactory.Singleton.GetSprite(BoundEntity.IconSpriteName);
			inventoryAllSwitchCondition = BoundEntity.CanInventorySwitchToCondition;
			useCurrencySatisfiedCondition = BoundEntity.GetUseCurrencySatisfiedCondition;
			currencyModel = this.GetModel<ICurrencyModel>();
			currencyAmountDict = currencyModel.GetCurrencyAmountDict();
		}

		protected override void OnBindEntityProperty() {
			
		}

		public ISkillEntity SkillEntity => BoundEntity;
		public ISkillEntity OnBuildNewSkillEntity() {
			return OnBuildNewEntity() as ISkillEntity;
		}

		public bool IsHUDSlot { get; set; }
	}
}