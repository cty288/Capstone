using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _02._Scripts.Runtime.Levels.Models;
using _02._Scripts.Runtime.Levels.ViewControllers;
using _02._Scripts.Runtime.Skills.Model.Instance;
using Cysharp.Threading.Tasks;
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
    
    protected override void OnEntityStart() {
			
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
            (r) => r.Collectable && r is RustyPistolEntity).FirstOrDefault();
        
        IResourceEntity weaponEntity = weapon.EntityCreater.Invoke(true, 1);
        inventorySystem.AddItemToNonHotBarSlot(weaponEntity);

        player.AlwaysNonLethal = true;
    }

    private void NextConditionalDialogue() {
        currentConditionalDialogueIndex++;
        if (currentConditionalDialogueIndex >= conditionalDialogueGroups.Length) {
            return;
        }
        HintManager.Singleton.ShowHint(conditionalDialogueGroups[currentConditionalDialogueIndex]);
    }
}
