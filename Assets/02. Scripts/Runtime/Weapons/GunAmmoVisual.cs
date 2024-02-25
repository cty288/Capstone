using System;
using Framework;
using MikroFramework.Architecture;
using Runtime.GameResources.ViewControllers;
using Runtime.Inventory.Model;
using Runtime.Utilities;
using Runtime.Weapons.Model.Base;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace Runtime.Weapons
{
    public class GunAmmoVisual : AbstractMikroController<MainGame>
    {
        //[SerializeField] private GameObject weapon;
        private IWeaponEntity _associatedWeapon;
        [SerializeField] private Renderer[] renderers;
        [SerializeField] private RawImage[] images;
        [SerializeField] private Material unlitIndicator;
        [SerializeField] private Material spriteIndicator;
        [SerializeField] private bool updateIndicators = true;
        [SerializeField] private TMP_Text currentAmmo;
        [SerializeField] private TMP_Text maxAmmo;
        private Material[] _instanceGunBarrelIndicators;
        private static readonly int MaxAmmo = Shader.PropertyToID("_MaxAmmo");
        private static readonly int CurrentAmmo = Shader.PropertyToID("_CurrentAmmo");

        
        void Awake()
        {
            if(!updateIndicators) return;
            _instanceGunBarrelIndicators = new Material[renderers.Length + images.Length];
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i].material == null)
                {
                    renderers[i].material = new Material(unlitIndicator);
                    _instanceGunBarrelIndicators[i] = renderers[i].material;
                }
                else
                {
                    foreach (var material in renderers[i].materials)
                    {
                        if (material.shader.name.Equals(unlitIndicator.shader.name))
                        {
                            _instanceGunBarrelIndicators[i] = material;
                        }
                    }
                }
                if (_instanceGunBarrelIndicators[i] == null)
                {
                    renderers[i].material = new Material(unlitIndicator);
                    _instanceGunBarrelIndicators[i] = renderers[i].material;
                }
            }
            
            for (int i = 0; i < images.Length; i++)
            {
                images[i].material = new Material(spriteIndicator);
                _instanceGunBarrelIndicators[renderers.Length + i] = images[i].material;
                if (_instanceGunBarrelIndicators[renderers.Length + i] == null)
                {
                    images[i].material = new Material(spriteIndicator);
                    _instanceGunBarrelIndicators[renderers.Length + i] = images[i].material;
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

            if(maxAmmo) maxAmmo.text = _associatedWeapon.GetAmmoSize().RealValue.ToString();
            _associatedWeapon.CurrentAmmo.RegisterWithInitValue(OnAmmoChanged)
                .UnRegisterWhenGameObjectDestroyedOrRecycled(transform.parent.gameObject);
            _associatedWeapon.GetAmmoSize().RealValue.RegisterOnValueChanged(OnAmmoSizeChanged)
                .UnRegisterWhenGameObjectDestroyedOrRecycled(transform.parent.gameObject);
        }

        private void OnAmmoSizeChanged(int arg1, int num) {
            if(!updateIndicators) return;
            foreach (var gunBarrelIndicator in _instanceGunBarrelIndicators)
            {
                gunBarrelIndicator.SetInteger(MaxAmmo, num);
            }

            if(maxAmmo) maxAmmo.text = num.ToString();
        }

        public void OnAmmoChanged(int num)
        {
            if(!updateIndicators) return;
            foreach (var gunBarrelIndicator in _instanceGunBarrelIndicators)
            {
                gunBarrelIndicator.SetFloat(CurrentAmmo, num);
            }

            if(currentAmmo) currentAmmo.text = num.ToString();
        }
        
    }
}
