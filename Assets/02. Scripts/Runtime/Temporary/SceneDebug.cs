using _02._Scripts.Runtime.Currency;
using _02._Scripts.Runtime.Currency.Model;
using _02._Scripts.Runtime.Skills.Model.Base;
using _02._Scripts.Runtime.Skills.Model.Instance;
using Framework;
using MikroFramework.Architecture;
using Runtime.Inventory.Model;
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
            // if (Input.GetKeyDown(KeyCode.B))
            // {
            //     boss.TakeDamage(10);
            // }

            if (Input.GetKeyDown(KeyCode.P))
            {
                player.TakeDamage(10);
            }

            
        }
    }
}
