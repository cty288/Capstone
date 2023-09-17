using System.Collections;
using System.Collections.Generic;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityGameObject;
using MikroFramework.BindableProperty;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Factions;
using Runtime.Weapons;
using Runtime.Weapons.Model.Base;
using UnityEditor;
using UnityEngine;
using UnityEngine.Pool;

namespace Runtime.Utilities.Collision
{
    /// <summary>
    /// HitScan checks for collision using Raycast.
    /// </summary>
    public class HitScan : IHitDetector, IBelongToFaction
    {
        private IHitResponder hitResponder;
        public IHitResponder HitResponder { get => hitResponder; set => hitResponder = value; }

        private ObjectPool<TrailRenderer> trailPool;
        private TrailRenderer _tr;
        private Vector3 offset = Vector3.zero;
        private Transform _launchPoint;
        private Camera _camera;
        // private List<LineRenderer> _lineRenderers;
        private LayerMask _layer;
        private IWeaponEntity _weapon;

        public HitScan(IHitResponder hitResponder, Faction faction, TrailRenderer tr)
        {
            this.hitResponder = hitResponder;
            CurrentFaction.Value = faction;
            _tr = tr;
            
            trailPool = new ObjectPool<TrailRenderer>(CreateTrail);
        }
        
        /// <summary>
        /// Called every frame to check for Raycast collision.
        /// </summary>
        /// <returns>Returns true if hit detected.</returns>
        public void CheckHit(HitDetectorInfo hitDetectorInfo)
        {
            // Debug.Log("checkhit");
            _launchPoint = hitDetectorInfo.launchPoint;
            _camera = hitDetectorInfo.camera;
            _layer = hitDetectorInfo.layer;
            _weapon = hitDetectorInfo.weapon;

            ShootBullet();
        }

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
            RaycastHit hit;
            
            if (Physics.Raycast(_camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0)), out hit,
                    _weapon.GetRange().RealValue.Value, _layer))
            {
                // Debug.Log("hit");
                IHurtbox hurtbox = hit.collider.GetComponent<IHurtbox>();
                if (hurtbox != null)
                {
                    hitData = new HitData().SetHitScanData(hitResponder, hurtbox, hit, this);
                    hitData.Recoil = _weapon.GetRecoil().RealValue.Value;
                    hitData.HitDirectionNormalized = Vector3.Normalize(_camera.transform.forward + offset);
                }

                if (hitData.Validate())
                {
                    // Debug.Log("validate");
                    hitData.HitDetector.HitResponder?.HitResponse(hitData);
                    hitData.Hurtbox.HurtResponder?.HurtResponse(hitData);
                    
                    CoroutineRunner.Singleton.StartCoroutine(PlayTrail(_launchPoint.position, hit.point, hit));
                }
                else
                {
                    CoroutineRunner.Singleton.StartCoroutine(PlayTrail(_launchPoint.position, _launchPoint.position + (shootDir * _weapon.GetRange().RealValue), new RaycastHit()));
                }
            }
            else
            {
                CoroutineRunner.Singleton.StartCoroutine(PlayTrail(_launchPoint.position, _launchPoint.position + (shootDir * _weapon.GetRange().RealValue), new RaycastHit()));
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

        public BindableProperty<Faction> CurrentFaction { get; } = new BindableProperty<Faction>(Faction.Friendly);
    }
}

