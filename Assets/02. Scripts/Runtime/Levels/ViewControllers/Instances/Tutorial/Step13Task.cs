using System.Collections;
using System.Collections.Generic;
using _02._Scripts.Runtime.CollectableResources.ViewControllers.Base;
using _02._Scripts.Runtime.Currency.Model;
using _02._Scripts.Runtime.Levels.Models.LevelPassCondition;
using _02._Scripts.Runtime.Pillars.Systems;
using _02._Scripts.Runtime.PlayerTasks;
using _02._Scripts.Runtime.WeaponParts.Model.Base;
using MikroFramework.Architecture;
using MikroFramework.ResKit;
using Polyglot;
using Runtime.Controls;
using Runtime.GameResources.Model.Base;
using Runtime.Inventory.Model;
using Runtime.Utilities;
using UnityEngine;

namespace _02._Scripts.Runtime.Levels.ViewControllers.Instances.Tutorial {

	
	public class Step13Task : PlayerTask, ICanGetModel, ICanRegisterEvent, ICanGetSystem {
		private bool satisified = false;
		
		public override void OnInit() {
			base.OnInit();
			this.RegisterEvent<OnPillarActivated>(OnPillarActivated);
		}

		private void OnPillarActivated(OnPillarActivated obj) {
			satisified = true;
		}


		public override string GetDescription() {
			return Localization.Get("TUTORIAL_STEP13_TASK");
		}

		public override bool IsSatisfied() {
			return satisified;
		}
		
		public override void OnFinish() {
			this.UnRegisterEvent<OnPillarActivated>(OnPillarActivated);
			CoroutineRunner.Singleton.StartCoroutine(ClearPanel());
		}

		private IEnumerator ClearPanel() {
			yield return new WaitForSeconds(2f);
			IPlayerTaskSystem playerTaskSystem = this.GetSystem<IPlayerTaskSystem>();
			playerTaskSystem.AddTask(new PickWeaponPartsTask());
		}
	}

	public class PickWeaponPartsTask : PlayerTask, ICanGetModel {

		private IInventoryModel inventoryModel;
		private int partsCount = 0;
		
		public override void OnInit() {
			base.OnInit();
			inventoryModel = this.GetModel<IInventoryModel>();
		}

		public override string GetDescription() {
			return Localization.GetFormat("TUTORIAL_PICK_PARTS_TASK", partsCount);
		}

		public override bool IsSatisfied() {
			int count = 0;
			var slots = inventoryModel.GetAllSlots((slot => !slot.IsEmpty()));
			foreach (ResourceSlot resourceSlot in slots) {
				IResourceEntity resourceEntity =
					GlobalGameResourceEntities.GetAnyResource(resourceSlot.GetLastItemUUID());
				if (resourceEntity is IWeaponPartsEntity) {
					count++;
				}
			}

			partsCount = count;
			return partsCount >= 3;
		}

		public override void OnFinish() {
			CoroutineRunner.Singleton.StartCoroutine(ClearPanel());
		}
		
		private IEnumerator ClearPanel() {
			yield return new WaitForSeconds(2f);
			this.SendEvent<OnTutorialTaskFinish>(new OnTutorialTaskFinish() {
				TaskID = 13
			});
		}
	}
}