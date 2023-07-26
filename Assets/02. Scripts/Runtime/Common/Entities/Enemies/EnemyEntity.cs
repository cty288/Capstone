using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using _02._Scripts.Runtime.Base.Entity.ClassifiedEntity;
using _02._Scripts.Runtime.Common.Properties;
using _02._Scripts.Runtime.Utilities;
using MikroFramework.BindableProperty;
using MikroFramework.Pool;
using UnityEngine;

public interface IEnemyEntity : IEntity, IHaveCustomProperties {
	public BindableProperty<int> GetDanger();
	public BindableProperty<HealthInfo> GetHealth();
	public BindableList<TasteType> GetTaste();
	public BindableProperty<float> GetVigiliance();

	public BindableProperty<float> GetAttackRange();
	
	public int GetRarity();
}

public abstract class EnemyEntity<T> : AbstractHaveCustomPropertiesEntity, IEnemyEntity where T : EnemyEntity<T>, new() {
	protected override void OnEntityRegisterProperties() {
		RegisterInitialProperty(new Rarity());
		RegisterInitialProperty<IDangerProperty>(new Danger());
		RegisterInitialProperty<IHealthProperty>(new Health());
		RegisterInitialProperty<ITasteProperty>(new Taste());
		RegisterInitialProperty<IVigilianceProperty>(new Vigiliance());
		RegisterInitialProperty<IAttackRangeProperty>(new AttackRange());
		OnEnemyRegisterProperties();
	}
	
	public BindableProperty<int> GetDanger() {
		return GetProperty<IDangerProperty>().RealValue;
	}

	public BindableProperty<HealthInfo> GetHealth() {
		return GetProperty<IHealthProperty>().RealValue;
	}

	public BindableList<TasteType> GetTaste() {
		return GetProperty<ITasteProperty>().RealValues;
	}

	public BindableProperty<float> GetVigiliance() {
		return GetProperty<IVigilianceProperty>().RealValue;
	}

	public BindableProperty<float> GetAttackRange() {
		return GetProperty<IAttackRangeProperty>().RealValue;
	}

	public int GetRarity() {
		return GetProperty<IRarityProperty>().RealValue;
	}


	protected abstract void OnEnemyRegisterProperties();

	public override void OnDoRecycle() {
		SafeObjectPool<T>.Singleton.Recycle(this as T);
	}
}
