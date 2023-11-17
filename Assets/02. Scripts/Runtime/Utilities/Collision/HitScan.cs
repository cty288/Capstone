using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityGameObject;
using Framework;
using MikroFramework.Architecture;
using MikroFramework.BindableProperty;
using MikroFramework.ResKit;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Factions;
using Runtime.Weapons;
using Runtime.Weapons.Model.Base;
using UnityEditor;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Rendering.Universal;
using UnityEngine.VFX;

namespace Runtime.Utilities.Collision
{
    /// <summary>
    /// HitScan checks for collision using Raycast.
    /// </summary>
    public class HitScan : IHitDetector, IBelongToFaction, ICanGetUtility
    {
        private IHitResponder hitResponder;
        public IHitResponder HitResponder { get => hitResponder; set => hitResponder = value; }

        private TrailRenderer _tr;
        private ObjectPool<TrailRenderer> trailPool;

        private VisualEffect[] _vfx;
        
        
        public GameObject bulletHoleDecal;
        public ObjectPool<GameObject> bulletHolesPool;
        public float bulletHoleFadeTime = 5f;
        
        private Vector3 offset = Vector3.zero;
        private Transform _launchPoint;
        private Camera _camera;
        private Camera _fpsCamera;
        private LayerMask _layer;
        private IWeaponEntity _weapon;
        private bool showDamageNumber = true;

        private bool _useVFX = true;

        private RaycastHit[] _hits = new RaycastHit[10];
        public HitScan(IHitResponder hitResponder, Faction faction, TrailRenderer tr, bool showDamageNumber = true)
        {
            this.hitResponder = hitResponder;
            CurrentFaction.Value = faction;
            _tr = tr;
            this.showDamageNumber = showDamageNumber;
            trailPool = new ObjectPool<TrailRenderer>(CreateTrail);
            _useVFX = false;
            bulletHolesPool = new ObjectPool<GameObject>(CreateBulletHole, OnTakeFromPool, 
                OnReturnedToPool, OnDestroyPoolObject, true, 10, 20);
            bulletHoleDecal = this.GetUtility<ResLoader>().LoadSync<GameObject>("BulletHoleDecal");
        }
        
        public HitScan(IHitResponder hitResponder, Faction faction, VisualEffect[] vfx, Camera fpsCam, bool showDamageNumber = true)
        {
            this.hitResponder = hitResponder;
            CurrentFaction.Value = faction;
            _vfx = vfx;
            _fpsCamera = fpsCam;
            _useVFX = true;
            this.showDamageNumber = showDamageNumber;
            bulletHolesPool = new ObjectPool<GameObject>(CreateBulletHole, OnTakeFromPool, 
                OnReturnedToPool, OnDestroyPoolObject, true, 10, 20);
            bulletHoleDecal = this.GetUtility<ResLoader>().LoadSync<GameObject>("BulletHoleDecal");
        }
        
        /// <summary>
        /// Called every frame to check for Raycast collision.
        /// </summary>
        /// <returns>Returns true if hit detected.</returns>
        public void CheckHit(HitDetectorInfo hitDetectorInfo, int damage)
        {
            // Debug.Log("checkhit");
            _launchPoint = hitDetectorInfo.launchPoint;
            _camera = hitDetectorInfo.camera;
            _layer = hitDetectorInfo.layer;
            _weapon = hitDetectorInfo.weapon;
            this.Damage = damage;

            ShootBullet();
        }

        public int Damage { get; protected set; }

