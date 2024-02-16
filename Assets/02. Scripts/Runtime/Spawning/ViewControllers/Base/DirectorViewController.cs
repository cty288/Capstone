using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using _02._Scripts.Runtime.Levels.Models;
using _02._Scripts.Runtime.Levels.ViewControllers;
using _02._Scripts.Runtime.Utilities;
using Cysharp.Threading.Tasks;
using MikroFramework.Architecture;
using MikroFramework.Event;
using MikroFramework.Utilities;
using UnityEngine;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Properties;
using Runtime.DataFramework.Properties.CustomProperties;
using Runtime.DataFramework.ViewControllers.Entities;
using Runtime.Enemies;
using Runtime.Enemies.Model;
using Runtime.Utilities;
using Runtime.Utilities.ConfigSheet;
using UnityEngine.AI;
using UnityEngine.Serialization;
using PropertyName = Runtime.DataFramework.Properties.PropertyName;
using Random = UnityEngine.Random;

namespace Runtime.Spawning
{
    public class DirectorOnSpawnEnemyUnregister : IUnRegister
    {
        private Action<GameObject, IDirectorViewController> onSpawnEnemy;

        private IDirectorViewController director;
		
        public DirectorOnSpawnEnemyUnregister(IDirectorViewController director, Action<GameObject, IDirectorViewController> onSpawnEnemy) {
            this.director = director;
            this.onSpawnEnemy = onSpawnEnemy;
        }

        public void UnRegister() {
            director.UnregisterOnSpawnEnemy(onSpawnEnemy);
            director = null;
        }
    }
    
    public interface IDirectorViewController : IEntityViewController {
        IDirectorEntity DirectorEntity => Entity as IDirectorEntity;

        public void SetLevelEntity(ILevelEntity levelEntity);
        public void SetLevelViewController(ILevelViewController levelViewController);

        public IUnRegister RegisterOnSpawnEnemy(Action<GameObject, IDirectorViewController> onSpawnEnemy);
        
        public void UnregisterOnSpawnEnemy(Action<GameObject, IDirectorViewController> onSpawnEnemy);
    }
    
    public abstract class DirectorViewController<T>  : AbstractBasicEntityViewController<T>, IDirectorViewController where T : class, IDirectorEntity, new() 
    {
        protected IDirectorModel directorModel;
        protected Lottery m_lottery;
        
        protected ILevelEntity LevelEntity;
        protected ILevelViewController LevelVC;
        protected int levelNumber;
        protected Action<GameObject, IDirectorViewController> onSpawnEnemy;
        
        // protected IPlayerModel playerModel;

        [Header("Director Info")]
        [SerializeField] public float baseMinSpawnTimer;
        [SerializeField] public float baseMaxSpawnTimer;
        [SerializeField] public float baseStartingCredits;
        [SerializeField] public float baseCreditsPerSecond;
        [SerializeField] public float baseMaxActiveTime;
        [SerializeField] public float baseDirectorCooldown;
        [SerializeField] public int baseMaxDirectorEnemies;
        [SerializeField] public float minSpawnRange;
        [SerializeField] public float maxSpawnRange;
        [SerializeField] public float addCreditsInterval = 1f;
        
        [Header("Director Don't Edit")]
        private HashSet<IEntity> currentEnemies = new HashSet<IEntity>();
        [SerializeField] private float currentCredits;
        [SerializeField] private float creditTimer = 0f;
        [SerializeField] private float directorSpawnTimer = 0f;
        [SerializeField] private AnimationCurve spawnCurve;
        [SerializeField] private float totalTimeElapsedInDirector = 0f;
        [SerializeField] private int totalEliteEnemies = 0;
        protected override bool CanAutoRemoveEntityWhenLevelEnd { get; } = false;
        
        protected override void Awake() {
            base.Awake();
            directorModel = this.GetModel<IDirectorModel>();
        }

        protected override void OnEntityStart() {
          //  insideArenaCheckPoints =
             //   GameObject.FindGameObjectsWithTag("ArenaRefPoint").Select(x => x.transform.position).ToArray();

            m_lottery = new Lottery();
            // currentCredits = BoundEntity.GetStartingCredits().RealValue;
            creditTimer = addCreditsInterval;
            currentCredits = baseStartingCredits;
        }

        protected override IEntity OnBuildNewEntity() {
            return OnBuildNewEntity(1);
        }
        
