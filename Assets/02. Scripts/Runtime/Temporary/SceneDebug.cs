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

            if (Input.GetKeyDown(KeyCode.M)) {
                /*ISkillModel skillModel = this.GetModel<ISkillModel>();
                var skill = skillModel.GetSkillBuilder<TestSkill>(2).FromConfig().Build();
                this.GetModel<IInventoryModel>().AddItem(skill);*/
            }
        }
    }
}
