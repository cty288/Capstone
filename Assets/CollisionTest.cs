using UnityEngine;

public class CollisionTest : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        // Check if the left mouse button is clicked
        if (Input.GetMouseButtonDown(0))
        {
            // Create a ray from the mouse position
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // Check if the ray hits any object
            if (Physics.Raycast(ray, out hit))
            {
                // Get the GameObject that was hit
                GameObject clickedObject = hit.collider.gameObject;

                // Do something with the clickedObject (e.g., print its name)
                Debug.Log("Clicked on object: " + clickedObject.name);

                // You can also return the clickedObject or perform any other actions here
                // return clickedObject;
            }
        }
    }
}
