using System.Collections;
using MikroFramework.Architecture;
using UnityEngine;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Properties.CustomProperties;
using Runtime.DataFramework.ViewControllers.Entities;
using Runtime.Enemies.Model;
using Runtime.Utilities.ConfigSheet;

namespace Runtime.Spawning
{
    public interface IDirectorViewController : IEntityViewController {
        IDirectorEntity DirectorEntity => Entity as IDirectorEntity;
    }
    
    public abstract class AbstractDirectorViewController<T>  : AbstractBasicEntityViewController<DirectorEntity> where  T : class, IDirectorEntity, new() 
    {
        protected IDirectorModel directorModel;
        
        protected override void Awake() {
            base.Awake();
            directorModel = this.GetModel<IDirectorModel>();
        }
        
        protected override IEntity OnBuildNewEntity()
        {
            DirectorBuilder<T> builder = directorModel.GetDirectorBuilder<T>();
            return OnInitDirectorEntity(builder);
        }
        
        protected abstract IEntity OnInitDirectorEntity(DirectorBuilder<T> builder);

        protected override void OnEntityStart()
        {
            throw new System.NotImplementedException();
        }

        protected override void OnBindEntityProperty()
        {
            throw new System.NotImplementedException();
        }
    }
}