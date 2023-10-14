using Framework;
using MikroFramework.Architecture;
using Runtime.DataFramework.Entities;
using Runtime.GameResources.Model.Base;

namespace Runtime.Spawning
{
    public interface IDirectorModel : IEntityModel, ISavableModel {
        DirectorBuilder<T> GetDirectorBuilder<T>(bool addToModelOnceBuilt = true)
            where T : class, IDirectorEntity, new();
    }


    public class DirectorModel : EntityModel<IDirectorEntity>, IDirectorModel
    {
        public DirectorBuilder<T> GetDirectorBuilder<T>(bool addToModelOnceBuilt = true) where T : class, IDirectorEntity, new()
        {
            DirectorBuilder<T> builder = entityBuilderFactory.GetBuilder<DirectorBuilder<T>, T>(1);

            if (addToModelOnceBuilt) {
                builder.RegisterOnEntityCreated(OnEntityBuilt);
            }

            return builder;
        }
    }
}