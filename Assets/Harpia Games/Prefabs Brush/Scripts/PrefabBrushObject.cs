using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

// ReSharper disable ConvertIfStatementToConditionalTernaryExpression

namespace Harpia.PrefabBrush
{
    /// <summary>
    /// This script allows any game object to be interacted with by the Prefab Brush.
    /// </summary>
    [DisallowMultipleComponent]
    public class PrefabBrushObject : MonoBehaviour
    {
#if UNITY_EDITOR

        private static float clippingSliderValue = 1;

        public struct RadiusBounds
        {
            public Vector3 pivot;
            public float radius;
            public readonly float originalRadius;

            public RadiusBounds(Bounds cachedBounds, Transform root)
            {
                originalRadius = (cachedBounds.extents.x + cachedBounds.extents.z) / 2;
                radius = originalRadius * clippingSliderValue;
                pivot = root.transform.position;
            }

            public bool Intersects(RadiusBounds other)
            {
#if HARPIA_DEBUG
                if (other.pivot == Vector3.zero && radius == 0) Debug.LogError($"Intersects {other.pivot} {other.radius} {pivot} {radius}");
#endif
                return Intersects(other.radius, other.pivot);
            }

            public bool Intersects(Vector3 raycastHitPoint)
            {
                var sqrDistance = (pivot - raycastHitPoint).sqrMagnitude;
                return sqrDistance < radius * radius;
            }

            public bool Intersects(float radiusParam, Vector3 pointPoint)
            {
                float distanceSqr = (pivot - pointPoint).sqrMagnitude;
                float radiusSum = (radius * radius + radiusParam * radiusParam);
                return distanceSqr < radiusSum;
            }
        }

        //Transforms
        private Vector3 _lastBoundsPosition;
        private Vector3 _lastBoundsScale;
        private Quaternion _lastBoundsRotation;

        //Bounds
        private RadiusBounds _cachedRadiusBounds;
        private Bounds _cachedBounds;

        [Tooltip("Allow this object to be erased by the Prefab Brush.")]
        public bool erasable = true;

        [Tooltip("When enabled, this function assesses if a newly painted object would reside within the boundaries of an existing object. If a potential overlap is detected, it prevents the placement of the new object to ensure objects don't clip into one another.")]
        public bool meshClippingChecks = true;

        private static PrefabBrushObject[] brushObjectsCache;
        private static List<PrefabBrushObject> toDispose;

        [SerializeField, HideInInspector]
        private MeshCollider addedCollider;
        
        [SerializeField, HideInInspector]
        private Rigidbody addedBody;

        public void Init(bool addPhysicsCompoentns)
        {
            if (addPhysicsCompoentns)
            {
                //Lets add a collider
                if (!HasPhysicalCollider(this.gameObject))
                {
                    MeshRenderer meshRenderer = GetComponentInChildren<MeshRenderer>();
                    if (meshRenderer != null)
                    {
                        addedCollider = gameObject.AddComponent<MeshCollider>();
                        addedCollider.convex = true;
                    }
                }

                //Let add a RB
                if (!HasValidRigidbody(this.gameObject))
                {
                    addedBody = gameObject.AddComponent<Rigidbody>();
                    addedBody.isKinematic = false;
                    addedBody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                    
                }

#if HARPIA_DEBUG
                Debug.Log($"Added collider and rigidbody to {gameObject.name}", gameObject);
#endif
                
                toDispose??= new List<PrefabBrushObject>();
                toDispose.Add(this);
            }

            hideFlags = HideFlags.DontSaveInBuild;

            InitCachedBounds();
        }

        private void DrawBounds(Vector3 hitPos, Color boundsColor, float sqrDistance)
        {
            float distanceSqr = (transform.position - hitPos).sqrMagnitude;
            if (distanceSqr > sqrDistance) return;

            Handles.color = boundsColor;
            Transform transform1 = transform;
            Handles.DrawWireArc(_cachedRadiusBounds.pivot, transform1.up, transform1.right, 360, _cachedRadiusBounds.radius);
        }

        private void DrawBoundsEraser2(Vector3 hitPos, Color boundsColor, float sqrDistance, float eraserRadius)
        {
            float distanceSqr = (transform.position - hitPos).sqrMagnitude;
            if (distanceSqr > sqrDistance) return;

            var v = _cachedRadiusBounds.Intersects(eraserRadius, hitPos);
            Handles.color = v ? Color.red : boundsColor;
            Transform transform1 = transform;
            Handles.DrawWireArc(_cachedRadiusBounds.pivot, transform1.up, transform1.right, 360, _cachedRadiusBounds.radius);
        }

        public static void DrawBoundsAll(Vector3 hitPos, Color boundsColor)
        {
            PrefabBrushObject[] all = GetBrushObjects();
            float sqrDistance = clippingSliderValue * 5;
            foreach (PrefabBrushObject brushObject in all)
            {
                if (brushObject == null) continue;
                brushObject.DrawBounds(hitPos, boundsColor, sqrDistance * sqrDistance);
            }
        }

        public static void DrawBoundsEraser(Vector3 hitPos, Color boundsColor, float sqrDistance, float eraserRadius)
        {
            PrefabBrushObject[] all = GetBrushObjects();
            foreach (PrefabBrushObject brushObject in all)
            {
                if (brushObject == null) continue;
                if (brushObject.erasable == false) continue;
                brushObject.DrawBoundsEraser2(hitPos, boundsColor, sqrDistance * sqrDistance, eraserRadius );
            }
        }

