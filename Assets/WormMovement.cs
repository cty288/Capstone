using UnityEngine;

public class WormMovement : MonoBehaviour
{
    public float moveSpeed = 5.0f; // Adjust the movement speed as needed.
    private Rigidbody rb;

    void Start()
    {
        // Get the Rigidbody component attached to the GameObject.
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        // Get input for movement in the horizontal (A/D) and vertical (W/S) axes.
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        // Calculate the movement direction based on input.
        Vector3 movementDirection = new Vector3(horizontalInput, 0.0f, verticalInput).normalized;

        // Check if there's input for movement.
        if (movementDirection != Vector3.zero)
        {
            // Rotate the character to face the movement direction.
            Quaternion targetRotation = Quaternion.LookRotation(movementDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10.0f);

            // Move the character in the calculated direction.
            Vector3 moveAmount = movementDirection * moveSpeed * Time.deltaTime;
            rb.MovePosition(transform.position + moveAmount);
        }
    }
}