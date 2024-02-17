using System.Collections;
using MikroFramework;
using MikroFramework.Pool;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Factions;
using Runtime.Utilities.Collision;
using Runtime.Weapons.ViewControllers.Base;
using UnityEngine;
public class WormBossMissile : AbstractBulletViewController
{
    
    public GameObject particlePrefab;
    private SafeGameObjectPool pool;
    private GameObject particleInstance;
   
    private Rigidbody rb;
    private ParticleSystem trail;
    private Vector3 deviatedPrediction , standardPrediction;
    private float maxTimePrediction = 2f;
    //needs to be in customizable via sheets
    private float timer = 10f;
    private Transform player;
    public GameObject explosion;
    private int explosionDamage;
    private float explosionSize;
    private float bulletSpeed;
    private float maxAngle;
    // Start is called before the first frame update
    void Start()
    {
        
        //pool = GameObjectPoolManager.Singleton.CreatePool(particlePrefab, 30, 50);
        trail = this.transform.GetChild(1).gameObject.GetComponent<ParticleSystem>();
        trail.Play();
        rb = this.gameObject.GetComponent<Rigidbody>();

        
    }
    public void Setup(float timer, Transform player, float maxAngle, float bulletSpeed)
    {
        this.timer = timer;
        this.player = player;
        this.maxAngle = 130;
        this.bulletSpeed = bulletSpeed + 30;
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        //transform.Rotate(0, 10, 0);
        rb.velocity = transform.forward * bulletSpeed;
        var leadTimePercentage = Mathf.InverseLerp(10, 100, Vector3.Distance(transform.position, player.transform.position));
        PredictMovement(leadTimePercentage);
        AddDeviation(leadTimePercentage);
        Rotate();


    }
    private void PredictMovement(float leadTimePercentage)
    {
        var predictionTime = Mathf.Lerp(0, maxTimePrediction, leadTimePercentage);
        var playerRb = player.gameObject.GetComponent<Rigidbody>();
        standardPrediction = player.position + playerRb.velocity * predictionTime;
    }

    private void AddDeviation(float leadTimePercentage)
    {
        var deviation = new Vector3(Mathf.Cos(Time.time * 2), 0, 0);

        var predictionOffset = transform.TransformDirection(deviation) * 50 * leadTimePercentage;

        deviatedPrediction = standardPrediction + predictionOffset;
    }

    private void Rotate()
    {
        var heading = deviatedPrediction - transform.position;

        var rotation = Quaternion.LookRotation(heading);
        rb.MoveRotation(Quaternion.RotateTowards(transform.rotation, rotation, 130 * Time.deltaTime));
    }

    protected override void OnHitResponse(HitData data)
    {
        // throw new System.NotImplementedException();
    }

    protected override void OnHitObject(Collider other)
    {
        Explode();
        Debug.Log("Der");
    }

    protected override void OnBulletReachesMaxRange()
    {
        // throw new System.NotImplementedException();
    }

    protected override void OnBulletRecycled()
    {
        StopAllCoroutines();
    }
    void Explode()
    {
        SafeGameObjectPool pool = GameObjectPoolManager.Singleton.CreatePool(explosion, 10, 30);
        GameObject exp = pool.Allocate();

        exp.transform.position = transform.position;
        exp.transform.rotation = Quaternion.identity;
        exp.GetComponent<IExplosionViewController>().Init(Faction.Explosion, explosionDamage, 2, bulletOwner, bulletOwner.GetComponent<ICanDealDamage>());
    }
}
