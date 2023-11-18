using System;
using System.Collections;
using System.Collections.Generic;
using _02._Scripts.Runtime.Player.Commands;
using DG.Tweening;
using Mikrocosmos;
using MikroFramework.Architecture;
using MikroFramework.Event;
using Runtime.Player;
using Runtime.Utilities;
using UnityEngine;

public class PlayerImageEffectController : EntityAttachedViewController<PlayerEntity> {
	private Material glitchMaterial;
	private Material bloodMaterial;
	
	
	
	private Dictionary<Material, Dictionary<string, float>> materialPropertyInitialValues =
		new Dictionary<Material, Dictionary<string, float>>();
	
	private Dictionary<Material, Dictionary<string, Color>> materialPropertyInitialColors =
		new Dictionary<Material, Dictionary<string, Color>>();
	
	[SerializeField] private AnimationCurve initialHurtEffectSizeCurve = null;
	[SerializeField] private float maxInitialGlitchIntensity = 0.2f;
	

	private float additionalGlitchIntensity = 0f;
	private float additionalRGBGlitchIntensity = 0f;
	private float additioanlHurtEffectSize = 0f;
	private float currentInitialHurtEffectSize = 0f;

	

	[SerializeField]
	private float animSpeed = 0.5f;

	[SerializeField] private float maxHurtEffectSizeWhenTakeDamage = 1f;
	
	
	protected override void Awake() {
		base.Awake();
		

		
	}


	private void Start() {
		glitchMaterial = ImageEffectController.Singleton.GetScriptableRendererFeatureMaterial(5);
		bloodMaterial =  ImageEffectController.Singleton.GetScriptableRendererFeatureMaterial(6);
		
		
		this.RegisterEvent<OnPlayerTakeDamage>(OnPlayerTakeDamage).UnRegisterWhenGameObjectDestroyed(gameObject);
		this.RegisterEvent<OnPlayerDie>(OnPlayerDie).UnRegisterWhenGameObjectDestroyed(gameObject);
		this.RegisterEvent<OnPlayerRespawn>(OnPlayerRespawn)
			.UnRegisterWhenGameObjectDestroyedOrRecycled(gameObject);
		/*glitchMaterial = Instantiate(glitchMaterial); 
		bloodMaterial = Instantiate(bloodMaterial);

		((ImageEffectController.Singleton.RenderData.rendererFeatures[5]) as SandstormRendererFeature).Material =
			glitchMaterial;

		((ImageEffectController.Singleton.RenderData.rendererFeatures[6]) as SandstormRendererFeature).Material =
			bloodMaterial;
		
		((ImageEffectController.Singleton.FPSRendererData.rendererFeatures[2]) as SandstormRendererFeature).Material =
			glitchMaterial;

		((ImageEffectController.Singleton.FPSRendererData.rendererFeatures[3]) as SandstormRendererFeature).Material =
			bloodMaterial;
		
		this.RegisterEvent<OnPlayerTakeDamage>(OnPlayerTakeDamage).UnRegisterWhenGameObjectDestroyed(gameObject);
		this.RegisterEvent<OnPlayerDie>(OnPlayerDie).UnRegisterWhenGameObjectDestroyed(gameObject);
		this.RegisterEvent<OnPlayerRespawn>(OnPlayerRespawn)
			.UnRegisterWhenGameObjectDestroyedOrRecycled(gameObject);*/
	}


	private void OnPlayerTakeDamage(OnPlayerTakeDamage e) {
		
		float maxHealth = boundEntity.GetMaxHealth();
		
		float damagePercentage = (float) e.DamageTaken / maxHealth;
		additionalGlitchIntensity += Mathf.Lerp(0, 0.3f, damagePercentage);
		additionalRGBGlitchIntensity += Mathf.Lerp(0, 0.002f, damagePercentage);
		additioanlHurtEffectSize += Mathf.Lerp(0, maxHurtEffectSizeWhenTakeDamage, damagePercentage);
	}

	protected override void OnEntityFinishInit(PlayerEntity entity) {
		
	}

