using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
using UnityEngine;

namespace SCPE
{
    public class PixelizeRenderer : ScriptableRendererFeature
    {
        class PixelizeRenderPass : PostEffectRenderer<Pixelize>
        {
            public PixelizeRenderPass(EffectBaseSettings settings)
            {
                this.settings = settings;
                shaderName = ShaderNames.Pixelize;
                ProfilerTag = GetProfilerTag();
            }

            public void Setup(ScriptableRenderer renderer)
            {
                this.cameraColorTarget = GetCameraTarget(renderer);
                volumeSettings = VolumeManager.instance.stack.GetComponent<Pixelize>();
                
                if(volumeSettings && volumeSettings.IsActive()) renderer.EnqueuePass(this);
            }

            protected override void ConfigurePass(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
            {
                if (!volumeSettings) return;

                base.ConfigurePass(cmd, cameraTextureDescriptor);
            }
            
            private static readonly int _Scale = Shader.PropertyToID("_Scale");
            private static readonly int _PixelScale = Shader.PropertyToID("_PixelScale");
            private static readonly int _Resolution = Shader.PropertyToID("_Resolution");

            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                if (ShouldRender(renderingData) == false) return;

                var cmd = GetCommandBuffer(ref renderingData);

                CopyTargets(cmd, renderingData);
                
                var resolution = volumeSettings.resolutionPreset.value == Pixelize.Resolution.Custom ? volumeSettings.resolution.value : (int)volumeSettings.resolutionPreset.value;

                Material.SetFloat(_Scale, volumeSettings.amount.value);
                Material.SetFloat(_PixelScale, renderingData.cameraData.camera.scaledPixelHeight / (float)resolution);
                Material.SetInt(_Resolution, (int)resolution);

                FinalBlit(this, context, cmd, renderingData, 0);
            }
        }

        PixelizeRenderPass m_ScriptablePass;

        [SerializeField]
        public EffectBaseSettings settings = new EffectBaseSettings();

        public override void Create()
        {
            m_ScriptablePass = new PixelizeRenderPass(settings);
            m_ScriptablePass.renderPassEvent = settings.injectionPoint;
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            m_ScriptablePass.Setup(renderer);
        }
    }
}