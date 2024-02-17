using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using BehaviorDesigner.Runtime.Tasks.Movement;
using UnityEngine;

public class TurretCanSeeObjects : Action {
	 [BehaviorDesigner.Runtime.Tasks.Tooltip("Should the 2D version be used?")]
        [UnityEngine.Serialization.FormerlySerializedAs("usePhysics2D")]
        public bool m_UsePhysics2D;
        [BehaviorDesigner.Runtime.Tasks.Tooltip("Specifies the type of detection that should be used.")]
        public SharedDetectionMode m_DetectionMode = DetectionMode.Object | DetectionMode.ObjectList | DetectionMode.Tag | DetectionMode.LayerMask;
        [BehaviorDesigner.Runtime.Tasks.Tooltip("If using the Object detection mode, specifies the target object that is being searched.")]
        [UnityEngine.Serialization.FormerlySerializedAs("targetObject")]
        public SharedGameObject m_TargetObject;
        [BehaviorDesigner.Runtime.Tasks.Tooltip("If using the Target Objects detection mode, specifies the objects that are being searched.")]
        [UnityEngine.Serialization.FormerlySerializedAs("targetObjects")]
        public SharedGameObjectList m_TargetObjects;
        [BehaviorDesigner.Runtime.Tasks.Tooltip("If using the Tag detection mode, specifies the tag of the objects that are being searched.")]
        [UnityEngine.Serialization.FormerlySerializedAs("targetTag")]
        public SharedString m_TargetTag;
        [BehaviorDesigner.Runtime.Tasks.Tooltip("If using the LayerMask detection mode, specifies the LayerMask of the objects that are being searched.")]
        [UnityEngine.Serialization.FormerlySerializedAs("objectLayerMask")]
        public SharedLayerMask m_TargetLayerMask;
        [BehaviorDesigner.Runtime.Tasks.Tooltip("If using the LayerMask detection mode, specifies the maximum number of colliders that the physics cast can collide with.")]
        [UnityEngine.Serialization.FormerlySerializedAs("maxCollisionCount")]
        public int m_MaxCollisionCount = 200;
        [BehaviorDesigner.Runtime.Tasks.Tooltip("The LayerMask of the objects to ignore when performing the line of sight check.")]
        [UnityEngine.Serialization.FormerlySerializedAs("ignoreLayerMask")]
        public LayerMask m_IgnoreLayerMask;
        [BehaviorDesigner.Runtime.Tasks.Tooltip("The field of view angle of the agent (in degrees).")]
        [UnityEngine.Serialization.FormerlySerializedAs("fieldOfViewAngle")]
        public SharedFloat m_FieldOfViewAngle = 90;
        [BehaviorDesigner.Runtime.Tasks.Tooltip("The distance that the agent can see.")]
        [UnityEngine.Serialization.FormerlySerializedAs("viewDistance")]
        public SharedFloat m_ViewDistance = 1000;
        [BehaviorDesigner.Runtime.Tasks.Tooltip("The raycast offset relative to the pivot position.")]
        [UnityEngine.Serialization.FormerlySerializedAs("offset")]
        public SharedVector3 m_Offset;
        [BehaviorDesigner.Runtime.Tasks.Tooltip("The target raycast offset relative to the pivot position.")]
        [UnityEngine.Serialization.FormerlySerializedAs("targetOffset")]
        public SharedVector3 m_TargetOffset;
        [BehaviorDesigner.Runtime.Tasks.Tooltip("The offset to apply to 2D angles.")]
        [UnityEngine.Serialization.FormerlySerializedAs("angleOffset2D")]
        public SharedFloat m_AngleOffset2D;
        [BehaviorDesigner.Runtime.Tasks.Tooltip("Should the target bone be used?")]
        [UnityEngine.Serialization.FormerlySerializedAs("useTargetBone")]
        public SharedBool m_UseTargetBone;
        [BehaviorDesigner.Runtime.Tasks.Tooltip("The target's bone if the target is a humanoid.")]
        [UnityEngine.Serialization.FormerlySerializedAs("targetBone")]
        public SharedHumanBodyBones m_TargetBone;
        [BehaviorDesigner.Runtime.Tasks.Tooltip("Should a debug look ray be drawn to the scene view?")]
        [UnityEngine.Serialization.FormerlySerializedAs("drawDebugRay")]
        public SharedBool m_DrawDebugRay;
        [BehaviorDesigner.Runtime.Tasks.Tooltip("Should the agent's layer be disabled before the Can See Object check is executed?")]
        [UnityEngine.Serialization.FormerlySerializedAs("disableAgentColliderLayer")]
        public SharedBool m_DisableAgentColliderLayer;
        [BehaviorDesigner.Runtime.Tasks.Tooltip("The object that is within sight.")]
        [UnityEngine.Serialization.FormerlySerializedAs("returnedObject")]
        public SharedGameObject m_ReturnedObject;
        [BehaviorDesigner.Runtime.Tasks.Tooltip("Eye")]
        [UnityEngine.Serialization.FormerlySerializedAs("eye")]
        public SharedTransform eyeTransform;
        
        
        
