using System;
using MikroFramework.Pool;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Tags;
using Runtime.DataFramework.Properties;
using Runtime.DataFramework.Properties.TagProperty;
using Runtime.Utilities.ConfigSheet;

namespace Runtime.DataFramework.Entities.Builders {
    public abstract class EntityBuilder<TBuilder, TEntity> : IPoolable 
        where TEntity : class, IEntity, new() 
        where TBuilder : EntityBuilder<TBuilder, TEntity>{
        protected virtual TEntity Entity { get; set; } = null;
    
        protected Action<TEntity> onEntityCreated = null;
        public EntityBuilder() {
       
        }
    
        public TBuilder RegisterOnEntityCreated(Action<TEntity> onCreated) {
            this.onEntityCreated += onCreated;
            return (TBuilder) this;
        }

        protected void CheckEntity() {
            if (Entity == null) {
                Entity = SafeObjectPool<TEntity>.Singleton.Allocate();
            }
        }
        
        public TBuilder OverrideName(string name) {
            CheckEntity();
            Entity.EntityName = name;
            return (TBuilder) this;
        }

        public TBuilder FromConfig(ConfigTable overrideTable = null) {
            CheckEntity();
            Entity.LoadPropertyBaseValueFromConfig(overrideTable);
            return (TBuilder) this;
        }

        /// <summary>
        /// Override the property's base value and its modifier
        /// If the entity has multiple properties with the same name, every property will be overriden
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        [Obsolete("Use SetProperty with no modifier instead. To set modifier, do this in entity's own class.")]
        public TBuilder SetProperty<ValueType>(PropertyNameInfo propertyName, ValueType value, IPropertyDependencyModifier<ValueType> modifier = null) {
            CheckEntity();
            Entity.SetPropertyBaseValue(propertyName, value, modifier);
            return (TBuilder) this;
        }
        
        public TBuilder SetProperty<ValueType>(PropertyNameInfo propertyName, ValueType value) {
            CheckEntity();
            Entity.SetPropertyBaseValue(propertyName, value);
            return (TBuilder) this;
        }
    
        /// <summary>
        /// Override the property's modifier
        /// If the entity has multiple properties with the same name, every property will be overriden
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="modifier"></param>
        /// <typeparam name="ValueType"></typeparam>
        /// <returns></returns>
        [Obsolete("This is obsolete. To set modifier, do this in entity's own class.")]
        public TBuilder SetModifier<ValueType>(PropertyNameInfo propertyName, IPropertyDependencyModifier<ValueType> modifier) {
            CheckEntity();
            Entity.SetPropertyModifier(propertyName, modifier);
            return (TBuilder) this;
        }
    
        /// <summary>
        /// Set dependencies for a property
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="dependencies"></param>
        /// <returns></returns>
        public TBuilder SetDependencies(PropertyNameInfo propertyName, PropertyNameInfo[] dependencies) {
            CheckEntity();
            Entity.GetProperty(propertyName).SetDependentProperties(dependencies);
            return (TBuilder) this;
        }
        
        public TBuilder AddTag(TagName tag, int level) {
            CheckEntity();
            if (Entity.HasProperty(new PropertyNameInfo(PropertyName.tags))) {
                Entity.GetProperty<ITagProperty>().BaseValue.Add(tag, level);
            }
            return (TBuilder) this;
        }

        public TEntity Build() {
            CheckEntity();
            TEntity ent = this.Entity;
            this.Entity = null;
            ent.OnAllocate();
            ent.OnAwake();
            ent.Initialize();
            onEntityCreated?.Invoke(ent);
            RecycleToCache();
            ent.OnStart(false);
            return ent;
        }

        public void OnRecycled() {
            onEntityCreated = null;
        }

        public bool IsRecycled { get; set; }
        public abstract void RecycleToCache();
    
    }
}
