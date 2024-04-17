using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace _02._Scripts.Runtime.TimeSystem
{
    public class WindUtility : MonoBehaviour
    {
        private Camera _cam;
        private Volume _vol;
        private SandstormEffect _sandstorm;
        private bool _doRun = false;
        
        // Start is called before the first frame update
        void Start()
        {
            _vol = GetComponent<Volume>();
            _cam = Camera.main;
            if (_vol.profile.TryGet<SandstormEffect>(out _sandstorm))
            {
                _doRun = true;
            }
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            if (_doRun)
            {
                _sandstorm.cameraDirectionUtility.value = _cam.transform.forward;
            }
        }
    }
}
