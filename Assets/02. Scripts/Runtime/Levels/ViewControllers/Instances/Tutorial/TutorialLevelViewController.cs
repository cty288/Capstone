using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _02._Scripts.Runtime.Levels.Models;
using _02._Scripts.Runtime.Levels.ViewControllers;
using _02._Scripts.Runtime.Levels.ViewControllers.Instances.Tutorial;
using _02._Scripts.Runtime.PlayerTasks;
using _02._Scripts.Runtime.Skills.Model.Instance;
using Cysharp.Threading.Tasks;
using MikroFramework;
using MikroFramework.Architecture;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Properties.CustomProperties;
using Runtime.GameResources;
using Runtime.GameResources.Model.Base;
using Runtime.Inventory.Model;
using Runtime.Player;
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

    [SerializeField] private HintMessageGroup[] conditionalDialogueGroups;
    private int currentConditionalDialogueIndex = -1;
    private IInventorySystem inventorySystem;
    private IInventoryModel inventoryModel;
    private IPlayerTaskSystem playerTaskSystem;
    
    protected override void OnEntityStart() {
        inventorySystem = this.GetSystem<IInventorySystem>();
        inventoryModel = this.GetModel<IInventoryModel>();
        playerTaskSystem = this.GetSystem<IPlayerTaskSystem>();
    }

    protected override void OnBindEntityProperty() {
			
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
        
        IInventorySystem inventorySystem = this.GetSystem<IInventorySystem>();
        IResourceEntity entity = skill.EntityCreater.Invoke(true, 1);
        inventorySystem.AddItem(entity);
        
        
        var weapon = ResourceTemplates.Singleton.GetResourceTemplates(ResourceCategory.Weapon,
            (r) => r.Collectable && r is SubMachineGunEntity).FirstOrDefault();
        
        IResourceEntity weaponEntity = weapon.EntityCreater.Invoke(true, 1);
        inventorySystem.AddItemToNonHotBarSlot(weaponEntity);

        player.AlwaysNonLethal = true;
        
        NextConditionalDialogue();
    }

    public void NextConditionalDialogue() {
        currentConditionalDialogueIndex++;
        if (currentConditionalDialogueIndex >= conditionalDialogueGroups.Length) {
            return;
        }
        HintManager.Singleton.ShowHint(conditionalDialogueGroups[currentConditionalDialogueIndex]);
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
        UntilAction action = UntilAction.Allocate(() => inventoryModel.GetSelectedHotBarSlot(HotBarCategory.Right).GetQuantity() > 0);
        action.OnEndedCallback += NextConditionalDialogue;
        action.Execute();
    }
}
