using Runtime.Utilities.Collision;
using Runtime.Weapons.ViewControllers.Base;
using MikroFramework.Pool;
using MikroFramework;
using UnityEngine;
using Runtime.DataFramework.Entities.ClassifiedTemplates.Damagable;


public class BladeSentinalBladeDanmaku : AbstractBulletViewController
{
    private float bulletSpeed;
    public GameObject particlePrefab;
    private SafeGameObjectPool pool;
    private GameObject particleInstance;
    private GameObject childToRotate;
    private float rotationSpeed = 90f;
    private float afterBulletSpeed;
    private float afterRotationSpeed;
    private bool secondPhase = false;
    private Rigidbody rb;
    private Transform rotateAroundPoint;
    protected override void OnBulletReachesMaxRange()
    {
        
    }

    protected override void OnBulletRecycled()
    {
        secondPhase = false;
    }

    protected override void OnHitObject(Collider other)
    {
        
    }

    protected override void OnHitResponse(HitData data)
    {
       
    }
    public void SetData(float bulletSpeed , float initTime, float afterBulletSpeed , float afterRotationSpeed , Transform origin)
    {
     
        
        this.bulletSpeed = bulletSpeed;
        this.afterRotationSpeed = afterRotationSpeed;
        this.afterBulletSpeed = afterBulletSpeed;
        Invoke("SetSpeed", initTime);
        Invoke("SetRotationSpeed", initTime);
        Invoke("SetPhase", 6.45f);
        rotateAroundPoint = origin;

    }

    // Start is called before the first frame update
    void Start()
    {
        rb = this.gameObject.GetComponent<Rigidbody>();
        childToRotate = this.gameObject.transform.GetChild(0).gameObject;
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
        if (!secondPhase)
        {

            this.gameObject.GetComponent<Rigidbody>().velocity = this.gameObject.transform.forward * bulletSpeed;
            childToRotate.transform.Rotate(0, 360 * Time.deltaTime, 0);
            this.gameObject.transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);
        }
        else
        {

           
            //rb.velocity = Vector3.zero;
        }
    }
    private void FixedUpdate()
    {
        if (secondPhase)
        {

            this.gameObject.transform.RotateAround(rotateAroundPoint.position, Vector3.up, 20 * Time.deltaTime);


            if(rb.velocity.magnitude < 0.05f)
            {
                rb.velocity = Vector3.zero;
            }
            else
            {
                float dampingFactor = 0.98f; // You can adjust this value based on your preference

                // Apply a force in the opposite direction of the current velocity
                rb.AddForce(-rb.velocity * dampingFactor, ForceMode.Acceleration);
            }
            
            Quaternion deltaQuat = Quaternion.FromToRotation(this.gameObject.transform.forward, Vector3.up);

            Vector3 axis;
            float angle;
            deltaQuat.ToAngleAxis(out angle, out axis);

            float dampenFactor = 0.8f; // this value requires tuning
            rb.AddTorque(-rb.angularVelocity * dampenFactor, ForceMode.Acceleration);

            float adjustFactor = 0.5f; // this value requires tuning
            rb.AddTorque(axis.normalized * angle * adjustFactor, ForceMode.Acceleration);
        }
    }
    private void SetSpeed()
    {
        this.bulletSpeed = afterBulletSpeed;
    }
    private void SetRotationSpeed()
    {
        this.rotationSpeed = afterRotationSpeed;
    }
    private void SetPhase()
    {
        secondPhase = true;
    }
}
