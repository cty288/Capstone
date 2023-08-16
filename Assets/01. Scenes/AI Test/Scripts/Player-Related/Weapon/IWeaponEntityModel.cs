namespace Runtime.DataFramework.Entities
{
    /// <summary>
    /// Model for enemies
    /// </summary>
    public interface IWeaponEntityModel : IEntityModel<IWeaponEntity>
    {
        /// <summary>
        /// Get the enemy builder for the entity type
        /// </summary>
        /// <param name="rarity"></param>
        /// <param name="addToModelOnceBuilt"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        // EnemyBuilder<T> GetEnemyBuilder<T>(int rarity, bool addToModelOnceBuilt = true)
        //     where T : class, IEnemyEntity, new();

    }


    public class WeaponEntityModel : EntityModel<IWeaponEntity>, IWeaponEntityModel
    {
        // public EnemyBuilder<T> GetEnemyBuilder<T>(int rarity, bool addToModelOnceBuilt = true) where T : class, IEnemyEntity, new()
        // {
        //     EnemyBuilder<T> builder = entityBuilderFactory.GetBuilder<EnemyBuilder<T>, T>(rarity);

        //     if (addToModelOnceBuilt)
        //     {
        //         builder.RegisterOnEntityCreated(OnEntityBuilt);
        //     }

        //     return builder;
        // }
    }
}