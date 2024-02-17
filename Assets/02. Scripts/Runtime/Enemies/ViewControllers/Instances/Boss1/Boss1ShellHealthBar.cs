using MikroFramework.BindableProperty;
using Polyglot;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.Enemies.Model.Properties;
using Runtime.UI.HealthBar;

namespace a {
	public class Boss1ShellHealthBar : NormalEnemyHealthBar {
		protected override void OnSetEntity(BindableProperty<HealthInfo> boundHealthProperty, IDamageable entity) {
			base.OnSetEntity(boundHealthProperty, entity);
			nameText.text = Localization.Get("Boss1_shell_name");
		}

		protected override void OnBuffUpdate(IBuff buff, BuffUpdateEventType updateType) {
			return;
		}
	}
}