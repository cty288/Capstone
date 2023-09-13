using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityCircleCollider2D;
using JetBrains.Annotations;
using MikroFramework;
using MikroFramework.BindableProperty;
using Runtime.Controls;
using Runtime.DataFramework.Entities;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Factions;
using Runtime.DataFramework.Properties.CustomProperties;
using Runtime.GameResources.Model.Base;
using Runtime.Temporary.Player;
using Runtime.Temporary.Weapon;
using Runtime.Utilities.Collision;
using Runtime.Weapons.Model.Base;
using Runtime.Weapons.Model.Builders;
using Runtime.Weapons.ViewControllers.Base;
using UnityEngine;
using UnityEngine.Pool;

namespace Runtime.Weapons
{
    public class RustyPistolEntity : WeaponEntity<RustyPistolEntity>
    {
        [field: SerializeField] public override string EntityName { get; set; } = "RustyPistol";
        
        public override void OnRecycle()
        {
        }
        
        protected override string OnGetDescription(string defaultLocalizationKey)
        {
            return null;
        }

        protected override ICustomProperty[] OnRegisterCustomProperties()
        {
            return null;
        }

        public override ResourceCategory GetResourceCategory() {
            return ResourceCategory.Weapon;
        }

        public override string OnGroundVCPrefabName { get; } = "RustyPistolOnGround";
    }

    public class RustyPistol : AbstractWeaponViewController<RustyPistolEntity>, IHitResponder
    {
        private Camera cam;
        // public GameObject lineRendererPrefab;
        // private List<LineRenderer> lineRenderers;
        // public LayerMask layer;
        
        // [Header("Timers & Counters")]
        // private float lastShootTime;
        // private int currentAmmo;
        // private float currentReloadCD;
        //
        // [Header("Gun General Settings")] [SerializeField]
        // protected Transform launchPoint;
        //
        // [Header("HitDetector Settings")] [SerializeField]
        // private HitScan hitScan;
        // private HitDetectorInfo hitDetectorInfo;
        
        [SerializeField] private GameObject hitParticlePrefab;
        
        [field: ES3Serializable]
        public BindableProperty<Faction> CurrentFaction { get; protected set; } = new BindableProperty<Faction>(Faction.Friendly);

        
        // For IHitResponder.
        public int Damage => BoundEntity.GetBaseDamage().RealValue;
        private DPunkInputs.PlayerActions playerActions;

        protected override void Awake() {
            base.Awake();
            playerActions = ClientInput.Singleton.GetPlayerActions();
            cam = Camera.main;
        }
        
        protected override IEntity OnInitWeaponEntity(WeaponBuilder<RustyPistolEntity> builder) {
            return builder.FromConfig().Build();
        }
        
        protected override void OnBindEntityProperty() {}

        
        //=====
        private float lastShootTime = 0f;
        private float reloadStartTime = 0f;
        private int currentAmmo;

        private ObjectPool<TrailRenderer> trailPool;
        public TrailRenderer trailPrefab;
        public ParticleSystem particleSystem;
        public LayerMask layer;
        
        protected override void OnEntityStart()
        {
            currentAmmo = BoundEntity.GetAmmoSize().RealValue;
            
            trailPool = new ObjectPool<TrailRenderer>(CreateTrail, null, null, null, true, 10, 30);
            
            // lineRenderers = new List<LineRenderer>();
            // for (int i = 0; i < BoundEntity.GetBulletsPerShot().RealValue; i++)
            // {
            //     Debug.Log("adding line renderer");
            //     lineRenderers.Add(Instantiate(lineRendererPrefab, transform).GetComponent<LineRenderer>());
            // }
            //
            // hitScan = new HitScan(this, CurrentFaction.Value);
            // hitDetectorInfo = new HitDetectorInfo
            // {
            //     camera = cam,
            //     layer = layer,
            //     lineRenderers = lineRenderers,
            //     launchPoint = launchPoint,
            //     weapon = BoundEntity
            // };
        }
            
        public void Shoot()
        {
            particleSystem.Play();
            
            Vector3 shootDir = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0)).direction;
            float spread = BoundEntity.GetSpread().RealValue.Value;
            shootDir += new Vector3(
                Random.Range(-spread, spread),
                Random.Range(-spread, spread),
                Random.Range(-spread, spread));
            shootDir.Normalize();
            
            if (Physics.Raycast(cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0)), out RaycastHit hit,
                    BoundEntity.GetRange().RealValue.Value, layer))
            {
                Debug.Log("hit");
                StartCoroutine(PlayTrail(particleSystem.transform.position, hit.point, hit));
            }
            else
            {
                Debug.Log("no hit");
                StartCoroutine(PlayTrail(particleSystem.transform.position, particleSystem.transform.position + (shootDir * BoundEntity.GetRange().RealValue), new RaycastHit()));
            }
        }

        private IEnumerator PlayTrail(Vector3 startPoint, Vector3 endPoint, RaycastHit hit)
        {
            TrailRenderer instance = trailPool.Get();
            instance.gameObject.SetActive(true);
            instance.transform.position = startPoint;
            yield return null; // avoid position carry-over from last frame if reused

            instance.emitting = true;

            float distance = Vector3.Distance(startPoint, endPoint);
            float remainingDistance = distance;
            while (remainingDistance > 0)
            {
                instance.transform.position = Vector3.Lerp(
                    startPoint,
                    endPoint,
                    Mathf.Clamp01(1 - (remainingDistance / distance))
                );
                remainingDistance -= 50f * Time.deltaTime;

                yield return null;
            }

            instance.transform.position = endPoint;
            
            Instantiate(hitParticlePrefab, hit.point, Quaternion.identity);

            yield return new WaitForSeconds(0.01f);
            yield return null;
            instance.emitting = false;
            instance.gameObject.SetActive(false);
            trailPool.Release(instance);
        }
        
        private TrailRenderer CreateTrail()
        {
            GameObject instance = new GameObject("bullet trail");
            TrailRenderer trail = instance.AddComponent<TrailRenderer>();
            trail.colorGradient = trailPrefab.colorGradient;
            trail.material = trailPrefab.material;
            trail.widthCurve = trailPrefab.widthCurve;
            trail.time = trailPrefab.time;
            trail.minVertexDistance = trailPrefab.minVertexDistance;
            
            trail.emitting = false;
            trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            return trail;
        }
        
        public void Update()
        {
            if (playerActions.Shoot.IsPressed())
            {
                if (currentAmmo > 0 &&
                    Time.time > lastShootTime + BoundEntity.GetAttackSpeed().RealValue)
                {
                    lastShootTime = Time.time;

                    Shoot();
                    
                    currentAmmo--;
                    if(currentAmmo == 0)
                        reloadStartTime = Time.time;
                }
            }
            
            if (Time.time > reloadStartTime + BoundEntity.GetReloadSpeed().RealValue)
            {
                currentAmmo = BoundEntity.GetAmmoSize().RealValue;
            }
        }

        // public void Shoot() {
        //     hitScan.CheckHit(hitDetectorInfo);
        // }
        
        public bool CheckHit(HitData data)
        {
            return data.Hurtbox.Owner != gameObject;
        }
        
        public void HitResponse(HitData data)
        {
            Instantiate(hitParticlePrefab, data.HitPoint, Quaternion.identity);
            
            //TODO: Change to non-temporary class of player entity.
            PlayerMovement playerMovement = FindObjectOfType<PlayerMovement>();
            // playerMovement.rb.AddForce(data.Recoil * -data.HitDirectionNormalized, ForceMode.Impulse);
        }
    }
}