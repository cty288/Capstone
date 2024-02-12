using Runtime.Enemies;
using Runtime.Utilities.Collision;

namespace _02._Scripts.Runtime.WeaponParts.Model.Instance.BuildBuff.Mineral {
	public class MineralBuffExplosion : BasicExplosion {
		public override bool CheckHit(HitData data) {
			return false;
		}
	}
}