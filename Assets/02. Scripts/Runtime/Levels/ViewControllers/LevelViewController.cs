using System.Collections.Generic;
using _02._Scripts.Runtime.Levels.Models;
using MikroFramework.Architecture;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.ViewControllers.Entities;
using Runtime.Enemies.Model;
using Runtime.Weapons.Model.Builders;
using UnityEngine;

namespace _02._Scripts.Runtime.Levels.ViewControllers {
	public interface ILevelViewController: IEntityViewController {
		public void SetLevelNumber(int levelNumber);
		public ILevelEntity OnBuildNewLevel();

		public void Init();
	}
	public abstract class LevelViewController<T> : AbstractBasicEntityViewController<T>, ILevelViewController
		where  T : class, ILevelEntity, new()  {
		
		[SerializeField] protected List<GameObject> enemies = new List<GameObject>();

		private ILevelModel levelModel;
		private int levelNumber;
		protected override void Awake() {
			base.Awake();
			levelModel = this.GetModel<ILevelModel>();
		}

		protected override IEntity OnBuildNewEntity() {
			LevelBuilder<T> builder = levelModel.GetLevelBuilder<T>();
			return OnInitLevelEntity(builder, levelNumber);
		}

		protected abstract IEntity OnInitLevelEntity(LevelBuilder<T> builder, int levelNumber);


		public void SetLevelNumber(int levelNumber) {
			this.levelNumber = levelNumber;
		}

		public ILevelEntity OnBuildNewLevel() {
			return OnBuildNewEntity() as ILevelEntity;
		}

		public void Init() {
			
		}
	}
}