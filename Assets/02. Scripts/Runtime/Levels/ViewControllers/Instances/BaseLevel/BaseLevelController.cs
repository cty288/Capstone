using _02._Scripts.Runtime.Levels.Models;
using _02._Scripts.Runtime.PlayerTasks;
using Cysharp.Threading.Tasks;
using MikroFramework.Architecture;
using MikroFramework.AudioKit;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Properties.CustomProperties;
using UnityEngine;

namespace _02._Scripts.Runtime.Levels.ViewControllers.Instances.BaseLevel {
	public class BaseLevelEntity : LevelEntity<BaseLevelEntity> {
		[field: ES3Serializable] public override string EntityName { get; set; } = "BaseLevelEntity";
		
		public override bool HasRandomBossEncounter => false;

		public override void OnRecycle() {
			base.OnRecycle();
		}

		protected override void OnInitModifiers(int rarity) {
			
		}

		protected override ICustomProperty[] OnRegisterCustomProperties() {
			return null;
		}
	}
	
	public class BaseLevelController : LevelViewController<BaseLevelEntity> {
		private IPlayerTaskSystem playerTaskSystem;
		protected override void Awake() {
			base.Awake();
			playerTaskSystem = this.GetSystem<IPlayerTaskSystem>();
		}

		protected override void OnEntityStart() {
			
		}
		
		protected override void OnBindEntityProperty() {
			
		}

		protected override IEntity OnInitLevelEntity(LevelBuilder<BaseLevelEntity> builder, int levelNumber) {
			return builder.Build();
		}

		public override async UniTask Init() {
			await base.Init();
			NextConditionalDialogue();
			levelModel.BaseTutorialStatus.IntroTriggered = true;
		}

		public void OnBaseIntroFinish() {
			playerTaskSystem.AddTask(new NPCTalkTask("NPC_GUNSMITH"));
			playerTaskSystem.AddTask(new NPCTalkTask("NPC_SKILL"));
		}
	}
}