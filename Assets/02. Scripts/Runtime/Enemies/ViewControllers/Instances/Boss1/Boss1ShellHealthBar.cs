using MikroFramework.BindableProperty;
using Polyglot;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.Enemies.Model.Properties;
using Runtime.UI.HealthBar;

namespace a {
	public class Boss1ShellHealthBar : NormalEnemyHealthBar {
		public override void OnSetEntity(BindableProperty<HealthInfo> boundHealthProperty, IDamageable entity) {
			base.OnSetEntity(boundHealthProperty, entity);
			nameText.text = Localization.Get("Boss1_shell_name");
		}
	}
}