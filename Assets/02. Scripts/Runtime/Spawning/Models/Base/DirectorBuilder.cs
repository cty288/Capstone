using MikroFramework.Pool;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Entities.Builders;
using Runtime.DataFramework.Properties;
using Runtime.Weapons.Model.Builders;

namespace Runtime.Spawning
{
    public class DirectorBuilder<T> : EntityBuilder<DirectorBuilder<T>, T> where T : class, IEntity, new()
    {
        public DirectorBuilder()
        {
            CheckEntity();
        }
        
        public override void RecycleToCache()
        {
            SafeObjectPool<DirectorBuilder<T>>.Singleton.Recycle(this);
        }

        public static DirectorBuilder<T> Allocate(int rarity)
        {
            DirectorBuilder<T> target = SafeObjectPool<DirectorBuilder<T>>.Singleton.Allocate();
            target.SetProperty<int>(new PropertyNameInfo(PropertyName.rarity), rarity);
            return target;
        }
    }
}