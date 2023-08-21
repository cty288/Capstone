using UnityEngine;

namespace Runtime.Temporary
{
    public class MeteorBehavior : MonoBehaviour
    {
        private float forceMagnitude = 10f;  // Adjust this value to control the speed of the meteors.
        private Vector3 direction;

        private Rigidbody rb;

        private void Start()
        {
            direction = new Vector3(Random.Range(0f, 1f), -1, Random.Range(0f, 1f));
            // Debug.Log(direction);
            rb = this.gameObject.GetComponent<Rigidbody>();
            rb.AddForce(direction.normalized * forceMagnitude, ForceMode.Impulse);
            // Debug.Log("end");
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.tag == "Ground")
            {
                GameObject.Destroy(this.gameObject);
            }
        }
    }
}
