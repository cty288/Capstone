using System.Collections.Generic;
using System.Linq;
using _02._Scripts.Runtime.Currency;
using _02._Scripts.Runtime.Currency.Model;
using _02._Scripts.Runtime.Skills.Model.Base;
using _02._Scripts.Runtime.Skills.Model.Instance;
using Framework;
using MikroFramework.Architecture;
using Runtime.GameResources;
using Runtime.GameResources.Model.Base;
using Runtime.Inventory.Model;
using Runtime.Weapons.Model.Base;
using UnityEngine;

namespace Runtime.Temporary
{
    public class SceneDebug : AbstractMikroController<MainGame>
    {
        public TestEnity boss;
        public TestEnity player;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.M)) {
                ICurrencySystem currencySystem = this.GetSystem<ICurrencySystem>();
                currencySystem.AddCurrency(CurrencyType.Combat, 10);
                currencySystem.AddCurrency(CurrencyType.Mineral, 10);
                currencySystem.AddCurrency(CurrencyType.Plant, 10);
                currencySystem.AddCurrency(CurrencyType.Time, 10);
            }
            
            if (!Application.isEditor) {
                return;
            }
            if (Input.GetKeyDown(KeyCode.B))
            {
                boss.TakeDamage(10);
            }

            if (Input.GetKeyDown(KeyCode.P))
            {
                player.TakeDamage(10);
            }

            if (Input.GetKeyDown(KeyCode.R)) {
                IEnumerable<ResourceTemplateInfo> infos1 =
                    ResourceTemplates.Singleton.GetResourceTemplates(ResourceCategory.WeaponParts);

                IEnumerable<ResourceTemplateInfo> infos2 =
                    ResourceTemplates.Singleton.GetResourceTemplates(ResourceCategory.Weapon,
                        (entity => (entity as IWeaponEntity).GetAttackSpeed().RealValue.Value <= 0.3f));
                
                Debug.Log(infos1.Count());
                Debug.Log(infos2.Count());

                IResourceEntity entity = infos1.First().EntityCreater.Invoke(false, 1);
                Debug.Log(entity.EntityName);
            }
            
        }
    }
}
