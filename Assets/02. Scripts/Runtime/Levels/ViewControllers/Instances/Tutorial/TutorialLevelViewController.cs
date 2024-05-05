using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _02._Scripts.Runtime.Currency;
using _02._Scripts.Runtime.Currency.Model;
using _02._Scripts.Runtime.Levels.Models;
using _02._Scripts.Runtime.Levels.ViewControllers;
using _02._Scripts.Runtime.Levels.ViewControllers.Instances.Tutorial;
using _02._Scripts.Runtime.Pillars.Models;
using _02._Scripts.Runtime.PlayerTasks;
using _02._Scripts.Runtime.Skills.Model.Base;
using _02._Scripts.Runtime.Skills.Model.Instance;
using Cysharp.Threading.Tasks;
using MikroFramework;
using MikroFramework.Architecture;
using MikroFramework.UIKit;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Properties.CustomProperties;
using Runtime.GameResources;
using Runtime.GameResources.Model.Base;
using Runtime.Inventory.Model;
using Runtime.Player;
using Runtime.Spawning;
using Runtime.Spawning.ViewControllers.Instances;
using Runtime.UI;
using Runtime.Utilities;
using Runtime.Weapons;
using UnityEngine;

public class TutorialLevelEntity : LevelEntity<TutorialLevelEntity> {
    [field: ES3Serializable] public override string EntityName { get; set; } = "TutorialLevelEntity";

    public bool CanOpenInventory { get; set; } = false;

    public override void OnRecycle() {
        base.OnRecycle();
    }

    protected override void OnInitModifiers(int rarity) {
			
    }

    protected override ICustomProperty[] OnRegisterCustomProperties() {
        return null;
    }
}
public class TutorialLevelViewController : LevelViewController<TutorialLevelEntity> {

   
    [SerializeField] private GameObject[] enemyGroups;
    [SerializeField] private Collider pillarTrigger;
    [SerializeField] private GameObject tutorialPillar;
    
    private IInventorySystem inventorySystem;
    private IInventoryModel inventoryModel;
    private IPlayerTaskSystem playerTaskSystem;
    private ISkillEntity initialSkill = null;
    private ICurrencySystem currencySystem;
    
    protected override void OnEntityStart() {
        inventorySystem = this.GetSystem<IInventorySystem>();
        inventoryModel = this.GetModel<IInventoryModel>();
        playerTaskSystem = this.GetSystem<IPlayerTaskSystem>();
        currencySystem = this.GetSystem<ICurrencySystem>();
        this.RegisterEvent<OnTutorialTaskFinish>(OnTutorialTaskFinish)
            .UnRegisterWhenGameObjectDestroyedOrRecycled(gameObject);
        this.RegisterEvent<OnSpawnEnemyGroup>(OnSpawnEnemyGroup)
            .UnRegisterWhenGameObjectDestroyedOrRecycled(gameObject);
        MainUI.Singleton.ShowBlackScreen(true);
    }

    private void OnSpawnEnemyGroup(OnSpawnEnemyGroup e) {
        enemyGroups[e.Index].SetActive(true);
    }

    private void OnTutorialTaskFinish(OnTutorialTaskFinish e) {
       
        if (e.TaskID == 12) {
            pillarTrigger.enabled = true;
            pillarTrigger.gameObject.GetComponent<BossPillarViewController>().SetCanInteract(true);
        }
        else {
            
        }
        NextConditionalDialogue();
    }

    protected override void OnBindEntityProperty() {
			
    }

    protected override void SpawnPillars() {
        GameObject pillar = tutorialPillar;
        IBossPillarViewController pillarViewController = pillar.GetComponent<IBossPillarViewController>();
        string id = pillarViewController.InitPillar(BoundEntity, bossSpawnCostInfo, pillarRewardsInfo);
    }

    protected override IEntity OnInitLevelEntity(LevelBuilder<TutorialLevelEntity> builder, int levelNumber) {
        return builder
            .Build();
    }

    public async override UniTask Init() {
        await base.Init();
        IPlayerEntity player = this.GetModel<IGamePlayerModel>().GetPlayer();
        
        var skill = ResourceTemplates.Singleton.GetResourceTemplates(ResourceCategory.Skill,
                (r) => r.Collectable && r is MedicalNeedleSkill).FirstOrDefault();

        player.SetHealth(80);
        IInventorySystem inventorySystem = this.GetSystem<IInventorySystem>();
        initialSkill = skill.EntityCreater.Invoke(true, 1) as ISkillEntity;
        initialSkill.AdditionalSkillSwitchLocker.Retain();
        inventorySystem.AddItem(initialSkill, false);
        
        
        var weapon = ResourceTemplates.Singleton.GetResourceTemplates(ResourceCategory.Weapon,
            (r) => r.Collectable && r is SubMachineGunEntity).FirstOrDefault();
        
        IResourceEntity weaponEntity = weapon.EntityCreater.Invoke(true, 1);
        inventorySystem.AddItemToNonHotBarSlot(weaponEntity, false);

        player.AlwaysNonLethal = true;
        
        await UniTask.WaitForSeconds(3);
        
        NextConditionalDialogue();
    }

   

    public void OnOpeningDone() {
        OpeningDone();
    }

    private async UniTask OpeningDone() {
        await UniTask.WaitForSeconds(1f);
        MainUI.Singleton.HideBlackScreen();
        await UniTask.WaitForSeconds(1f);
        NextConditionalDialogue();
    }
    public void Step2MoveTask() {
        playerTaskSystem.AddTask(new Step2MoveTask());
    }
    
    public void Step3Task() {
        playerTaskSystem.AddTask(new Step3Task());
    }
    public void Step5Task() {
        playerTaskSystem.AddTask(new Step5Task());
    }
    
    public void Step6Task() {
        playerTaskSystem.AddTask(new Step6Task());
    }
    
    public void OnStep8Finish() {
        BoundEntity.CanOpenInventory = true;
        /*UntilAction action = UntilAction.Allocate(() => inventoryModel.GetSelectedHotBarSlot(HotBarCategory.Right).GetQuantity() > 0);
        action.OnEndedCallback += NextConditionalDialogue;
        action.Execute();*/
        playerTaskSystem.AddTask(new Step8Task());
    }
    
    public void Step9Task() {
        playerTaskSystem.AddTask(new Step9Task());
    }
    
    public void Step11Task() {
        playerTaskSystem.AddTask(new Step11Task());
    }
    
    public void Step12Task() {
        playerTaskSystem.AddTask(new Step12Task(5));
    }
    
    public void Step13Task() {
        playerTaskSystem.AddTask(new Step13Task());
    }
    
    public void Step14Task() {
        playerTaskSystem.AddTask(new Step14Task());
    }

    public void AllowUseSkill(bool allow) {
        if (allow) {
            initialSkill.AdditionalSkillSwitchLocker.Release(); 
        }
        else {
            initialSkill.AdditionalSkillSwitchLocker.Retain();
        }
        
    }
    
    public void Step16Task() {
        currencySystem.AddCurrency(CurrencyType.Plant, 5);
        playerTaskSystem.AddTask(new Step16Task());
    }
    
    public void Step17Task() {
        currencySystem.AddCurrency(CurrencyType.Plant, 20);
        currencySystem.AddCurrency(CurrencyType.Time, 20);
        playerTaskSystem.AddTask(new Step17Task());
    }
    
    public void Step18Task() {
        playerTaskSystem.AddTask(new Step18Task());
    }
}
