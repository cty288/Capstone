using UnityEngine;

namespace _02._Scripts.Runtime.Utilities
{
    public static class MathFunctions {
        public static Vector3 RandomPointInAnnulus(Vector3 origin, float minRadius, float maxRadius){
            var randomDirection = (Random.insideUnitCircle * origin).normalized;
            var randomDistance = Random.Range(minRadius, maxRadius);
            var point = origin + new Vector3(randomDirection.x, 0, randomDirection.y) * randomDistance;
     
            return point;
        }
    }
}