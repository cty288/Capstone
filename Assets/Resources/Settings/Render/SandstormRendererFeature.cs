using UnityEngine;
using UnityEngine.Rendering.Universal;

public class SandstormRendererFeature : ScriptableRendererFeature
{

    [SerializeField] private Material material;

    public Material Material {
        get => material;
        set => material = value;
    }

    

    SandstormRenderPass m_ScriptablePass;
    
    
    public override void Create()
    {
        m_ScriptablePass = new SandstormRenderPass(material);
        m_ScriptablePass.renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
    }

    // Here you can inject one or multiple render passes in the renderer.
    // This method is called when setting up the renderer once per-camera.
    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        m_ScriptablePass.source = renderer.cameraColorTarget;
        renderer.EnqueuePass(m_ScriptablePass);
    }
}


