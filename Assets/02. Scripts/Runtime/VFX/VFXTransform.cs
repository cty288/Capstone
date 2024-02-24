using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VFXTransform : MonoBehaviour
{
    public Vector3 targetSize; // The target size you want to reach
    public float duration; // The duration over which you want to increase the size

    public Vector3 initialSize; // The initial size of the object
    public float timer = 0f; // Timer to track the progress
    private float saveTime;
    // Start is called before the first frame update
    void Start()
    {
        initialSize = transform.localScale; // Record the initial size
        saveTime = timer;
    }

    private void OnEnable()
    {
        
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
        
    }
    private void OnDisable()
    {
        timer = saveTime;
    }
}
