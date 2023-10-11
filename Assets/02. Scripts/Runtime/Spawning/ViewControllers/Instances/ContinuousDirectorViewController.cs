using _02._Scripts.Runtime.Levels.Models;
using _02._Scripts.Runtime.Levels.ViewControllers.Instances;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Properties.CustomProperties;
using Runtime.Utilities.ConfigSheet;

namespace Runtime.Spawning.ViewControllers.Instances
{
    public class ContinuousDirectorEntity : DirectorEntity<ContinuousDirectorEntity>
    {
        public override string EntityName { get; set; }
        protected override ConfigTable GetConfigTable()
        {
            throw new System.NotImplementedException();
        }

        protected override void OnEntityStart(bool isLoadedFromSave)
        {
            throw new System.NotImplementedException();
        }

        public override void OnRecycle()
        {
            throw new System.NotImplementedException();
        }

        protected override string OnGetDescription(string defaultLocalizationKey)
        {
            throw new System.NotImplementedException();
        }

        protected override void OnInitModifiers(int rarity)
        {
            throw new System.NotImplementedException();
        }

        protected override void OnEntityRegisterAdditionalProperties()
        {
            throw new System.NotImplementedException();
        }

        protected override ICustomProperty[] OnRegisterCustomProperties()
        {
            throw new System.NotImplementedException();
        }
    }
    
    public class ContinuousDirectorViewController : DirectorEntity<ContinuousDirectorEntity> {
        
    }
}