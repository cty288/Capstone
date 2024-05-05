using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.VFX;

public class ScreenSpaceVFXManager : MonoBehaviour
{
    [SerializeField] private float instantBuffSpeed = 3f;
    [SerializeField] private float instantBuffLingerSpeed = 0.5f;
    [SerializeField] private BuffEffect buffEffect;

    private List<BuffEffect> _buffEffectStack;

    public static ScreenSpaceVFXManager Instance
    {
        get
        {
            if (_instance != null)
            {
                return _instance;
            }

            var obj = FindObjectOfType<ScreenSpaceVFXManager>();
            if (obj == null)
            {
                return null;
            }
            
            _instance = obj;
            return obj;
        }
    }
    private static ScreenSpaceVFXManager _instance;


    [SerializeField] private Volume vol;
    public void Start()
    {
        UnityEngine.Rendering.VolumeProfile volumeProfile = GetComponent<UnityEngine.Rendering.Volume>()?.profile;
        if(!volumeProfile) throw new System.NullReferenceException(nameof(UnityEngine.Rendering.VolumeProfile));
 
        if(!volumeProfile.TryGet(out buffEffect)) throw new System.NullReferenceException(nameof(buffEffect));
    }

    public bool PlayHeal(Color color)
    {
        buffEffect.colorHeal.value = color;
        buffEffect.healToggle.value = 1;
        StartCoroutine(Heal());
        return true;
    }

    public IEnumerator Heal()
    {
        float progress = 0f;

        while (progress < 1)
        {
            progress += Time.deltaTime * instantBuffSpeed;
            var t = easeInOutQuint(progress);
            //Do something with t
            buffEffect.healToggle.value = t;
            
            yield return null;
        }

        progress = 0f;
        while (progress < 1)
        {
            progress += Time.deltaTime * instantBuffLingerSpeed;
            var t = easeOutSine(progress);
            //Do something with t
            buffEffect.healToggle.value = 1-t;
            
            yield return null;
        }
        
        buffEffect.healToggle.value = 0;
        
        StopCoroutine(Heal());
    }
    
    
    public bool SetBuff(Color color, bool toggle, int buffIndex){
        if (buffIndex == 0)
        {
            Buff(toggle, buffIndex);
            return true;
        }
        else if(buffIndex == 1)
        {
            Buff(toggle, buffIndex);
            return true;
        }
        else
        {
            return false;
        }
    }

    public IEnumerator Buff(bool toggle, int i)
    {
        float progress = 0f;

        if (toggle)
        {
            while (progress < 1)
            {
                progress += Time.deltaTime;
                var t = easeOutElastic(progress);
                //Do something with t
            
            
                yield return null;
            }
        }
        else
        {

            while (progress < 1)
            {
                progress += Time.deltaTime;
                var t = easeOutSine(progress);
                //Do something with t


                yield return null;
            }
        }

        StopCoroutine(Buff(toggle, i));
    }
    
    private float easeInOutQuint(float x) {
        return x < 0.5 ? 16 * x * x * x * x * x : 1 - Mathf.Pow(-2 * x + 2, 5) / 2;
    }
    
    private float easeOutSine(float x) {
        return Mathf.Sin((x * Mathf.PI) / 2);

    }
    
    private float easeOutElastic(float x){
        const float c4 = (2 * Mathf.PI) / 3;

        return x <= 0
            ? 0
            : x >= 1
                ? 1
                : Mathf.Pow(2, -10 * x) * Mathf.Sin((x * 10 - 0.75f) * c4) + 1;

    }
}