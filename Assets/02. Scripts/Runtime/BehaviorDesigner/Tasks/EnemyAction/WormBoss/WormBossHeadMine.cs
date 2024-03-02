using System;
using System.Collections;
using System.Threading;
using Cysharp.Threading.Tasks;
using MikroFramework;
using MikroFramework.Pool;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Factions;
using Runtime.Utilities.Collision;
using Runtime.Weapons.ViewControllers.Base;
using UnityEngine;
using UnityEngine.Serialization;
using DG.Tweening;
using Runtime.DataFramework.ViewControllers.Entities;


public class WormBossHeadMine : AbstractBulletViewController
{
    private Rigidbody rb;
    
    private Vector3 target;
    private Vector3 randomSpinDir;
    private float speed;
    private float explosionTimer = 1f;
    
    private bool onGround;
    private bool isExploding;

    [FormerlySerializedAs("explosion")] public GameObject explosionPrefab;
    private SafeGameObjectPool explosionPool;

    private CancellationTokenSource cancellationToken;
    
    // private Sequence animationSequence;

    void Start()
    {
        autoRecycleWhenHit = false;
        
        onGround = false;
        rb = GetComponent<Rigidbody>();
        randomSpinDir = UnityEngine.Random.insideUnitSphere;
        randomSpinDir.Normalize();
        
        cancellationToken = new CancellationTokenSource();
        explosionPool = GameObjectPoolManager.Singleton.CreatePool(explosionPrefab, 10,30);
    }

    protected override void Update()
    {
        //travel to ground
        if (!isExploding && !onGround)
        {
            Vector3 direction = (target - transform.position).normalized;

            // Calculate velocity using constant speed
            Vector3 velocity = direction * speed;

            // Update the bullet's velocity
            rb.velocity = velocity;

            // Check if the bullet has reached the target
            if (Vector3.Distance(target, transform.position) < 0.1f)
            {
                rb.velocity = Vector3.zero;
                onGround = true;
                StartExplosion();
            }
        }
    }

    protected override void OnBulletReachesMaxRange() { }

    protected override void OnBulletRecycled() { }

    protected override void OnHitObject(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            cancellationToken.Cancel();
            Explode();
        }
    }

    protected override void OnHitResponse(HitData data) { }
    
    private async UniTask StartExplosion()
    {
        isExploding = true;
        
        Sequence animationSequence = DOTween.Sequence();
        
        float timeBetweenPulses = explosionTimer;
        
        Tween rotate = transform.DORotate(randomSpinDir, 1f).SetEase(Ease.Linear);
        Tween moveUp = transform.DOMoveY(transform.position.y + 0.6f, 1f).SetEase(Ease.OutCubic);
        Tween initialPulse = transform.DOPunchScale(new Vector3(0.3f, 0.3f, 0.3f), 0.2f, 1).SetDelay(timeBetweenPulses);
        
        animationSequence.Append(rotate);
        animationSequence.Join(moveUp);
        animationSequence.Append(initialPulse);
        
        for(int i = 0; i < 9; i++)
        {
            timeBetweenPulses *= 0.8f;
            Tween pulse = transform.DOPunchScale(new Vector3(0.3f, 0.3f, 0.3f), 0.2f, 1).SetDelay(timeBetweenPulses);
            animationSequence.Append(pulse);
        }
        
        await animationSequence.Play().WithCancellation(cancellationToken: cancellationToken.Token);
        
        Explode();
    }


    internal void SetData(float bulletSpeed, Vector3 target)
    {
        speed = bulletSpeed;
        this.target = target;
    }


    void Explode()
    {
        GameObject exp = explosionPool.Allocate();

        //Instantiate(explosion,transform.position,Quaternion.identity);
        exp.transform.position = transform.position;
        exp.transform.rotation = Quaternion.identity;
        
        exp.GetComponent<IExplosionViewController>().Init(Faction.Explosion, 5, 0.3f, bulletOwner,
            bulletOwner.GetComponent<ICanDealDamage>());
        
        RecycleToCache();
    }
}
