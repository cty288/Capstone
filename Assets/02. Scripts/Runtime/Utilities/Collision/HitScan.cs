using System.Collections;
using System.Collections.Generic;
using MikroFramework.BindableProperty;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Factions;
using Runtime.Weapons;
using Runtime.Weapons.Model.Base;
using UnityEditor;
using UnityEngine;

namespace Runtime.Utilities.Collision
{
    /// <summary>
    /// HitScan checks for collision using Raycast.
    /// </summary>
    public class HitScan : IHitDetector, IBelongToFaction
    {
        private IHitResponder hitResponder;
        public IHitResponder HitResponder { get => hitResponder; set => hitResponder = value; }
        

        private Vector3 offset = Vector3.zero;
        private Transform _launchPoint;
        private Camera _camera;
        private List<LineRenderer> _lineRenderers;
        private LayerMask _layer;
        private IWeaponEntity _weapon;

        public HitScan(IHitResponder hitResponder, Faction faction)
        {
            this.hitResponder = hitResponder;
            CurrentFaction.Value = faction;
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
            _lineRenderers = hitDetectorInfo.lineRenderers;
            _layer = hitDetectorInfo.layer;
            _weapon = hitDetectorInfo.weapon;

            foreach (LineRenderer lr in _lineRenderers)
            {
                ShootRay(lr);
            }
        }

        //TODO: faction and IDamagable integration.
        private void ShootRay(LineRenderer lr)
        {
            Vector3 origin = _camera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0));
            //TODO: Adjust spread to be less random and less punishing when further away [?].
            offset[0] = Random.Range(-_weapon.GetSpread().RealValue.Value, _weapon.GetSpread().RealValue.Value);
            offset[1] = Random.Range(-_weapon.GetSpread().RealValue.Value, _weapon.GetSpread().RealValue.Value);
            offset[2] = Random.Range(-_weapon.GetSpread().RealValue.Value, _weapon.GetSpread().RealValue.Value);
            
            HitData hitData = null;
            RaycastHit hit;
            if (Physics.Raycast(origin, Vector3.Normalize(_camera.transform.forward + offset), out hit,
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

                    //Draw hitscan line.
                    SetLine(lr, _launchPoint.position, hitData.HitPoint);
                    Debug.Log("hit");
                    CoroutineRunner.Singleton.StartCoroutine(DrawHitscan(lr));
                }
            }
            else
            {
                SetLine(lr, _launchPoint.position, Vector3.Normalize(_camera.transform.forward + offset) * _weapon.GetRange().RealValue.Value);
                CoroutineRunner.Singleton.StartCoroutine(DrawHitscan(lr));
            }
        }

        private void SetLine(LineRenderer lr, Vector3 start, Vector3 end)
        {
            lr.SetPosition(0, start);
            lr.SetPosition(1, end);
        }
        
        IEnumerator DrawHitscan(LineRenderer lr)
        {
            lr.enabled = true;
            yield return new WaitForSeconds(0.3f);
            lr.enabled = false;
        }

        public BindableProperty<Faction> CurrentFaction { get; } = new BindableProperty<Faction>(Faction.Friendly);
    }
}

