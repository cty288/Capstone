namespace _02._Scripts.Runtime.Utilities {
	public static class NavMeshHelper {
		public static int GetSpawnableAreaMask() {
			int layerMask = ~(1 << 1);
			return layerMask;
		}
	}
}