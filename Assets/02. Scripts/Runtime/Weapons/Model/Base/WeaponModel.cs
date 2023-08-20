using MikroFramework.Architecture;
using Runtime.DataFramework.Entities;
using Runtime.Weapons.Model.Builders;

namespace Runtime.Weapons.Model.Base
{
    public interface IWeaponModel : IModel, IEntityModel<IWeaponEntity> {
        WeaponBuilder<T> GetWeaponBuilder<T>(bool addToModelOnceBuilt = true)
            where T : class, IWeaponEntity, new();
    }


    public class WeaponModel : EntityModel<IWeaponEntity>, IWeaponModel
    {
        protected override void OnInit() {
            base.OnInit();
        }

        public WeaponBuilder<T> GetWeaponBuilder<T>(bool addToModelOnceBuilt = true) where T : class, IWeaponEntity, new()
        {
            WeaponBuilder<T> builder = entityBuilderFactory.GetBuilder<WeaponBuilder<T>, T>(1);

            if (addToModelOnceBuilt)
            {
                builder.RegisterOnEntityCreated(OnEntityBuilt);
            }

            return builder;
        }
    }
}