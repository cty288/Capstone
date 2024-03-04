using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using _02._Scripts.Runtime.CollectableResources.ViewControllers.Base;
using _02._Scripts.Runtime.Currency.Model;
using _02._Scripts.Runtime.Levels.Commands;
using _02._Scripts.Runtime.Levels.DayNight;
using _02._Scripts.Runtime.Levels.Models;
using _02._Scripts.Runtime.Levels.Models.Properties;
using _02._Scripts.Runtime.Levels.Sandstorm;
using _02._Scripts.Runtime.Levels.Systems;
using _02._Scripts.Runtime.Pillars.Models;
using _02._Scripts.Runtime.Pillars.Systems;
using _02._Scripts.Runtime.Rewards;
using _02._Scripts.Runtime.Utilities;
using AYellowpaper.SerializedCollections;
using Cysharp.Threading.Tasks;
using Framework;
using MikroFramework;
using MikroFramework.Architecture;
using MikroFramework.AudioKit;
using MikroFramework.Pool;
using MikroFramework.ResKit;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Properties;
using Runtime.DataFramework.ViewControllers.Entities;
using Runtime.Enemies.Model;
using Runtime.GameResources;
using Runtime.Player;
using Runtime.Spawning;
using Runtime.Spawning.ViewControllers.Instances;
using Runtime.Temporary;
using Runtime.Utilities;
using Runtime.Utilities.ConfigSheet;
using Runtime.Weapons.Model.Builders;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Serialization;
using PropertyName = Runtime.DataFramework.Properties.PropertyName;
using Random = UnityEngine.Random;


namespace _02._Scripts.Runtime.Levels.ViewControllers {
	public interface ILevelViewController: IEntityViewController {
		public int GetLevelNumber();
		public void SetLevelNumber(int levelNumber);
		public ILevelEntity OnBuildNewLevel(int levelNumber);

		public UniTask Init();
		
		public void OnExitLevel();

		public ISubAreaLevelEntity GetCurrentActiveSubAreaEntity();
		
		HashSet<GameObject> Enemies { get; }
	}

	public struct LevelSpawnCard : ICanGetModel {
		[field: ES3Serializable]
		public string TemplateEntityUUID { get; }

		public IEnemyEntity TemplateEntity {
			get {
				return this.GetModel<IEnemyEntityModel>().GetEntity(TemplateEntityUUID);
			}
		}
		
		[field: ES3Serializable]
		public int RealSpawnWeight { get; }
		//[field: ES3Serializable]
		//public int RealSpawnCost { get; }
		[field: ES3Serializable]
		public string[] PrefabNames { get; }
		
		[field: ES3Serializable]
		public bool IsNormalEnemy { get; }
		
		[field: ES3Serializable]
		public int MinRarity { get; }
		
		[field: ES3Serializable]
		public int MaxRarity { get; }
		
		
		

		public List<GameObject> Prefabs =>
			PrefabNames.Select(n => GlobalLevelManager.Singleton.GetEnemyPrefab(n)).ToList();
		
		//GlobalLevelManager.Singleton.GetEnemyPrefab(PrefabName);

		public string EntityName => TemplateEntity.EntityName;

	
		public float GetRealSpawnCost(int level, int rarity) {
			return TemplateEntity.GetRealSpawnCost(level, rarity);
		} 
		
		public LevelSpawnCard(IEnemyEntity templateEntity, int realSpawnWeight, string[] prefabNames, int minRarity, int maxRarity) {
			TemplateEntityUUID = templateEntity.UUID;
			RealSpawnWeight = realSpawnWeight;
			//RealSpawnCost = realSpawnCost;
			PrefabNames = prefabNames;
			IsNormalEnemy = templateEntity is INormalEnemyEntity;
			MinRarity = minRarity;
			MaxRarity = maxRarity;
		}

		public override bool Equals(object obj) {
			if (obj is LevelSpawnCard other) {
				return other.TemplateEntityUUID == TemplateEntityUUID;
			}

			return false;
		}

		public IArchitecture GetArchitecture() {
			return MainGame.Interface;
		}
	}
	
	[Serializable]
	public struct SpawnCardListConfig {
		[FormerlySerializedAs("spawnCardList")] public EnemySpawnInfo[] enemySpawnInfos;
	}

