using System;
using System.Collections;
using System.Collections.Generic;
using MikroFramework;
using MikroFramework.Pool;
using MikroFramework.Utilities;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Factions;
using Runtime.DataFramework.ViewControllers.Entities;
using Runtime.Weapons.ViewControllers.Base;
using UnityEngine;

public class ThrownGrenadeViewController : MonoBehaviour
{
    private Faction faction;
    private int damage;
    private float size;
    private GameObject bulletOwner;
    private ICanDealDamage owner;
    private Collider[] ignoreColliders;

    private Collider[] selfColliders;
   // private SphereCollider explosionCollider;
    
    [SerializeField] private float explosionDelay = 2f;
    [SerializeField] private LayerMask detectionLayer;
    [SerializeField] private GameObject explosion;
    
    private bool explosionDelayStarted = false;
    private SafeGameObjectPool pool;
    private void Awake() {
        selfColliders = GetComponentsInChildren<Collider>(true);
       // explosionCollider = GetComponent<SphereCollider>();
        pool = GameObjectPoolManager.Singleton.CreatePool(explosion, 20, 50);
    }


    private void OnCollisionEnter(Collision other) {
        if (!CheckCollider(other.collider)) {
            return;
        }
        if(PhysicsUtility.IsInLayerMask(other.collider.gameObject, detectionLayer) && !explosionDelayStarted) {
            explosionDelayStarted = true;
            StartCoroutine(ExplosionDelay(explosionDelay));
        }
        
        if (other.gameObject.TryGetComponent<ICreatureViewController>(out var creatureVC) || other.collider.gameObject.layer == LayerMask.NameToLayer("Hurtbox")) {
            Explode();
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (!CheckCollider(other)) {
            return;
        }
        
        if (other.gameObject.TryGetComponent<ICreatureViewController>(out var creatureVC) || other.gameObject.layer == LayerMask.NameToLayer("Hurtbox")) {
            Explode();
        }
    }

    private bool CheckCollider(Collider other) {
        if (other.gameObject.layer == LayerMask.NameToLayer("Hurtbox")) {
            return true;
        }

        return !other.isTrigger;
    }


    private IEnumerator ExplosionDelay(float time) {
        yield return new WaitForSeconds(time);
        Explode();
    }

    private void Explode() {
        
        GameObject explosion = pool.Allocate();
        explosion.transform.position = this.gameObject.transform.position;
        explosion.GetComponent<IExplosionViewController>().Init(Faction.Neutral, damage, size,
            null,
            owner);

        StopAllCoroutines();
        Destroy(gameObject);
    }

    public virtual void Init(Faction faction, int damage, float size, GameObject bulletOwner, ICanDealDamage owner,
        Collider[] ignoreColliders) {
        this.faction = faction;
        this.damage = damage;
        this.size = size;
        this.bulletOwner = bulletOwner;
        this.owner = owner;
        
        this.ignoreColliders = ignoreColliders;
        foreach (var selfCollider in selfColliders) {
            foreach (var ignoreCollider in ignoreColliders) {
                Physics.IgnoreCollision(selfCollider, ignoreCollider);
            }
        }

        //explosionCollider.radius = size;
    }
}
