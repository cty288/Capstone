using _02._Scripts.Runtime.Levels.Models;
using MikroFramework.AudioKit;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Properties.CustomProperties;
using UnityEngine;

namespace _02._Scripts.Runtime.Levels.ViewControllers.Instances.BaseLevel {
	public class BaseLevelEntity : LevelEntity<BaseLevelEntity> {
		[field: ES3Serializable] public override string EntityName { get; set; } = "BaseLevelEntity";

		public override void OnRecycle() {
			base.OnRecycle();
		}

		protected override void OnInitModifiers(int rarity) {
			
		}

		protected override ICustomProperty[] OnRegisterCustomProperties() {
			return null;
		}
	}
	
	public class BaseLevelController : LevelViewController<BaseLevelEntity> {
		
		protected override void OnEntityStart() {
			
		
		}
		
		protected override void OnBindEntityProperty() {
			
		}

		protected override IEntity OnInitLevelEntity(LevelBuilder<BaseLevelEntity> builder, int levelNumber) {
			return builder.Build();
		}
	}
}