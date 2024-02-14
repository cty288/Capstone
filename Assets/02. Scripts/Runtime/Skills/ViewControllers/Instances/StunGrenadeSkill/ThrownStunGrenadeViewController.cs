using Runtime.Weapons.ViewControllers.Base;

namespace _02._Scripts.Runtime.Skills.ViewControllers.Instances.StunGrenadeSkill {
	public class ThrownStunGrenadeViewController : ThrownGrenadeViewController {
		private float malfunctionBuffTime;
		private float powerlessBuffTime;
		private int powerlessBuffLevel;
		
		public void SetBuffInto(float malfunctionBuffTime, float powerlessBuffTime, int powerlessBuffLevel) {
			this.malfunctionBuffTime = malfunctionBuffTime;
			this.powerlessBuffTime = powerlessBuffTime;
			this.powerlessBuffLevel = powerlessBuffLevel;
		}

		protected override void OnExplosion(IExplosionViewController explosionViewController) {
			base.OnExplosion(explosionViewController);
			StunGrenadeExplosion stunGrenadeExplosion = (StunGrenadeExplosion) explosionViewController;
			stunGrenadeExplosion.SetBuffInto(malfunctionBuffTime, powerlessBuffTime, powerlessBuffLevel);
		}
	}
}