        protected IEntity OnBuildNewEntity(int level) {
            DirectorBuilder<T> builder = directorModel.GetDirectorBuilder<T>();
            builder.SetProperty(new PropertyNameInfo(PropertyName.level_number), level)
            //TODO: set other property base values here
            .SetProperty(new PropertyNameInfo(PropertyName.min_spawn_timer), baseMinSpawnTimer)
            .SetProperty(new PropertyNameInfo(PropertyName.max_spawn_timer), baseMaxSpawnTimer)
            .SetProperty(new PropertyNameInfo(PropertyName.credits_per_second), baseCreditsPerSecond)
            .SetProperty(new PropertyNameInfo(PropertyName.starting_credits), baseStartingCredits)
            .SetProperty(new PropertyNameInfo(PropertyName.max_active_time), baseMaxActiveTime)
            .SetProperty(new PropertyNameInfo(PropertyName.director_cooldown), baseDirectorCooldown)
            .SetProperty(new PropertyNameInfo(PropertyName.max_director_enemies), baseMaxDirectorEnemies);
            
            return OnInitDirectorEntity(builder);
        }
        
        protected abstract IEntity OnInitDirectorEntity(DirectorBuilder<T> builder);
        
        protected override void Update() {
            if (BoundEntity == null || String.IsNullOrEmpty(BoundEntity.UUID) || LevelEntity == null) { //not initialized yet
                return;
            }
            base.Update();
            UpdateTimers();
            
            if (creditTimer <= 0f)
            {
                AddCredits();
                creditTimer = addCreditsInterval;
            }

            if(directorSpawnTimer <= 0f)
            {
                AttemptToSpawn();
                
                directorSpawnTimer = Mathf.Lerp(BoundEntity.GetMaxSpawnTimer().RealValue,
                    BoundEntity.GetMinSpawnTimer().RealValue,
                    spawnCurve.Evaluate(totalTimeElapsedInDirector / BoundEntity.GetMaxActiveTime().RealValue));
            }
        }

        public void SetLevelEntity(ILevelEntity levelEntity) {
            LevelEntity = levelEntity;
            levelNumber = levelEntity.GetCurrentLevelCount();
            IEntity ent = OnBuildNewEntity(levelNumber);
            InitWithID(ent.UUID);
            
            //for some reason we need to do this again
            LevelEntity = levelEntity;
            levelNumber = levelEntity.GetCurrentLevelCount();
            
            // RegisterOnSpawnEnemy(OnSpawnEnemy).UnRegisterWhenGameObjectDestroyedOrRecycled(gameObject);
        }
        
        public void SetLevelViewController(ILevelViewController levelViewController) {
            LevelVC = levelViewController;
        }

        public IUnRegister RegisterOnSpawnEnemy(Action<GameObject, IDirectorViewController> onSpawnEnemy) {
            this.onSpawnEnemy += onSpawnEnemy;
            return new DirectorOnSpawnEnemyUnregister(this, onSpawnEnemy);
        }

        public void UnregisterOnSpawnEnemy(Action<GameObject, IDirectorViewController> onSpawnEnemy) {
            this.onSpawnEnemy -= onSpawnEnemy;
        }

        private void UpdateTimers()
        {
            totalTimeElapsedInDirector += Time.deltaTime;
            creditTimer -= Time.deltaTime;
            directorSpawnTimer -= Time.deltaTime;
        }
        
        protected virtual async void AttemptToSpawn()
        {
            // pick card, store the card
            // get subarea index where player is standing
            // int areaMask = playerModel.CurrentSubAreaMask.Value;
            
            // get subarea from level VC
            ISubAreaLevelEntity subArea = LevelVC.GetCurrentActiveSubAreaEntity();
            
            //get cards under cost from subarea
            if (subArea == null)
            {
                // print($"subarea is null");
                return;
            }
            
            //check if area is still active
            if (!subArea.IsActiveSpawner)
            {
                return;
            }
            
            //if over max, return to stop spawning in area
            // if(enemyCount >= subArea.GetMaxEnemyCount())
            //     return;
            List<LevelSpawnCard[]> cards = subArea.GetAllNormalEnemiesUnderCost(currentCredits);
            
            if (cards.Count > 0)
            {
                m_lottery.SetCards(cards);
                LevelSpawnCard[] selectedCard = m_lottery.PickNextCard();

                if (default(LevelSpawnCard).Equals(selectedCard)) {
                    return;
                }

                // bool success = true;
                int maxPackSize = Random.Range(1, 5);
                // while (success && maxPackSize > 0)
                while (maxPackSize > 0)
                {
                    // success = await SpawnEnemy(selectedCard, subArea.GetSubAreaNavMeshModifier());
                    await SpawnEnemy(selectedCard, subArea.GetSubAreaNavMeshModifier());
                    maxPackSize--;
                }
            }
        }

