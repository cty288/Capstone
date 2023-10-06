using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public enum CameraShakeBlendType {
   Maximum,
   Override
}

public struct CameraShakeData {
   public float Strength;
   public float Duration;
   public int Viberato;
   public float Randomness;
   
   public CameraShakeData(float strength, float duration, int viberato = 10, float randomness = 90f) {
      Strength = strength;
      Duration = duration;
      Viberato = viberato;
      Randomness = randomness;
   }
   
   public CameraShakeData Blend(CameraShakeData other, CameraShakeBlendType blendType) {
      switch (blendType) {
         case CameraShakeBlendType.Maximum:
            return new CameraShakeData(
               Mathf.Max(Strength, other.Strength),
               Strength > other.Strength ? Duration : other.Duration,
               Mathf.Max(Viberato, other.Viberato),
               Mathf.Max(Randomness, other.Randomness));
         case CameraShakeBlendType.Override:
            return other;
         default:
            throw new ArgumentOutOfRangeException(nameof(blendType), blendType, null);
      }
   }
}
public class CameraShaker : MonoBehaviour {
   private Tweener shakeTweener = null;

   private void Awake() {
      
   }
   
   
   public void Shake(CameraShakeData data, CameraShakeBlendType blendType = CameraShakeBlendType.Maximum) {
      if (shakeTweener != null) {
         shakeTweener.Kill(true);
      }

      data = data.Blend(data, blendType);

      shakeTweener = transform.DOShakePosition(data.Duration, data.Strength, data.Viberato, data.Randomness
         , false, true);
   }
}
