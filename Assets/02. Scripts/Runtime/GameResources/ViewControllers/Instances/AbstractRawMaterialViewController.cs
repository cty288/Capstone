using MikroFramework;
using MikroFramework.ActionKit;
using Runtime.DataFramework.Entities;
using Runtime.Enemies.Model.Builders;
using Runtime.GameResources.Model.Base;
using Runtime.GameResources.Model.Builder;
using UnityEngine;

namespace Runtime.GameResources.ViewControllers.Instances {
	public abstract class AbstractRawMaterialViewController<T> : AbstractResourceViewController<T> 
		where  T : class, IRawMaterialEntity, new(){
		protected override IEntity OnInitEntity() {
			this.Delay(2f, () => {
				Debug.Log("Hello");
			});
			
			DelayAction.Allocate(2f, () => {
				Debug.Log("Hello");
			}).Execute();
			
			Sequence.Allocate().AddAction(DelayAction.Allocate(5f, () => {
				Debug.Log("Hello");
			})).AddAction(UntilAction.Allocate(() => {
				return Input.GetKeyDown(KeyCode.Space);
			})).AddAction(DelayAction.Allocate(10f, () => {
				Debug.Log("Hello Again");
			})).Execute();
				
			RawMaterialBuilder<T> builder = entityModel.GetRawMaterialBuilder<T>();
			return OnInitEnemyEntity(builder);
		}

		protected abstract IEntity OnInitEnemyEntity(RawMaterialBuilder<T> builder);
	}
}