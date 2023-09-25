using System.Collections;
using Runtime.Utilities.Collision;
using UnityEngine;

namespace Runtime.Temporary
{
    public class EnemyBomb : MonoBehaviour
    {
        public AnimationCurve curve;
        private Vector3 targetPos;
        public float travelTime;
        private Vector3 start;

        private HitBox hb;
        // Start is called before the first frame update
        void Start()
        {
            start = transform.position;
            StartCoroutine(Curve());
            hb = GetComponent<HitBox>();

        }

        // Update is called once per frame
        void Update()
        {

        }
        public void Init(Transform target,float tTime)
        {
            targetPos = target.position;
            travelTime = tTime;
        }

        IEnumerator Curve()
        {
            float time = 0f;
 
            Vector3 end = targetPos - (transform.forward * 0.55f); // lead the target a bit to account for travel time, your math will vary

            while(time < travelTime)
            {
                time += Time.deltaTime;
 
                float linearT = time / travelTime;
                float heightT = curve.Evaluate(linearT);
 
                float height = Mathf.Lerp(0f, 5.0f, heightT); // change 3 to however tall you want the arc to be
 
                transform.position = Vector3.Lerp(start, end, linearT) + new Vector3(0f, height,0f);
 
                yield return null;
            }

        }
    }
}