        protected virtual async UniTask<bool> SpawnEnemy(LevelSpawnCard[] cards, int areaMask)
        {
            int rarity = 0;
            float[] costs = new float[cards.Length];
            
            // Check if can spawn elite
            if (cards.Length == 1)
            {
                // int[] rarity = {5};
                // float[] cost = {cards[0].GetRealSpawnCost(levelNumber, rarity[0])};
                rarity = Random.Range(5, 9);
                costs[0] = cards[0].GetRealSpawnCost(levelNumber, rarity);
                bool isElite = true;

                if (currentCredits > costs[0]
                    && totalEliteEnemies <= BoundEntity.GetMaxEliteEnemies().RealValue 
                    && Random.Range(0, 100) < 50)
                {
                    return await SpawnHelper(cards[0], areaMask, rarity, costs[0], isElite);
                }
            }
            
            // NORMAL SPAWN
            int maxRarity = cards.Select(c => c.MaxRarity).Max();
            int minRarity = cards.Select(c => c.MinRarity).Min();
            for(int r = maxRarity; r >= minRarity; r--)
            {
                float checkCost = cards.Sum(c => c.GetRealSpawnCost(levelNumber, r));
                if (currentCredits > checkCost)
                {
                    rarity = r;
                    for (int c = 0; c < cards.Length; c++)
                    {
                        costs[c] = cards[c].GetRealSpawnCost(levelNumber, r);
                    }
                    break;
                }
            }
            
            List<UniTask> tasks = new List<UniTask>();
            for (int i = 0; i < cards.Length; i++)
            {
                tasks.Add(SpawnHelper(cards[i], areaMask, rarity, costs[i], false));
            }
            
            await UniTask.WhenAll(tasks);
            return true;
        }

        private async UniTask<bool> SpawnHelper(LevelSpawnCard card, int areaMask, int rarity, float cost, bool isElite)
        {
            //raycast down from random point within min/max range
            int spawnAttempts = 5;
            while (currentCredits > cost && spawnAttempts > 0)
            {
                float angle = Random.Range(0, 360); 
                float radius = Random.Range(minSpawnRange, maxSpawnRange);

                float x = transform.position.x + radius * Mathf.Cos(angle * Mathf.Deg2Rad);
                float z = transform.position.z + radius * Mathf.Sin(angle * Mathf.Deg2Rad);

                Vector3 spawnPos = new Vector3(
                    x, 
                    transform.position.y, //+ 500f,  
                    z
                );
                
                NavMesh.SamplePosition(spawnPos, out NavMeshHit hitNavMesh, Mathf.Infinity, NavMesh.AllAreas);
                spawnPos = hitNavMesh.position;

                GameObject prefabToSpawn = card.Prefabs[Random.Range(0, card.Prefabs.Count)];
                NavMeshFindResult findResult =
                    await SpawningUtility.FindNavMeshSuitablePosition(
                        gameObject, () => prefabToSpawn.GetComponent<ICreatureViewController>().SpawnSizeCollider,
                        spawnPos, 90, NavMeshHelper.GetSpawnableAreaMask(), default, 1f, 3f,
                        spawnAttempts);
        
                spawnAttempts -= findResult.UsedAttempts;
               
                if (findResult.IsSuccess) {
                    GameObject spawnedEnemy = CreatureVCFactory.Singleton.SpawnCreatureVC(prefabToSpawn, findResult.TargetPosition, 
                        Quaternion.Euler(0, Random.Range(0, 360), 0),
                        null, rarity,
                        levelNumber, true, 5, 30);
                    IEnemyViewController enemyVC = spawnedEnemy.GetComponent<IEnemyViewController>();
                    IEnemyEntity enemyEntity = enemyVC.EnemyEntity;
                    enemyEntity.SpawnedAreaIndex = areaMask;
                    if (isElite) {
                        totalEliteEnemies++;
                        enemyEntity.IsElite.Value = true;
                    }
                    onSpawnEnemy?.Invoke(spawnedEnemy, this);
                    currentCredits -= cost;
                    return true;
                }
            }
            return false;
        }
        
        protected virtual void AddCredits()
        {
            currentCredits += BoundEntity.GetCreditsPerSecond().RealValue;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, minSpawnRange);
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, maxSpawnRange);
        }


        public override void OnRecycled() {
            base.OnRecycled();
            
            creditTimer = addCreditsInterval;
            currentCredits = baseStartingCredits;
            directorSpawnTimer = 0f;
            totalTimeElapsedInDirector = 0f;
            
            IEnemyEntityModel enemyModel = this.GetModel<IEnemyEntityModel>();
            while (currentEnemies.Count > 0) {
                IEntity enemy = currentEnemies.First();
                currentEnemies.Remove(enemy);
                enemyModel.RemoveEntity(enemy.UUID, true);
            }
            currentEnemies.Clear();
            
            onSpawnEnemy = null;
            m_lottery = null;
            LevelEntity = null;
        }
    }
}