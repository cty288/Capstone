using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using _02._Scripts.Runtime.Utilities;
using BehaviorDesigner.Runtime;
using Cysharp.Threading.Tasks;
using Runtime.BehaviorDesigner.Tasks.EnemyAction;
using Runtime.DataFramework.ViewControllers.Entities;
using Runtime.Spawning;
using System.Threading.Tasks;
using TaskStatus = BehaviorDesigner.Runtime.Tasks.TaskStatus;

namespace Runtime.BehaviorDesigner.Tasks.EnemyAction
{
    public partial class WormBossArcMovement : EnemyAction
    {
        private Vector3 start;
        private Vector3 end;
        private float progress = 0f;
        public float duration = 8f; // Duration in seconds
        private float speed; // Calculated speed based on the duration
        public float jumpHeight = 50f; // Amplitude of the jump
        private bool endDive = false;

        private Vector3 direction;
        
        public override void OnStart()
        {
            EnableColliders(true);

            progress = 0;
            float minDistance = 50f; // Minimum distance between sample and sample2

            // Generate the first random sample
            Vector3 sample = this.transform.position + Random.insideUnitSphere * 70;
            Debug.Log(sample);
            // Generate the second random sample, ensuring it is at least minDistance away from the first sample
            Vector3 sample2;
            do
            {
                // sample2 = this.transform.position + Random.insideUnitSphere * 70;
                sample2 = RandomPointInAnnulus(sample, 40, 70);
                Debug.Log(sample2);
            } while (Vector3.Distance(sample, sample2) < minDistance);

            NavMeshHit hit;
            if (NavMesh.SamplePosition(sample, out hit, 15, 1))
            {
                start = hit.position;
                Debug.Log(start);
            }
            if (NavMesh.SamplePosition(sample2, out hit, 15, 1))
            {
                end = hit.position;
                Debug.Log(end);
            }

            // Calculate the speed based on the duration
            speed = 1f / duration;
        }

        private void EnableColliders(bool enable)
        {
            Collider[] colliders = gameObject.GetComponents<Collider>();
            foreach (var c in colliders)
            {
                if (c == null) continue;
                if (!c.isTrigger)
                    c.enabled = enable;
            }
        }

        public Vector3 RandomPointInAnnulus(Vector2 origin, float minRadius, float maxRadius){
     
            var randomDirection = (Random.insideUnitCircle * origin).normalized;
     
            var randomDistance = Random.Range(minRadius, maxRadius);
     
            var point = origin + randomDirection * randomDistance;
     
            return new Vector3(point.x, 0, point.y);
        }

        
        public override TaskStatus OnUpdate()
        {
            if(endDive) {
                return TaskStatus.Success;
            }
            
            // Check if the movement has reached the end
            if (progress >= 1f)
            {
                StartCoroutine(MoveUnderGround());
                return TaskStatus.Running;
            }
            
            // Interpolate between the start and end positions using a Bezier curve with a jump in the y-axis
            Vector3 newPosition = BezierCurve(start, end, progress, jumpHeight);

            // Calculate the forward vector based on the difference between the new position and the current position
            direction = newPosition - transform.position;

            // If there's a significant change in position
            if (direction.magnitude > 0.01f)
            {
                // Set the object's rotation to look in the direction of movement
                transform.rotation = Quaternion.LookRotation(direction);
            }

            // Set the object's position
            transform.position = newPosition;

            // Increment the progress based on time and speed
            progress += Time.deltaTime * speed;

            return TaskStatus.Running;
        }


        public override void OnEnd()
        {
            base.OnEnd();
            
            EnableColliders(false);
        }

        private IEnumerator MoveUnderGround()
        {
            float time = 0;
            float duration = 8f;
            while (time < duration)
            {
                time += Time.deltaTime;
                Vector3 dir = direction.normalized;
                transform.position += dir * Time.deltaTime * speed;    
                yield return null;
            }

            endDive = true;
        }
        
        
        // Bezier curve calculation with a jump in the y-axis
        private Vector3 BezierCurve(Vector3 p0, Vector3 p1, float t, float jumpHeight)
        {
            float u = 1f - t;
            float tt = t * t;
            float uu = u * u;
            float uuu = uu * u;
            float ttt = tt * t;

            Vector3 p = uuu * p0;
            p += 3f * uu * t * p0;
            p += 3f * u * tt * p1;
            p += ttt * p1;

            // Add jump height to the y-axis using a sine wave
            p.y += jumpHeight * Mathf.Sin(t * Mathf.PI);

            return p;
        }

        // Derivative of the Bezier curve (used for calculating forward vector)
        private Vector3 BezierCurveDerivative(Vector3 p0, Vector3 p1, float t)
        {
            float u = 1f - t;
            float tt = t * t;
            float uu = u * u;

            Vector3 derivative = -3f * uu * (p0 - p1) + 6f * u * t * (p0 - p1) + 3f * tt * (p1 - p0);

            return derivative.normalized;
        }
    }
}