        //TODO: faction and IDamagable integration.
        private void ShootBullet()
        {
            Vector3 shootDir = _camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0)).direction;
            float spread = _weapon.GetSpread().RealValue.Value;
            shootDir += new Vector3(
                Random.Range(-spread, spread),
                Random.Range(-spread, spread),
                Random.Range(-spread, spread));
            shootDir.Normalize();
            
            
            HitData hitData = null;
            //RaycastHit hit;
            
            
            for (int i = 0; i < _hits.Length; i++) {
                _hits[i] = new RaycastHit();
            }
            int nums = Physics.RaycastNonAlloc(_camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0)), _hits,
                _weapon.GetRange().RealValue.Value, _layer);
            
            var sortedHits = _hits.OrderBy(hit => hit.transform ? hit.distance : float.MaxValue).ToArray();


            bool hitAnything = false;
            for (int i = 0; i < nums; i++) {
                RaycastHit hit = sortedHits[i];
                
                // Debug.Log("hit w/ gun: " + hit.collider.name);
                IHurtbox hurtbox = hit.collider.GetComponent<IHurtbox>();

                HurtboxModifier hurtboxModifier = hit.collider.GetComponent<HurtboxModifier>();
                if (hurtboxModifier) {
                    if (hurtboxModifier.IgnoreHurtboxCheck) {
                        continue;
                    }
                    
                    if (hurtboxModifier.RedirectActivated) {
                        hurtbox = hurtboxModifier.Hurtbox;
                    }
                }


                if (hurtbox == null && hit.collider.isTrigger) {
                    continue;
                }
                

                
                hitAnything = true;
                if (hurtbox != null)
                {
                    Debug.Log("hurtbox make hitdata: " + hurtbox);
                    hitData = new HitData().SetHitScanData(hitResponder, hurtbox, hit, this, showDamageNumber);
                    hitData.HitDirectionNormalized = Vector3.Normalize(_camera.transform.forward + offset);
                }

                if (hurtbox != null && hitData.Validate())
                {
                    
                    Debug.Log("hurtbox respond: " + hurtbox);
                    // hit something with hurtbox
                    hitData.HitDetector.HitResponder?.HitResponse(hitData);
                    hitData.Hurtbox.HurtResponder?.HurtResponse(hitData);
                    if(!_useVFX)
                        CoroutineRunner.Singleton.StartCoroutine(PlayTrail(_launchPoint.position, hit.point, hit));
                    else
                        PlayBulletVFX(_launchPoint.position, hit.point);
                    break;
                }
                else
                {
                    Debug.Log("no hurtbox");
                    // hit something without hurtbox, e.g. wall
                    if(!_useVFX)
                        CoroutineRunner.Singleton.StartCoroutine(PlayTrail(_launchPoint.position, hit.point, new RaycastHit()));
                    else 
                        PlayBulletVFX(_launchPoint.position, hit.point);
                    float positionMultiplier = 0.2f;
                    float spawnX = hit.point.x - hit.normal.x * positionMultiplier;
                    float spawnY = hit.point.y - hit.normal.y * positionMultiplier;
                    float spawnZ = hit.point.z - hit.normal.z * positionMultiplier;
                    Vector3 spawnPosition = new Vector3(spawnX, spawnY, spawnZ);
                    
                    GameObject bulletHole = bulletHolesPool.Get();
                    // GameObject bulletHole
                    bulletHole.transform.position = spawnPosition;  
                    bulletHole.transform.rotation = Quaternion.LookRotation(hit.normal);
                    bulletHole.transform.Rotate(Vector3.forward, Random.Range(0f, 360f));
                    CoroutineRunner.Singleton.StartCoroutine(FadeBullet(bulletHole));
                    break;
                }
            }

            if (!hitAnything) {
                //hit nothing
                if(!_useVFX)
                    CoroutineRunner.Singleton.StartCoroutine(PlayTrail(_launchPoint.position, _launchPoint.position + (shootDir * _weapon.GetRange().RealValue), new RaycastHit()));
                else
                    PlayBulletVFX(_launchPoint.position, _launchPoint.position + (shootDir * _weapon.GetRange().RealValue));
            }
        }

        private IEnumerator FadeBullet(GameObject bulletHole)
        {
            yield return new WaitForSeconds(bulletHoleFadeTime);
            // Debug.Log("release bullet");
            bulletHolesPool.Release(bulletHole);
        }
        
        void OnReturnedToPool(GameObject obj)
        {
            obj.SetActive(false);
        }

        // Called when an item is taken from the pool using Get
        void OnTakeFromPool(GameObject obj)
        {
            obj.SetActive(true);
        }
        
        void OnDestroyPoolObject(GameObject obj)
        {
            GameObject.Destroy(obj);
        }

        protected void PlayBulletVFX(Vector3 startPoint, Vector3 endPoint)
        {
            // Camera calculations
            var conversion = _camera.ScreenToWorldPoint(_fpsCamera.WorldToScreenPoint(startPoint));
            //var newStartPos = _vfx[0].transform.InverseTransformPoint(conversion);
            startPoint = conversion;
            
            Vector3 dir = endPoint - startPoint;
            float bulletSpeed = _weapon.GetBulletSpeed().GetRealValue().Value * 0.5f;
            float maxDistance = Vector3.Distance(startPoint, endPoint);
            float lifeTime = maxDistance / bulletSpeed;

            foreach (var vfx in _vfx)
            {
                vfx.transform.rotation = Quaternion.identity;

                vfx.SetVector3("StartPos", startPoint);
                //vfx.SetVector3("EndPos", endPoint);
                vfx.SetVector3("FireVector", dir);
                vfx.SetFloat("BulletSpeed", bulletSpeed);
                vfx.SetFloat("MaxDistance", maxDistance);
                vfx.SetFloat("LifeTime", lifeTime);
                vfx.Play();
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
                remainingDistance -= _weapon.GetBulletSpeed().GetRealValue().Value * Time.deltaTime;

                yield return null;
            }

            instance.transform.position = endPoint;

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
            trail.colorGradient = _tr.colorGradient;
            trail.material = _tr.material;
            trail.widthCurve = _tr.widthCurve;
            trail.time = _tr.time;
            trail.minVertexDistance = _tr.minVertexDistance;
            
            trail.emitting = false;
            trail.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            return trail;
        }
        
        private GameObject CreateBulletHole()
        {
            return GameObject.Instantiate(bulletHoleDecal);;
        }

        public BindableProperty<Faction> CurrentFaction { get; } = new BindableProperty<Faction>(Faction.Friendly);
        
        public IArchitecture GetArchitecture()
        {
            return MainGame.Interface;
        }
    }
}

