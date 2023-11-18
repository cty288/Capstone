using System;
using System.Collections;
using System.Collections.Generic;
using Framework;
using MikroFramework.Architecture;
using MikroFramework.Event;
using MikroFramework.ResKit;
using MikroFramework.Singletons;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Mikrocosmos
{
    public class ImageEffectController : MonoMikroSingleton<ImageEffectController>, IController {
       
        
        private UniversalRendererData renderData;

        public UniversalRendererData RenderData => renderData;
        
       
        private UniversalRendererData fpsRendererData;

        public UniversalRendererData FPSRendererData => fpsRendererData;
        private ResLoader resLoader;

        private Dictionary<ScriptableRendererFeature, bool> isOnByDefault =
            new Dictionary<ScriptableRendererFeature, bool>();
        private void Awake() {
          
            resLoader = this.GetUtility<ResLoader>();
            renderData = resLoader.LoadSync<UniversalRendererData>("resources://Settings/URP-HighFidelity-Renderer");
            fpsRendererData = resLoader.LoadSync<UniversalRendererData>("resources://Settings/FPSArm-Renderer");
            //fpsRendererData = resLoader.LoadSync<UniversalRendererData>("FPSArm-Renderer");


            var features = renderData.rendererFeatures;
            foreach (ScriptableRendererFeature feature in features) {
               // SandstormRendererFeature sandstormRendererFeature = feature as SandstormRendererFeature;
                isOnByDefault.TryAdd(feature, feature.isActive);
            }
        }

        public void DisableAllFeatures() {
            
            Awake();
            var features = renderData.rendererFeatures;
            foreach (ScriptableRendererFeature feature in features)
            {
                
                feature.SetActive(false);
            }
        }
   

        public SandstormRendererFeature GetScriptableRendererFeature(int index) {
            Awake();
            return renderData.rendererFeatures[index] as SandstormRendererFeature;
        }
        public Material GetScriptableRendererFeatureMaterial(int index) {
            Awake();
            SandstormRendererFeature feature = renderData.rendererFeatures[index] as SandstormRendererFeature;
            return feature.Material;
        }
        public Material TurnOnScriptableRendererFeature(int index) {
            Awake();
            SandstormRendererFeature feature = renderData.rendererFeatures[index] as SandstormRendererFeature;
            feature.SetActive(true);
            return feature.Material;
        }

        public void TurnOffScriptableRendererFeature(int index) {
            renderData.rendererFeatures[index].SetActive(false);
        }

        
        protected override void OnApplicationQuit() {
            var features = renderData.rendererFeatures;
            foreach (ScriptableRendererFeature feature in features) {
                if (isOnByDefault.TryGetValue(feature, out var value)) {
                    feature.SetActive(value);
                }
            }
            //DisableAllFeatures();
            base.OnApplicationQuit();
            
        }

        
        public IArchitecture GetArchitecture() {
            return MainGame.Interface;
        }
    }
}
