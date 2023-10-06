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
        [SerializeField] private Material gunBarrelIndicator;
        private Material _instanceGunBarrelIndicator;
        private static readonly int MaxAmmo = Shader.PropertyToID("_MaxAmmo");
        private static readonly int CurrentAmmo = Shader.PropertyToID("_CurrentAmmo");

        
        void Awake() {
            MeshRenderer renderer = GetComponent<MeshRenderer>();
            if (renderer.material == null)
            {
                renderer.material = new Material(gunBarrelIndicator);
            }
            _instanceGunBarrelIndicator = renderer.material;
        }

        public void Init(IWeaponEntity entity) {
            _associatedWeapon = entity;
            _associatedWeapon.CurrentAmmo.RegisterWithInitValue(OnAmmoChanged)
                .UnRegisterWhenGameObjectDestroyedOrRecycled(transform.parent.gameObject);
            _instanceGunBarrelIndicator.SetInteger(MaxAmmo, _associatedWeapon.GetAmmoSize().RealValue);
        }

        public void OnAmmoChanged(int num)
        {
            _instanceGunBarrelIndicator.SetFloat(CurrentAmmo, num);
        }
        
    }
}