	private void Update() {
		if (boundEntity.GetCurrentHealth() > 0) {
			float healthPercentage = (float) boundEntity.GetCurrentHealth() / boundEntity.GetMaxHealth();
			float hurtPercentage = 1 - healthPercentage;
		
			float hurtEffectSize = initialHurtEffectSizeCurve.Evaluate(hurtPercentage);
			additioanlHurtEffectSize = Mathf.Lerp(additioanlHurtEffectSize, 0, Time.deltaTime * animSpeed);
			additioanlHurtEffectSize = Mathf.Min(additioanlHurtEffectSize, 2f);
			hurtEffectSize += additioanlHurtEffectSize;
		
		
		
		
			currentInitialHurtEffectSize = Mathf.Lerp(currentInitialHurtEffectSize, hurtEffectSize, Time.deltaTime * animSpeed);
			
			SetValue(bloodMaterial, "_VignetteSize", currentInitialHurtEffectSize);


			float glitchingIntensity = Mathf.Lerp(0, maxInitialGlitchIntensity, Mathf.Max(hurtPercentage - 0.2f, 0));
			additionalGlitchIntensity = Mathf.Lerp(additionalGlitchIntensity, 0, Time.deltaTime * animSpeed);
			glitchingIntensity = Mathf.Min(glitchingIntensity + additionalGlitchIntensity, 0.8f);
		
			additionalRGBGlitchIntensity = Mathf.Lerp(additionalRGBGlitchIntensity, 0, Time.deltaTime * animSpeed);
			additionalGlitchIntensity = Mathf.Min(additionalGlitchIntensity, 0.05f);


			SetValue(glitchMaterial, "_Intensity", glitchingIntensity);
			SetValue(glitchMaterial, "_RGBGlichingIntensity", additionalRGBGlitchIntensity);
			SetColor(bloodMaterial, "_VignetteColor", Color.red, 0f);
		}
		


	}

	private void SetValue(Material material, string propertyName, float value) {
		if (!materialPropertyInitialValues.ContainsKey(material)) {
			materialPropertyInitialValues.Add(material, new Dictionary<string, float>());
		}
		
		if (!materialPropertyInitialValues[material].ContainsKey(propertyName)) {
			materialPropertyInitialValues[material].Add(propertyName, material.GetFloat(propertyName));
		}
		
		material.SetFloat(propertyName, value);
	}
	
	private void OnPlayerDie(OnPlayerDie obj) {
		SetValue(glitchMaterial, "_Intensity", 1, 5f);
		SetValue(glitchMaterial, "_RGBGlichingIntensity", 0.3f, 5f);
		
		SetValue(bloodMaterial, "_TotalPower", 5f, 8f);
		SetColor(bloodMaterial, "_VignetteColor", Color.black,  8f);
	}
	
	private void OnPlayerRespawn(OnPlayerRespawn obj) {
		SetValue(glitchMaterial, "_Intensity", 0, 2f);
		SetValue(glitchMaterial, "_RGBGlichingIntensity", 0f, 2f);
		
		SetValue(bloodMaterial, "_TotalPower", 1f, 2f);
		SetColor(bloodMaterial, "_VignetteColor", Color.red,  2f);
	}
	
	private Tween SetValue(Material material, string propertyName, float value, float time) {
		if (!materialPropertyInitialValues.ContainsKey(material)) {
			materialPropertyInitialValues.Add(material, new Dictionary<string, float>());
		}
		
		if (!materialPropertyInitialValues[material].ContainsKey(propertyName)) {
			materialPropertyInitialValues[material].Add(propertyName, material.GetFloat(propertyName));
		}

		material.DOKill();
		return material.DOFloat(value, propertyName, time);
		//return DOTween.To(() => material.GetFloat(propertyName), x => material.SetFloat(propertyName, x), value, time);
	}
	
	private Tween SetColor(Material material, string propertyName, Color value, float time) {
		if (!materialPropertyInitialColors.ContainsKey(material)) {
			materialPropertyInitialColors.Add(material, new Dictionary<string, Color>());
		}
		
		if (!materialPropertyInitialColors[material].ContainsKey(propertyName)) {
			materialPropertyInitialColors[material].Add(propertyName, material.GetColor(propertyName));
		}

		material.DOKill();
		return material.DOColor(value, propertyName, time);
		
		//return DOTween.To(() => material.GetColor(propertyName), x => material.SetColor(propertyName, x), value, time);
	}

	private void OnDestroy() {
		ResetProperty();
	}

	private void OnApplicationQuit() {
		ResetProperty();
	}

	private void ResetProperty() {
		foreach (var material in materialPropertyInitialValues.Keys) {
			foreach (var property in materialPropertyInitialValues[material].Keys) {
				material.SetFloat(property, materialPropertyInitialValues[material][property]);
			}
		}
		
		foreach (var material in materialPropertyInitialColors.Keys) {
			foreach (var property in materialPropertyInitialColors[material].Keys) {
				material.SetColor(property, materialPropertyInitialColors[material][property]);
			}
		}
	}
	
	
}
