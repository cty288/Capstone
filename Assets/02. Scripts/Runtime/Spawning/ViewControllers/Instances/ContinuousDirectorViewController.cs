using _02._Scripts.Runtime.Levels.Models;
using _02._Scripts.Runtime.Levels.ViewControllers.Instances;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Properties.CustomProperties;
using Runtime.Utilities.ConfigSheet;
using UnityEngine.PlayerLoop;

namespace Runtime.Spawning.ViewControllers.Instances
{
    public class ContinuousDirectorEntity : DirectorEntity<ContinuousDirectorEntity>
    {
        public override void OnRecycle()
        {
        }



        protected override ICustomProperty[] OnRegisterCustomProperties()
        {
            return null;
        }
    }
    
    public class ContinuousDirectorViewController : DirectorViewController<ContinuousDirectorEntity> {
        protected override void OnEntityStart()
        {
        }

        protected override void OnBindEntityProperty()
        {
        }

        protected override IEntity OnInitDirectorEntity(DirectorBuilder<ContinuousDirectorEntity> builder)
        {
            return builder
                .Build();
        }
    }
}