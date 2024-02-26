using System.Collections;
using MikroFramework;
using MikroFramework.Pool;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Factions;
using Runtime.Utilities.Collision;
using Runtime.Weapons.ViewControllers.Base;
using UnityEngine;
using UnityEngine.Serialization;

public class WormBossMissile : AbstractBulletViewController
{
    public Rigidbody rb;
    public ParticleSystem streaksTrail;
    public ParticleSystem smokeTrail;

    public GameObject particlePrefab;
    public GameObject explosionPrefab;
    private SafeGameObjectPool explosionPool;
    private GameObject particleInstance;
   
    private Transform player;
    private Rigidbody playerRb;
    
    private Vector3 deviatedPrediction , standardPrediction;
    private float maxTimePrediction = 2f;
    
    private float missileLifetime;
    private float bulletSpeed;
    private float maxAngle;
    private int explosionDamage;
    private float explosionSize;

    private void Start()
    {
        explosionPool = GameObjectPoolManager.Singleton.CreatePool(explosionPrefab, 10, 30);
        streaksTrail.Play();
        smokeTrail.Play();
    }
    
    public void SetData(float missileLifetime, Transform player, float maxAngle, float bulletSpeed, 
        int explosionDamage, float explosionSize)
    {
        this.player = player;
        playerRb = player.gameObject.GetComponent<Rigidbody>();
        
        this.missileLifetime = missileLifetime;
        this.maxAngle = maxAngle;
        this.bulletSpeed = bulletSpeed;
        this.explosionDamage = explosionDamage;
        this.explosionSize = explosionSize;
    }

    protected override void Update()
    {
        base.Update();
        
        missileLifetime -= Time.deltaTime;
        if (missileLifetime <= 0)
        {
            Explode();
        }
    }

    private void FixedUpdate()
    {
        rb.velocity = transform.forward * bulletSpeed;
        var leadTimePercentage = Mathf.InverseLerp(10, 100, Vector3.Distance(transform.position, player.transform.position));
        PredictMovement(leadTimePercentage);
        AddDeviation(leadTimePercentage);
        Rotate();
    }
    
    private void PredictMovement(float leadTimePercentage)
    {
        var predictionTime = Mathf.Lerp(0, maxTimePrediction, leadTimePercentage);
        standardPrediction = player.position + playerRb.velocity * predictionTime;
    }

    private void AddDeviation(float leadTimePercentage)
    {
        var deviation = new Vector3(Mathf.Cos(Time.time * 2), 0, 0);

        var predictionOffset = transform.TransformDirection(deviation) * (50 * leadTimePercentage);

        deviatedPrediction = standardPrediction + predictionOffset;
    }

    private void Rotate()
    {
        var heading = deviatedPrediction - transform.position;

        var rotation = Quaternion.LookRotation(heading);
        rb.MoveRotation(Quaternion.RotateTowards(transform.rotation, rotation, maxAngle * Time.deltaTime));
    }

    protected override void OnHitResponse(HitData data)
    {
    }

    protected override void OnHitObject(Collider other)
    {
        Explode();
        Debug.Log("Der");
    }

    protected override void OnBulletReachesMaxRange()
    {
    }

    protected override void OnBulletRecycled()
    {
        StopAllCoroutines();
    }
    private void Explode()
    {
        GameObject exp = explosionPool.Allocate();

        exp.transform.position = transform.position;
        exp.transform.rotation = Quaternion.identity;
        exp.GetComponent<IExplosionViewController>().Init(Faction.Explosion, explosionDamage, explosionSize, bulletOwner, bulletOwner.GetComponent<ICanDealDamage>());
        RecycleToCache();
    }
}
