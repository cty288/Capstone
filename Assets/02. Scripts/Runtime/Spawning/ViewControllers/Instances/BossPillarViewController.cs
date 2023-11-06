﻿using System;
using System.Collections.Generic;
using _02._Scripts.Runtime.Currency.Model;
using _02._Scripts.Runtime.Levels.Models;
using _02._Scripts.Runtime.Levels.Models.Properties;
using MikroFramework.Architecture;
using MikroFramework.Event;
using MikroFramework.UIKit;
using Polyglot;
using Runtime.Controls;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Properties;
using Runtime.DataFramework.ViewControllers.Entities;
using Runtime.Spawning.Commands;
using Runtime.UI;
using Runtime.Utilities;
using UnityEngine;
using PropertyName = Runtime.DataFramework.Properties.PropertyName;

//using PropertyName = UnityEngine.PropertyName;

namespace Runtime.Spawning.ViewControllers.Instances {
	public interface IBossPillarViewController : IDirectorViewController {
		//IEntity OnBuildNewEntity(int level, Dictionary<CurrencyType, LevelBossSpawnCostInfo> bossSpawnCosts);
		void SetBossSpawnCosts(Dictionary<CurrencyType, LevelBossSpawnCostInfo> bossSpawnCosts);
		
		BoxCollider SpawnSizeCollider { get; }
	}
	public class BossPillarViewController : AbstractBasicEntityViewController<BossPillarEntity>, IDirectorViewController, IBossPillarViewController {
		protected override bool CanAutoRemoveEntityWhenLevelEnd { get; } = false;
		protected IDirectorModel directorModel;
		protected ILevelEntity LevelEntity;
		protected int levelNumber;
		protected Dictionary<CurrencyType, LevelBossSpawnCostInfo> bossSpawnCosts;
		protected Action<GameObject, IDirectorViewController> onSpawnEnemy;
		protected ILevelModel levelModel;
		
		
		protected override void Awake() {
			base.Awake();
			directorModel = this.GetModel<IDirectorModel>();
			levelModel = this.GetModel<ILevelModel>();
			
		}

		protected override IEntity OnBuildNewEntity() {
			return OnBuildNewEntity(1, new Dictionary<CurrencyType, LevelBossSpawnCostInfo>());
		}
		
		public IEntity OnBuildNewEntity(int level, Dictionary<CurrencyType, LevelBossSpawnCostInfo> bossSpawnCosts) {
			DirectorBuilder<BossPillarEntity> builder = directorModel.GetDirectorBuilder<BossPillarEntity>();
			builder.SetProperty(new PropertyNameInfo(PropertyName.level_number), level)
				//TODO: set other property base values here
				.SetProperty(new PropertyNameInfo(PropertyName.spawn_boss_cost), bossSpawnCosts);

			return builder.Build();
		}

		protected override void OnEntityStart() {
			levelModel.CurrentLevel.Value.IsInBossFight.RegisterOnValueChanged(OnBossFightStatusChanged)
				.UnRegisterWhenGameObjectDestroyedOrRecycled(gameObject);
		}

		private void OnBossFightStatusChanged(bool arg1, bool status) {
			if (currentInteractiveHint != null) {
				if (!status) {
					currentInteractiveHint.SetHint(ClientInput.Singleton.FindActionInPlayerActionMap("Interact"),
						Localization.Get(interactiveHintLocalizedKey));
				}
				else {
					currentInteractiveHint.SetHint(null, Localization.Get("DEPLOY_ERROR_COMBAT"));
				}
				
			}
		}

		protected override void OnBindEntityProperty() {
			
		}

		public void SetLevelEntity(ILevelEntity levelEntity) {
			LevelEntity = levelEntity;
			levelNumber = levelEntity.GetCurrentLevelCount();
			IEntity ent = OnBuildNewEntity(levelNumber, bossSpawnCosts);
			InitWithID(ent.UUID);
            
			//for some reason we need to do this again
			LevelEntity = levelEntity;
			levelNumber = levelEntity.GetCurrentLevelCount();
			bossSpawnCosts = BoundEntity.GetProperty<ISpawnBossCost>().RealValue.Value;
		}

		public IUnRegister RegisterOnSpawnEnemy(Action<GameObject, IDirectorViewController> onSpawnEnemy) {
			this.onSpawnEnemy += onSpawnEnemy;
			return new DirectorOnSpawnEnemyUnregister(this, onSpawnEnemy);
		}

		public void UnregisterOnSpawnEnemy(Action<GameObject, IDirectorViewController> onSpawnEnemy) {
			this.onSpawnEnemy -= onSpawnEnemy;
		}

		public void SetBossSpawnCosts(Dictionary<CurrencyType, LevelBossSpawnCostInfo> bossSpawnCosts) {
			this.bossSpawnCosts = bossSpawnCosts;
		}

		[field: SerializeField]
		public BoxCollider SpawnSizeCollider { get; protected set; }

		protected void SpawnBoss() {
			
			//onSpawnEnemy?.Invoke
		}

		protected override void OnPlayerPressInteract() {
			base.OnPlayerPressInteract();
			//TODO: is in battle -> can't interact; ui open -> can't interact
			if(levelModel.CurrentLevel.Value.IsInBossFight.Value || UIManager.Singleton.GetPanel<PillarUIViewController>(true) != null) {
				return;
			}

			this.SendCommand(OpenPillarUICommand.Allocate(gameObject,
				BoundEntity.GetProperty<ISpawnBossCost>().RealValue.Value));


		}
	}
}