	[Serializable]
	public struct EnemySpawnInfo {
		public GameObject mainPrefab;
		public List<GameObject> variants;
		public int minRarity;
		public int maxRarity;
	}
	
	[RequireComponent(typeof(NavMeshSurface))]
	public abstract class LevelViewController<T> : AbstractBasicEntityViewController<T>, ILevelViewController
		where  T : class, ILevelEntity, new() {

		[Header("Player")] 
		[SerializeField] protected List<Transform> playerSpawnPoints = new List<Transform>();
		
		[FormerlySerializedAs("enemySpawnCardConfig")]
		[FormerlySerializedAs("enemies")]
		[Header("Enemies")]
		[SerializeField] protected List<SpawnCardListConfig> enemySpawnCardConfigs;
		[SerializeField] 
		[SerializedDictionary("Currency Type", "Costs")]
		protected SerializedDictionary<CurrencyType, RewardCostInfo> bossSpawnCostInfo;
		
		[SerializeField]
		protected PillarRewardsInfo pillarRewardsInfo;
		
		[SerializeField] protected bool hasPillars = true;
		[SerializeField] protected string pillarPrefabName = "BossPillar";
		//[SerializeField] protected int pillarCount = 4;
		[SerializeField] protected Collider maxExtent;
		
		
		//[SerializeField] protected List<LevelEnemyPrefabConfig> bosses = new List<LevelEnemyPrefabConfig>();

		[SerializeField] protected GameObject playerSpawner;
		[SerializeField] private float[] sandstormProbability = new[] {0, 0.33f, 1f};

		[SerializeField] private int timeCurrencyLevel = 1;
		[SerializeField] private bool spawnExitDoor = true;
		
		private IGameEventSystem gameEventSystem;

		//mainPrefab + variants
		public HashSet<GameObject> Enemies {
			get {
				HashSet<GameObject> enemyPrefabs = new HashSet<GameObject>();
				
				
				ISubAreaLevelViewController[] subAreas = GetComponentsInChildren<ISubAreaLevelViewController>(true);
				if (subAreas != null) {
					foreach (var subArea in subAreas) {
						enemyPrefabs.UnionWith(subArea.Enemies);
					}
				}
				
				foreach (SpawnCardListConfig spawnCardList in enemySpawnCardConfigs)
				{
					foreach (EnemySpawnInfo info in spawnCardList.enemySpawnInfos)
					{
						enemyPrefabs.Add(info.mainPrefab);
						if (info.variants != null)
						{
							enemyPrefabs.UnionWith(info.variants);
						}
					}
				}
				return enemyPrefabs;
			}
		}
		
		[FormerlySerializedAs("subAreaLevels")] [Header("Sub Areas")]
		public List<GameObject> subAreaLevelPrefabs;

		private List<ISubAreaLevelViewController> subAreaLevels = new List<ISubAreaLevelViewController>();


		// [SerializeField] protected int maxEnemiesBaseValue = 50;

		private List<IEnemyEntity> templateEnemies = new List<IEnemyEntity>();
		private ILevelModel levelModel;
        
		private int levelNumber;
		private NavMeshSurface navMeshSurface;

		private HashSet<IDirectorViewController> playerSpawners = new HashSet<IDirectorViewController>();
		private IBossPillarViewController[] bossPillars;

		 private HashSet<IEntity> currentEnemies = new HashSet<IEntity>();
		[SerializeField] protected bool autoUpdateNavMeshOnStart = true;
		
		[Header("Audio Settings")]
		[SerializeField] private AudioClip ambientMusic;

		[SerializeField] private AudioClip bgm;
		[SerializeField] private float relativeVolume = 1f;
		
		
		private ILevelSystem levelSystem;
		protected AudioSource ambientMusicSource;
		protected override bool CanAutoRemoveEntityWhenLevelEnd { get; } = false;
		protected IGameTimeModel gameTimeModel;

		protected override void Awake() {
			base.Awake();
			levelModel = this.GetModel<ILevelModel>();
			navMeshSurface = GetComponent<NavMeshSurface>();
			levelSystem = this.GetSystem<ILevelSystem>();
			gameTimeModel = this.GetModel<IGameTimeModel>();
			gameEventSystem = this.GetSystem<IGameEventSystem>();
		}

		protected override IEntity OnBuildNewEntity() {
			return OnBuildNewLevel(levelNumber);
		}
	
		
		protected abstract IEntity OnInitLevelEntity(LevelBuilder<T> builder, int levelNumber);

		public int GetLevelNumber() {
			return levelNumber;
		}

		public void SetLevelNumber(int levelNumber) {
			this.levelNumber = levelNumber;
			
		}

		

		private void OnBossSpawned(OnBossSpawned e) {
			OnInitEnemy(e.Boss);
		}

		public List<LevelSpawnCard[]> CreateTemplateEnemies(int levelNumber) {
			List<LevelSpawnCard[]> spawnCards = new List<LevelSpawnCard[]>();

			foreach (var spawnCardList in enemySpawnCardConfigs)
			{
				LevelSpawnCard[] cards = new LevelSpawnCard[spawnCardList.enemySpawnInfos.Length];
				for (int card_index = 0; card_index < cards.Length; card_index++)
				{
					EnemySpawnInfo enemyInfo = spawnCardList.enemySpawnInfos[card_index];
					GameObject prefab = enemyInfo.mainPrefab;
					ICreatureViewController enemyViewController = prefab.GetComponent<ICreatureViewController>();
					IEnemyEntity enemyEntity = enemyViewController.OnInitEntity(levelNumber, 1) as IEnemyEntity;

					string[] prefabNames = new string[(enemyInfo.variants?.Count ?? 0) + 1];
					prefabNames[0] = prefab.name;
					for (int i = 0; i < enemyInfo.variants.Count; i++)
					{
						prefabNames[i + 1] = enemyInfo.variants[i].name;
					}

					templateEnemies.Add(enemyEntity);
					cards[card_index] = new LevelSpawnCard(enemyEntity, enemyEntity.GetRealSpawnWeight(levelNumber),
						prefabNames,
						enemyInfo.minRarity, enemyInfo.maxRarity);
				}
				spawnCards.Add(cards);
			}

			return spawnCards;
		}
		
		public virtual ILevelEntity OnBuildNewLevel(int levelNumber) {
			if (levelModel == null) {
				levelModel = this.GetModel<ILevelModel>();
			}
			LevelBuilder<T> builder = levelModel.GetLevelBuilder<T>();
			builder.SetProperty(new PropertyNameInfo(PropertyName.rarity), levelNumber)
				.SetProperty(new PropertyNameInfo(PropertyName.spawn_cards), CreateTemplateEnemies(levelNumber));
				//.SetProperty(new PropertyNameInfo(PropertyName.sub_area_levels), CreateSubAreaLevels());

			ILevelEntity levelEnity = OnInitLevelEntity(builder, levelNumber) as ILevelEntity;
			return levelEnity;
		}
		
		public List<ISubAreaLevelViewController> CreateSubAreaLevels() {
			List<ISubAreaLevelViewController> subAreaLevels = new List<ISubAreaLevelViewController>();
			
			foreach (var subLevel in this.subAreaLevelPrefabs) {
				ISubAreaLevelViewController subLevelVC = subLevel.GetComponent<ISubAreaLevelViewController>();
				ISubAreaLevelEntity levelEntity = subLevelVC.OnInitEntity();

				subLevelVC.InitWithID(levelEntity.UUID);
				subLevelVC.InitDirectors();
				subAreaLevels.Add(subLevelVC);
				BoundEntity.AddSubArea(levelEntity.UUID);
			}
			
			return subAreaLevels;
		}

		public async UniTask Init() {
			//navMeshSurface.BuildNavMesh();
			//navMeshSurface.navMeshData 
			this.RegisterEvent<OnBossSpawned>(OnBossSpawned).UnRegisterWhenGameObjectDestroyedOrRecycled(gameObject);

			this.RegisterEvent<OnNewDayStart>(OnNewDay).UnRegisterWhenGameObjectDestroyedOrRecycled(gameObject);
			
			
			/*if (BoundEntity.GetCurrentLevelCount() <= 1) {
				gameTimeModel.DayCountThisRound.RegisterWithInitValue(OnNewDay)
					.UnRegisterWhenGameObjectDestroyedOrRecycled(gameObject);
			}
			else {
				gameTimeModel.DayCountThisRound.RegisterOnValueChanged(OnNewDay)
					.UnRegisterWhenGameObjectDestroyedOrRecycled(gameObject);
			}*/
			
			
			subAreaLevels = CreateSubAreaLevels();
			if (autoCreateNewEntityWhenStart) {
				UpdateNavMesh();
			}
			foreach (var subarea in subAreaLevels) {
				subarea.SetLevelNumber(levelNumber);
			}
			
			UpdatePreExistingEnemies();
			OnSpawnPlayer();
			if (ambientMusic) {
				 ambientMusicSource = AudioSystem.Singleton.Play2DSound(ambientMusic, relativeVolume, true);
			}
			
			if (bgm) {
				AudioSystem.Singleton.PlayMusic(bgm);
			}
			//UpdateWallMaterials();
			await UniTask.Yield();
			SpawningUtility.UpdateRefPointsKDTree();
			if (spawnExitDoor) {
				await SpawnLevelExitDoor();
			}
			
			await SpawnPillars();
			UpdatePreExistingDirectors();
			SpawnCollectableResources();
			
			StartCoroutine(UpdateLevelSystemTime());
			
			this.Delay(5f, () => {
				LoadingCanvas.Singleton.Hide();

			});
			
			//this.GetModel<IGamePlayerModel>().GetPlayer().Armor.RegisterOnValueChanged()
		}

		private async UniTask SpawnLevelExitDoor() {

			GameObject door = await SpawningUtility.SpawnExitDoor(gameObject, "LevelExitDoor", maxExtent.bounds,
				playerSpawnPoints.ToArray());
			door.transform.SetParent(transform);
		}


		private HashSet<int> triggeredNewDay = new HashSet<int>();
		private void OnNewDay(OnNewDayStart e) {
			int day = e.DayCount;
			
			if (levelModel.CurrentLevel.Value != BoundEntity) {
				return;
			}
			
			if (triggeredNewDay.Contains(day)) {
				return;
			}
			
			triggeredNewDay.Add(day);
			BoundEntity.DayStayed++; 
			Debug.Log($"This is the {BoundEntity.DayStayed} day in this level");
			if (BoundEntity.DayStayed -1 >= sandstormProbability.Length) {
				return;
			}
			float sandstormProb = sandstormProbability[BoundEntity.DayStayed - 1];
			if (Random.Range(0f, 1f) <= sandstormProb) {
				//spawn sandstorm
				int sandstormHappenTime = 23 * 60;
				gameEventSystem.AddEvent(new SandstormEvent(), sandstormHappenTime);

				int warningTime = sandstormHappenTime / 2;
				gameEventSystem.AddEvent(new SandstormWarningEvent(), warningTime);
			}
			
			// Add Night Events
			// Night occurs at 8pm (20h)
			int nightHappeningTime = (GameTimeModel.NightStartHour - GameTimeModel.NewDayStartHour) * 60;
			gameEventSystem.AddEvent(new NightEvent(), nightHappeningTime);
			
			// Trigger warning 1 in-game hour before night time.
			gameEventSystem.AddEvent(NightWarningEvent.Allocate(60), nightHappeningTime - 60);
			
			// New Day Event
			gameEventSystem.AddEvent(new NewDayEvent(), 0);

			int prevDay = day - 1;
			if (prevDay > 0) {
				int timePointsPerDay =
					int.Parse(ConfigDatas.Singleton.GlobalDataTable.Get<string>("TIME_POINTS_PER_DAY", "Value1"));
				
				int timePointsCost = timePointsPerDay * prevDay;

				string entityName = $"Time{timeCurrencyLevel}";

				foreach (PlayerController controller in PlayerController.GetAllPlayers()) {
					for (int i = 0; i < timePointsCost; i++) {
						ResourceVCFactory.Singleton.AddToInventoryOrSpawnNewVc(entityName, true,
							controller.transform.position,
							true, timeCurrencyLevel);
					}
				}
				
			}
		}

		private void SpawnCollectableResources() {
			CollectableResourceSpawnArea[] collectableResourceViewControllers = GetComponentsInChildren<CollectableResourceSpawnArea>(true);
			float seed = Random.Range(0, 1000000f);
			foreach (CollectableResourceSpawnArea collectableResourceViewController in collectableResourceViewControllers) {
				collectableResourceViewController.Spawn(seed);
			}
		}

		private async UniTask SpawnPillars() {
			IPillarModel pillarModel = this.GetModel<IPillarModel>();
			if (!hasPillars) {
				return;
			}

			int pillarCount = CurrencyType.GetValues(typeof(CurrencyType)).Length;
			List<GameObject> pillars = await SpawningUtility.SpawnBossPillars(gameObject, pillarCount, pillarPrefabName, maxExtent.bounds);
			if (pillars == null) {
				return;
			}
			
			int pillarNumber = 0;
			HashSet<string> ids = new HashSet<string>();
			
			foreach (GameObject pillar in pillars) {
				IBossPillarViewController pillarViewController = pillar.GetComponent<IBossPillarViewController>();
				//pillarViewController.SetBossSpawnCosts(GetBossSpawnCostInfoDict());
				//pillar.transform.SetParent(transform);
				string id = pillarViewController.InitPillar(BoundEntity, bossSpawnCostInfo, pillarRewardsInfo);
				
				ids.Add(id);
				pillarNumber++;
				//InitDirector(pillarViewController);
				//RegisterOnSpawnEnemy(pillarViewController);
			}
			bossPillars = pillars.Select(p => p.GetComponent<IBossPillarViewController>()).ToArray();
			pillarModel.SetCurrentLevelPillars(ids);
			
		}
		private void UpdateWallMaterials() {
			LayerMask wallMask = LayerMask.NameToLayer("Wall");
			//find all colliders with wall layer
			Collider[] colliders = gameObject.GetComponentsInChildren<Collider>(true)
				.Where(c => c.gameObject.layer == wallMask).ToArray();

			PhysicMaterial mat = this.GetUtility<ResLoader>().LoadSync<PhysicMaterial>("Nofric");
			foreach (var collider in colliders) {
				collider.material = mat;
			}
			
		}
		private Dictionary<CurrencyType, RewardCostInfo> GetBossSpawnCostInfoDict() {
			Dictionary<CurrencyType, RewardCostInfo> dict = new Dictionary<CurrencyType, RewardCostInfo>();
			if (bossSpawnCostInfo == null) {
				return dict;
			}
			foreach (var info in bossSpawnCostInfo) {
				dict.Add(info.Key, info.Value);
			}

			return dict;
		}


		private IEnumerator UpdateLevelSystemTime() {
			while (true) {
				yield return new WaitForSeconds(1f);
				levelSystem.OnOneSecondPassed();
			}
		}

		private void UpdatePreExistingDirectors() {
			IDirectorViewController[] directors = GetComponentsInChildren<IDirectorViewController>(true);
			foreach (var directorViewController in directors) {
				InitDirector(directorViewController);
			}
		}
		
		protected void InitDirector(IDirectorViewController director) {
			director.SetLevelEntity(BoundEntity);
			director.SetLevelViewController(this);
		}
		
		protected void RegisterOnSpawnEnemy(IDirectorViewController director) {
			director.RegisterOnSpawnEnemy(OnSpawnEnemy).UnRegisterWhenGameObjectDestroyedOrRecycled(gameObject);
		}

		 private void OnSpawnEnemy(GameObject enemyObject, IDirectorViewController director) {
		 	OnInitEnemy(enemyObject.GetComponent<IEnemyViewController>());
		 }
		
		 private void UpdatePreExistingEnemies() {
		 	IEnemyViewController[] enemies = GetComponentsInChildren<IEnemyViewController>(true);
		 	foreach (var enemy in enemies) {
		 		enemy.RegisterOnEntityViewControllerInit(OnExistingEnemyInit)
		 			.UnRegisterWhenGameObjectDestroyedOrRecycled(gameObject);
		 	}
		 }

		 private void OnExistingEnemyInit(IEntityViewController entity) {
		 	entity.UnRegisterOnEntityViewControllerInit(OnExistingEnemyInit);
		 	OnInitEnemy(entity as IEnemyViewController);
		 }
		
		 private void OnInitEnemy(IEnemyViewController enemyObject) {
		 	IEnemyEntity enemyEntity = enemyObject.EnemyEntity;
		 	enemyEntity.RegisterOnEntityRecycled(OnEnemyEntityRecycled)
		 		.UnRegisterWhenGameObjectDestroyedOrRecycled(gameObject);
		    currentEnemies.Add(enemyEntity);
		 }
		
		 private void OnEnemyEntityRecycled(IEntity enemy) {
		 	enemy.UnRegisterOnEntityRecycled(OnEnemyEntityRecycled);
		 	
		    currentEnemies.Remove(enemy);
		 }

		protected override void Update() {
			base.Update();
			if (Input.GetKeyDown(KeyCode.F6)) {
				UpdateNavMesh();
			}
		}
		
		protected void UpdateNavMesh() {
			
			NavMeshBuildSettings buildSettings = navMeshSurface.GetBuildSettings();
			buildSettings.preserveTilesOutsideBounds = true;
			navMeshSurface.UpdateNavMesh(navMeshSurface.navMeshData, buildSettings);
		}


		protected virtual void OnSpawnPlayer() {
			if (playerSpawnPoints.Count == 0) {
				throw new Exception("No player spawn points found for level {gameObject.name}");
				return;
			}
			this.SendCommand<TeleportPlayerCommand>(
				TeleportPlayerCommand.Allocate(playerSpawnPoints.GetRandomElement().position));

			if (playerSpawner) {
				HashSet<PlayerController> players = PlayerController.GetAllPlayers();
				SafeGameObjectPool pool = GameObjectPoolManager.Singleton.CreatePool(playerSpawner, 10, 100);
				foreach (var player in players) {
					GameObject spawner = pool.Allocate();
					
					
					IDirectorViewController director = spawner.GetComponent<IDirectorViewController>();
					playerSpawners.Add(director);
					InitDirector(director);
					spawner.transform.SetParent(player.transform);
					spawner.transform.localPosition = Vector3.zero;
					spawner.transform.localRotation = Quaternion.identity;
					spawner.transform.localScale = Vector3.one;
					
					foreach (var subArea in subAreaLevels)
					{
						subArea.InitDirector(director);
					}
				}
			}
		}

		public virtual ISubAreaLevelEntity GetCurrentActiveSubAreaEntity()
		{
			if (BoundEntity == null) {
				return null;
			}
			int areaMask = this.GetModel<IPlayerModel>().CurrentSubAreaMask.Value;
			
			// get subarea from level entity
			return BoundEntity.GetAllSubAreaLevels().FirstOrDefault(x => 
				x.GetSubAreaNavMeshModifier() == areaMask);
		}
		
		public override void OnRecycled() {
			base.OnRecycled();
			currentEnemies.Clear();
			playerSpawners.Clear();
			if (ambientMusic && ambientMusicSource) {
				AudioSystem.Singleton.StopSound(ambientMusicSource);
			}
			bossPillars = null;
			subAreaLevels.Clear();
			triggeredNewDay.Clear();
		}
		
		public void OnExitLevel() {
			// BoundEntity.CurrentEnemyCount = 0;
			IEnemyEntityModel enemyModel = this.GetModel<IEnemyEntityModel>();
			IDirectorModel directorModel = this.GetModel<IDirectorModel>();
			IPillarModel pillarModel = this.GetModel<IPillarModel>();
			ISubAreaLevelModel subAreaLevelModel = this.GetModel<ISubAreaLevelModel>();
			
			ISpawnCardsProperty spawnCardsProperty = BoundEntity.GetProperty<ISpawnCardsProperty>();
			foreach (LevelSpawnCard[] spawnCards in spawnCardsProperty.RealValue.Value) {
				foreach (LevelSpawnCard spawnCard in spawnCards) {
					enemyModel.RemoveEntity(spawnCard.TemplateEntityUUID, true);
				}
			}


			foreach (ISubAreaLevelViewController subAreaLevel in subAreaLevels) {
				subAreaLevel.OnExitLevel(); // will also remove all enemies
				subAreaLevelModel.RemoveEntity(subAreaLevel.Entity.UUID,
					true);
			}
			
			while (currentEnemies.Count > 0) {
				IEntity enemy = currentEnemies.First();
				currentEnemies.Remove(enemy);
				enemyModel.RemoveEntity(enemy.UUID, true);
			}

			foreach (IEnemyEntity enemyEntity in templateEnemies) {
				enemyModel.RemoveEntity(enemyEntity.UUID, true);
			}
			templateEnemies.Clear();

			if (bossPillars != null) {
				foreach (var directorViewController in bossPillars) {
					pillarModel.RemoveEntity(directorViewController.Entity.UUID, true);
				}
			}
			
			
		

			foreach (IDirectorViewController spawner in playerSpawners) {
				directorModel.RemoveEntity(spawner.Entity.UUID, true);
			}
			
			
			StopAllCoroutines();
		}

		protected override void OnDestroy() {
			base.OnDestroy();
			OnRecycled();
		}
	}
}