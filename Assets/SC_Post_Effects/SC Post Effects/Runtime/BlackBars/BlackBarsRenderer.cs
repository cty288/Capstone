using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
using UnityEngine;

namespace SCPE
{
    public class BlackBarsRenderer : ScriptableRendererFeature
    {
        class BlackBarsRenderPass : PostEffectRenderer<BlackBars>
        {
            public BlackBarsRenderPass(EffectBaseSettings settings)
            {
                this.settings = settings;
                shaderName = ShaderNames.BlackBars;
                ProfilerTag = GetProfilerTag();
            }

            public void Setup(ScriptableRenderer renderer)
            {
                this.cameraColorTarget = GetCameraTarget(renderer);
                volumeSettings = VolumeManager.instance.stack.GetComponent<BlackBars>();
                
                if(volumeSettings && volumeSettings.IsActive()) renderer.EnqueuePass(this);
            }

            protected override void ConfigurePass(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
            {
                if (!volumeSettings) return;

                base.ConfigurePass(cmd, cameraTextureDescriptor);
            }

            public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
            {
                if (ShouldRender(renderingData) == false) return;

                var cmd = GetCommandBuffer(ref renderingData);

                CopyTargets(cmd, renderingData);

                Material.SetVector("_Size", new Vector2(volumeSettings.size.value / 10f, volumeSettings.maxSize.value * 5));

                FinalBlit(this, context, cmd, renderingData, (int)volumeSettings.mode.value);
            }
        }

        BlackBarsRenderPass m_ScriptablePass;
        [SerializeField]
        public EffectBaseSettings settings = new EffectBaseSettings(false);
        
        public override void Create()
        {
            m_ScriptablePass = new BlackBarsRenderPass(settings);
            m_ScriptablePass.renderPassEvent = settings.injectionPoint;
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            m_ScriptablePass.Setup(renderer);
        }
    }
}