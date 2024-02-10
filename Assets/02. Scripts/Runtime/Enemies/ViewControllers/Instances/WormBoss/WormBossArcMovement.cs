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
        public float duration = 10f; // Duration in seconds
        private float speed; // Calculated speed based on the duration
        public float jumpHeight = 50f; // Amplitude of the jump

        public override void OnStart()
        {
            progress = 0;
            float minDistance = 50f; // Minimum distance between sample and sample2

            // Generate the first random sample
            Vector3 sample = Random.insideUnitSphere * 50;

            // Generate the second random sample, ensuring it is at least minDistance away from the first sample
            Vector3 sample2;
            do
            {
                sample2 = Random.insideUnitSphere * 50;
            } while (Vector3.Distance(sample, sample2) < minDistance);

            NavMeshHit hit;
            if (NavMesh.SamplePosition(sample, out hit, 15, 1))
            {
                start = hit.position;
            }
            if (NavMesh.SamplePosition(sample2, out hit, 15, 1))
            {
                end = hit.position;
            }

            // Calculate the speed based on the duration
            speed = 1f / duration;
        }

        public override TaskStatus OnUpdate()
        {
            // Interpolate between the start and end positions using a Bezier curve with a jump in the y-axis
            Vector3 newPosition = BezierCurve(start, end, progress, jumpHeight);

            // Set the object's position
            this.gameObject.transform.position = newPosition;

            // Calculate the forward vector based on the direction of movement
            Vector3 direction = BezierCurveDerivative(start, end, progress);
            this.gameObject.transform.forward = direction.normalized;

            // Increment the progress based on time and speed
            progress += Time.deltaTime * speed;

            // Check if the movement has reached the end
            if (progress >= 1f)
            {
                return TaskStatus.Success;
            }

            return TaskStatus.Running;
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
