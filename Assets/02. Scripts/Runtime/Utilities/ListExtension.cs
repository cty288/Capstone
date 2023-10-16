using System.Collections.Generic;

namespace _02._Scripts.Runtime.Utilities {
	public static class ListExtension {
		public static T GetRandomElement<T>(this List<T> list) {
			return list[UnityEngine.Random.Range(0, list.Count)];
		}

		public static void Shuffle<T>(this List<T> list) {
			for (int i = 0; i < list.Count; i++) {
				T temp = list[i];
				int randomIndex = UnityEngine.Random.Range(i, list.Count);
				list[i] = list[randomIndex];
				list[randomIndex] = temp;
			}
		}
		
	}
}