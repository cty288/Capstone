namespace Runtime.DataFramework.Entities.Weapons
{
    public interface IWeaponEntityModel : IEntityModel<IWeaponEntity>
    {
        WeaponBuilder<T> GetWeaponBuilder<T>(int rarity, bool addToModelOnceBuilt = true)
            where T : class, IWeaponEntity, new();

    }


    public class WeaponEntityModel : EntityModel<IWeaponEntity>, IWeaponEntityModel
    {
        public WeaponBuilder<T> GetWeaponBuilder<T>(int rarity, bool addToModelOnceBuilt = true) where T : class, IWeaponEntity, new()
        {
            WeaponBuilder<T> builder = entityBuilderFactory.GetBuilder<WeaponBuilder<T>, T>(rarity);

            if (addToModelOnceBuilt)
            {
                builder.RegisterOnEntityCreated(OnEntityBuilt);
            }

            return builder;
        }
    }
}