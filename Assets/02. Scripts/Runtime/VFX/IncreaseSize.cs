using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IncreaseSize : MonoBehaviour
{
    public Vector3 targetSize; // The target size you want to reach
    public float duration; // The duration over which you want to increase the size

    private Vector3 initialSize; // The initial size of the object
    private float timer = 0f; // Timer to track the progress

    // Start is called before the first frame update
    void Start()
    {
        initialSize = transform.localScale; // Record the initial size
    }

    // Update is called once per frame
    void Update()
    {
        // Increment timer
        timer += Time.deltaTime;

        // Calculate the progress
        float progress = Mathf.Clamp01(timer / duration);

        // Interpolate between initial size and target size based on progress
        transform.localScale = Vector3.Lerp(initialSize, targetSize, progress);

        // If the target size is reached, reset the timer
        if (progress >= 1f)
        {
            timer = 0f;
        }
    }
}
