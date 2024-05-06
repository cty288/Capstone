using System;
using UnityEngine;

namespace Mikrocosmos
{
    public class MenuCameraEffect : MonoBehaviour
    {
        public float sensitivity = 5f; // Adjust this value to make the camera more or less sensitive to mouse movement
        public Vector2 clampInDegrees = new Vector2(30, 30); // Adjust these values to limit the range of camera movement

        private Vector2 startingRotation;
        private Vector2 rotation;

        private void Start()
        {
            startingRotation = transform.localRotation.eulerAngles;
        }

        void Update()
        {
            Vector2 mouseDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
            rotation += mouseDelta * sensitivity;

            rotation.x = Mathf.Clamp(rotation.x, -clampInDegrees.x, clampInDegrees.x);
            rotation.y = Mathf.Clamp(rotation.y, -clampInDegrees.y, clampInDegrees.y);

            transform.localRotation = Quaternion.Euler(startingRotation.x-rotation.y, startingRotation.y + rotation.x, 0);
        }
    }
}