        private GameObject[] m_AgentColliderGameObjects;
        private int[] m_OriginalColliderLayer;
        private Collider[] m_OverlapColliders;
        private Collider2D[] m_Overlap2DColliders;

        private int m_IgnoreRaycastLayer = LayerMask.NameToLayer("Ignore Raycast");
        
        

        public override void OnStart() {
            base.OnStart();
            if (eyeTransform.Value == null) {
                eyeTransform.Value = transform;
            }
        }

        // Returns success if an object was found otherwise failure
        public override TaskStatus OnUpdate()
        {
            m_ReturnedObject.Value = null;

            // The collider layers on the agent can be set to ignore raycast to prevent them from interferring with the raycast checks.
            if (m_DisableAgentColliderLayer.Value) {
                if (m_AgentColliderGameObjects == null) {
                    if (m_UsePhysics2D) {
                        var colliders = gameObject.GetComponentsInChildren<Collider2D>();
                        m_AgentColliderGameObjects = new GameObject[colliders.Length];
                        for (int i = 0; i < m_AgentColliderGameObjects.Length; ++i) {
                            m_AgentColliderGameObjects[i] = colliders[i].gameObject;
                        }
                    } else {
                        var colliders = gameObject.GetComponentsInChildren<Collider>();
                        m_AgentColliderGameObjects = new GameObject[colliders.Length];
                        for (int i = 0; i < m_AgentColliderGameObjects.Length; ++i) {
                            m_AgentColliderGameObjects[i] = colliders[i].gameObject;
                        }
                    }
                    m_OriginalColliderLayer = new int[m_AgentColliderGameObjects.Length];
                }

                // Change the layer. Remember the previous layer so it can be reset after the check has completed.
                for (int i = 0; i < m_AgentColliderGameObjects.Length; ++i) {
                    m_OriginalColliderLayer[i] = m_AgentColliderGameObjects[i].layer;
                    m_AgentColliderGameObjects[i].layer = m_IgnoreRaycastLayer;
                }
            }

            if ((m_DetectionMode.Value & DetectionMode.Object) != 0 && m_TargetObject.Value != null) {
                if (m_UsePhysics2D) {
                    m_ReturnedObject.Value = MovementUtility.WithinSight2D(eyeTransform.Value, m_Offset.Value, m_FieldOfViewAngle.Value, m_ViewDistance.Value, m_TargetObject.Value, m_TargetOffset.Value, m_AngleOffset2D.Value, m_IgnoreLayerMask, m_UseTargetBone.Value, m_TargetBone.Value, m_DrawDebugRay.Value);
                } else {
                    m_ReturnedObject.Value = MovementUtility.WithinSight(eyeTransform.Value, m_Offset.Value, m_FieldOfViewAngle.Value, m_ViewDistance.Value, m_TargetObject.Value, m_TargetOffset.Value, m_IgnoreLayerMask, m_UseTargetBone.Value, m_TargetBone.Value, m_DrawDebugRay.Value);
                }
            }

            if (m_ReturnedObject.Value == null && (m_DetectionMode.Value & DetectionMode.ObjectList) != 0) {
                var minAngle = Mathf.Infinity;
                for (int i = 0; i < m_TargetObjects.Value.Count; ++i) {
                    GameObject obj;
                    if ((obj = MovementUtility.WithinSight(eyeTransform.Value, m_Offset.Value, m_FieldOfViewAngle.Value, m_ViewDistance.Value, m_TargetObjects.Value[i], m_TargetOffset.Value, m_UsePhysics2D, m_AngleOffset2D.Value, out var angle, m_IgnoreLayerMask, m_UseTargetBone.Value, m_TargetBone.Value, m_DrawDebugRay.Value)) != null) {
                        // This object is within sight. Set it to the objectFound GameObject if the angle is less than any of the other objects
                        if (angle < minAngle) {
                            minAngle = angle;
                            m_ReturnedObject.Value = obj;
                        }
                    }
                }
            }

            if (m_ReturnedObject.Value == null && (m_DetectionMode.Value & DetectionMode.Tag) != 0 && !string.IsNullOrEmpty(m_TargetTag.Value)) {
                var targets = GameObject.FindGameObjectsWithTag(m_TargetTag.Value);
                if (targets != null) {
                    var minAngle = Mathf.Infinity;
                    for (int i = 0; i < targets.Length; ++i) {
                        GameObject obj;
                        if ((obj = MovementUtility.WithinSight(eyeTransform.Value, m_Offset.Value, m_FieldOfViewAngle.Value, m_ViewDistance.Value, targets[i], m_TargetOffset.Value, m_UsePhysics2D, m_AngleOffset2D.Value, out var angle, m_IgnoreLayerMask, m_UseTargetBone.Value, m_TargetBone.Value, m_DrawDebugRay.Value)) != null) {
                            // This object is within sight. Set it to the objectFound GameObject if the angle is less than any of the other objects
                            if (angle < minAngle) {
                                minAngle = angle;
                                m_ReturnedObject.Value = obj;
                            }
                        }
                    }
                }
            }

            if (m_ReturnedObject.Value == null && (m_DetectionMode.Value & DetectionMode.LayerMask) != 0) {
                if (m_UsePhysics2D) {
                    if (m_Overlap2DColliders == null) {
                        m_Overlap2DColliders = new Collider2D[m_MaxCollisionCount];
                    }

                    m_ReturnedObject.Value = MovementUtility.WithinSight2D(eyeTransform.Value, m_Offset.Value, m_FieldOfViewAngle.Value, m_ViewDistance.Value, m_Overlap2DColliders, m_TargetLayerMask.Value, m_TargetOffset.Value, m_AngleOffset2D.Value, m_IgnoreLayerMask, m_DrawDebugRay.Value);
                } else {
                    if (m_OverlapColliders == null) {
                        m_OverlapColliders = new Collider[m_MaxCollisionCount];
                    }

                    m_ReturnedObject.Value = MovementUtility.WithinSight(eyeTransform.Value, m_Offset.Value, m_FieldOfViewAngle.Value, m_ViewDistance.Value, m_OverlapColliders, m_TargetLayerMask.Value, m_TargetOffset.Value, m_IgnoreLayerMask, m_UseTargetBone.Value, m_TargetBone.Value, m_DrawDebugRay.Value);
                }
            }

            // Restore the original layers.
            if (m_DisableAgentColliderLayer.Value) {
                for (int i = 0; i < m_AgentColliderGameObjects.Length; ++i) {
                    m_AgentColliderGameObjects[i].layer = m_OriginalColliderLayer[i];
                }
            }

            if (m_ReturnedObject.Value != null) {
                return TaskStatus.Success;
            }

            // An object is not within sight so return failure
            return TaskStatus.Failure;
        }

        // Reset the public variables
        public override void OnReset()
        {
            m_DetectionMode = DetectionMode.Object | DetectionMode.ObjectList | DetectionMode.Tag | DetectionMode.LayerMask;
            m_FieldOfViewAngle = 90;
            m_ViewDistance = 1000;
            m_Offset = Vector3.zero;
            m_TargetOffset = Vector3.zero;
            m_AngleOffset2D = 0;
            m_TargetTag = "";
            m_DrawDebugRay = false;
            m_IgnoreLayerMask = 1 << LayerMask.NameToLayer("Ignore Raycast");
        }

        // Draw the line of sight representation within the scene window
        public override void OnDrawGizmos()
        {
            MovementUtility.DrawLineOfSight(Owner.transform, m_Offset.Value, m_FieldOfViewAngle.Value, m_AngleOffset2D.Value, m_ViewDistance.Value, m_UsePhysics2D);
        }

        public override void OnBehaviorComplete()
        {
            MovementUtility.ClearCache();
        }
}
