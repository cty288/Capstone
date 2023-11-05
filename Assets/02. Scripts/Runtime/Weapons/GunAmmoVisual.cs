using System;
using Framework;
using MikroFramework.Architecture;
using Runtime.GameResources.ViewControllers;
using Runtime.Inventory.Model;
using Runtime.Utilities;
using Runtime.Weapons.Model.Base;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Runtime.Weapons
{
    public class GunAmmoVisual : AbstractMikroController<MainGame>
    {
        //[SerializeField] private GameObject weapon;
        private IWeaponEntity _associatedWeapon;
        [SerializeField] private Renderer[] renderers;
        [SerializeField] private Material gunBarrelIndicator;
        [SerializeField] private bool updateIndicators = true;
        private Material[] _instanceGunBarrelIndicators;
        private static readonly int MaxAmmo = Shader.PropertyToID("_MaxAmmo");
        private static readonly int CurrentAmmo = Shader.PropertyToID("_CurrentAmmo");

        
        void Awake()
        {
            if(!updateIndicators) return;
            _instanceGunBarrelIndicators = new Material[renderers.Length];
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i].material == null)
                {
                    renderers[i].material = new Material(gunBarrelIndicator);
                    _instanceGunBarrelIndicators[i] = renderers[i].material;
                }
                else
                {
                    foreach (var material in renderers[i].materials)
                    {
                        if (material.shader.name.Equals(gunBarrelIndicator.shader.name))
                        {
                            _instanceGunBarrelIndicators[i] = material;
                        }
                    }
                }
                if (_instanceGunBarrelIndicators[i] == null)
                {
                    renderers[i].material = new Material(gunBarrelIndicator);
                    _instanceGunBarrelIndicators[i] = renderers[i].material;
                }
            }
        }

        public void Init(IWeaponEntity entity) {
            _associatedWeapon = entity;
            if(!updateIndicators) return;
            foreach (var gunBarrelIndicator in _instanceGunBarrelIndicators)
            {
                gunBarrelIndicator.SetInteger(MaxAmmo, _associatedWeapon.GetAmmoSize().RealValue);
            }
            _associatedWeapon.CurrentAmmo.RegisterWithInitValue(OnAmmoChanged)
                .UnRegisterWhenGameObjectDestroyedOrRecycled(transform.parent.gameObject);
        }

        public void OnAmmoChanged(int num)
        {
            if(!updateIndicators) return;
            foreach (var gunBarrelIndicator in _instanceGunBarrelIndicators)
            {
                gunBarrelIndicator.SetFloat(CurrentAmmo, num);
            }
        }
        
    }
}
