using System;
using System.Collections;
using MikroFramework;
using MikroFramework.Pool;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Factions;
using Runtime.Utilities.Collision;
using Runtime.Weapons.ViewControllers.Base;
using UnityEngine;
using UnityEngine.Serialization;
using DG.Tweening;


public class WormBossHeadMine : AbstractBulletViewController
{
    private float speed;
    private float gravity;
    private Vector3 dir;
    private Rigidbody rb;
    private Vector3 target;
    private bool arrived = false;
    private Vector3 randomSpinDir;
    public Material material;
    private bool onGround = false;
    private float duration = 0.1f;
    private Vector3 punchAmount = new Vector3(1f, 1f, 1f);
    public float initialFrequency = 1f;
    public float frequencyIncreaseRate = 0.3f;
    private float currentFrequency;
    public AnimationCurve curve;
    private float timer = 0f;

    protected override void OnBulletReachesMaxRange()
    {

    }

    protected override void OnBulletRecycled()
    {

    }

    protected override void OnHitObject(Collider other)
    {

    }

    protected override void OnHitResponse(HitData data)
    {

    }

    // Start is called before the first frame update
    void Start()
    {
        onGround = false;
        arrived = false;
        rb = GetComponent<Rigidbody>();
        dir = target - this.transform.position;
        randomSpinDir = UnityEngine.Random.insideUnitSphere;
        randomSpinDir.Normalize();
        currentFrequency = initialFrequency;


    }


    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if (onGround)
        {
            onGround = false;
            StartCoroutine(ActivateMine());
            
            // Cancel the previous InvokeRepeating and set a new one with updated frequency
            
        }
        if (!arrived)
        {

            Vector3 direction = (target - transform.position).normalized;

            // Calculate velocity using constant speed
            Vector3 velocity = direction * 15;

            // Update the bullet's velocity
            this.rb.velocity = velocity;

            // Check if the bullet has reached the target
            if (Vector3.Distance(target, transform.position) < 0.1f)
            {
                arrived = true;
                this.rb.velocity = Vector3.zero;
                onGround = true;
            }
        }
        else
        {
            transform.Rotate(randomSpinDir, 15 * Time.deltaTime);
        }

    }
    IEnumerator ActivateMine()
    {
        
        float t = 0f;
        float ct = 0f;
        while(t < 9f)
        {
            var val = curve.Evaluate(0.1f) * 3;
            Debug.Log(val);
            if(ct > val)
            {
                Debug.Log("hi");
                ct = 0;
                transform.DOPunchScale(new Vector3(0.1f, 0.1f, 0.1f), 1f , 1, 1);
            }
            t += Time.deltaTime;
            ct += Time.deltaTime;
        }
        yield return null;
    }

    internal void SetData(float bulletSpeed, GameObject player, Vector3 target)
    {
        speed = bulletSpeed;
        dir = player.transform.position - this.transform.position;
        this.target = target;
    }
    void PunchScale()
    {
        transform.DOPunchScale(punchAmount, duration);
    }




}