        private void OnValidate()
        {
            hideFlags = HideFlags.DontSaveInBuild;
        }

        public static PrefabBrushObject[] GetBrushObjects()
        {
            if (brushObjectsCache == null)
            {
                ForceInit();
            }

            return brushObjectsCache;
        }

        private static Bounds GetBoundsStatic(Transform obj)
        {
            Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();

            if (renderers.Length == 0) return new Bounds(obj.position, Vector3.zero);

            Bounds bounds = renderers[0].bounds;
            for (int index = 1; index < renderers.Length; index++)
            {
                Renderer renderer = renderers[index];
                bounds.Encapsulate(renderer.bounds);
            }

            return bounds;
        }

        public RadiusBounds GetBoundsSphere()
        {
            if (transform.localScale == _lastBoundsScale &&
                transform.position == _lastBoundsPosition &&
                transform.rotation == _lastBoundsRotation)
                return _cachedRadiusBounds;

            InitCachedBounds();

            return _cachedRadiusBounds;
        }

        public Bounds GetBounds()
        {
            if (transform.localScale == _lastBoundsScale &&
                transform.position == _lastBoundsPosition &&
                transform.rotation == _lastBoundsRotation)
                return _cachedBounds;

            InitCachedBounds();

            return _cachedBounds;
        }

        private void OnCollisionEnter(Collision other)
        {
            if (Application.isPlaying) return;

            Debug.Log($"enter col");
        }

        private void InitCachedBounds()
        {
            _cachedBounds = GetBoundsStatic(transform);

            Transform t = transform;
            Vector3 pos = t.position;

            _cachedRadiusBounds = new RadiusBounds(_cachedBounds, t);

            _lastBoundsPosition = pos;
            _lastBoundsScale = t.localScale;
            _lastBoundsRotation = t.rotation;
        }
        
        public static bool HasPhysicalCollider(GameObject o)
        {
            //Copy this method to prefabBrushObject
            foreach (Collider collider in o.GetComponentsInChildren<Collider>())
            {
                if (!collider.enabled) continue;
                if (collider.isTrigger) continue;
                return true;
            }
            return false;
        }

        public static bool HasValidRigidbody(GameObject o)
        {
            //Copy this method to prefabBrushObject
            foreach (Rigidbody rigidbody in o.GetComponentsInChildren<Rigidbody>())
                if (rigidbody.isKinematic == false)
                    return true;

            return false;
        }

        public static void Dispose(bool removePhysicalComponents = true )
        {
            if(removePhysicalComponents && toDispose != null)
            {
                bool removed = false;
                foreach (PrefabBrushObject o in toDispose)
                {
                    if (o == null) continue;
                    o.DisposeObject();
                    removed = true;
                }
                toDispose.Clear();
                if (removed)
                {
                    Debug.LogError($"[Prefab Brush] Colliders and Rigidbodies removed");
                    //PB_HandlesExtension.WriteTempText("Colliders and Rigidbodies removed", 1.2f);
                }

#if HARPIA_DEBUG
                Debug.Log($"[PrefabBrushObject] Dispose {toDispose.Count} physics objects");
#endif
            }
            
            if(brushObjectsCache == null) return;
            
            foreach (var obj in brushObjectsCache)
            {
                if (obj == null) continue;
                obj.DisposeObject();
            }
            

            brushObjectsCache = null;
        }

        private void DisposeObject()
        {
            if (addedCollider != null) DestroyImmediate(addedCollider);
            if (addedBody != null) DestroyImmediate(addedBody);
        }

        public static void ForceInit()
        {
#if UNITY_2022_2_OR_NEWER
            brushObjectsCache = FindObjectsByType<PrefabBrushObject>(FindObjectsInactive.Include,FindObjectsSortMode.None);
#else

            brushObjectsCache = FindObjectsOfType<PrefabBrushObject>();
#endif

            foreach (PrefabBrushObject brushObject in brushObjectsCache)
            {
                brushObject.InitCachedBounds();
            }
        }

        public static void SetClippingSize(float value)
        {
            PrefabBrushObject[] all = GetBrushObjects();
            clippingSliderValue = value;

            foreach (PrefabBrushObject brushObject in all)
            {
                if (brushObject == null) continue;
                brushObject.UpdateClippingSize();
            }
        }

        private void UpdateClippingSize()
        {
            _cachedRadiusBounds.radius = _cachedRadiusBounds.originalRadius * clippingSliderValue;
        }

        public bool BoundsIntersect(Vector3 raycastHitPoint)
        {
            if (!meshClippingChecks) return false;
            if (_cachedBounds.center == Vector3.zero && _cachedBounds.size == Vector3.zero) InitCachedBounds();
            return _cachedRadiusBounds.Intersects(raycastHitPoint);
        }

        public bool BoundsIntersect(RadiusBounds bounds)
        {
            if (_cachedBounds.center == Vector3.zero && _cachedBounds.size == Vector3.zero) InitCachedBounds();
            return _cachedRadiusBounds.Intersects(bounds);
        }

        public static void SetClippingSize(Slider clippingToleranceSlider)
        {
            clippingSliderValue = 1 - clippingToleranceSlider.value;
        }
#endif
    }
}